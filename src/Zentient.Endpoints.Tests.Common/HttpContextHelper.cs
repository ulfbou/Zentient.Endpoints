// <copyright file="HttpContextHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using Zentient.Endpoints.Http.Mapping;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides helper methods for working with <see cref="HttpContext"/> in tests,
    /// including deserializing <see cref="ProblemDetails"/> from <see cref="ContentHttpResult"/>
    /// and creating mock <see cref="HttpContext"/> instances.
    /// </summary>
    internal static class HttpContextHelper
    {
        /// <summary>
        /// Executes a ContentHttpResult and deserializes the response body as ProblemDetails.
        /// </summary>
        /// <param name="contentResult">The ContentHttpResult to execute.</param>
        /// <param name="httpContext">The HttpContext with a writable Response.Body stream.</param>
        /// <param name="serializerOptions">The JsonSerializerOptions to use.</param>
        /// <returns>The deserialized ProblemDetails instance.</returns>
        public static async Task<ProblemDetails?> DeserializeProblemDetailsFromContentResultAsync(
            ContentHttpResult contentResult,
            HttpContext httpContext,
            JsonSerializerOptions serializerOptions)
        {
            // Defensive: If contentResult is null, throw with a clear message
            if (contentResult is null)
                throw new ArgumentNullException(nameof(contentResult), "ContentHttpResult must not be null. Ensure the result is of type ContentHttpResult before calling this helper.");

            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            ArgumentNullException.ThrowIfNull(serializerOptions, nameof(serializerOptions));

            EnsureResponseBodyIsMemoryStream(httpContext.Response.Body);
            EnsureResponseBodyIsWritable(httpContext.Response.Body);

            httpContext.Response.Body.SetLength(0);
            await contentResult.ExecuteAsync(httpContext).ConfigureAwait(false);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body, leaveOpen: true);
            string responseBody = await reader.ReadToEndAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<ProblemDetails>(responseBody, serializerOptions);
        }

        /// <summary>
        /// Creates a default <see cref="DefaultHttpContext"/> instance for testing purposes.
        /// Configures a basic request path and an in-memory response body.
        /// Returns both the context and the response stream for test inspection.
        /// </summary>
        /// <param name="jsonSerializerOptions">Optional JsonSerializerOptions to register in the service provider.</param>
        /// <returns>A tuple of the <see cref="DefaultHttpContext"/> and its <see cref="MemoryStream"/> response body.</returns>
        public static (DefaultHttpContext HttpContext, MemoryStream ResponseStream) CreateHttpContext(
            JsonSerializerOptions? jsonSerializerOptions = null) // Added optional parameter
        {
            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            // Set a default TraceIdentifier, as it's often used in ProblemDetails
            httpContext.TraceIdentifier = Guid.NewGuid().ToString();

            // Optional: Set a default request path/instance for ProblemDetails
            httpContext.Request.Path = "/mocked-test-path";

            // --- Crucial addition for "provider" issue ---
            // Create a basic service collection
            var services = new ServiceCollection();

            // Add a test logger factory
            // Assuming TestLoggerFactory is correctly implemented and can be instantiated this way.
            services.AddSingleton<ILoggerFactory>(sp => TestLoggerFactory.Instance);

            // Add a mock HttpContextFactory, as IResult implementations might try to resolve it.
            services.AddSingleton<IHttpContextFactory>(new Mock<IHttpContextFactory>().Object);

            // Add JsonSerializerOptions to the service provider, wrapped in IOptions<JsonOptions>.
            // JsonHttpResult (and other JSON-based results) will try to resolve this.
            services.AddOptions<JsonOptions>().Configure(options =>
            {
                // Use provided options, or a reasonable default if none are provided.
                if (jsonSerializerOptions is not null)
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
                    // Copy other relevant properties as needed
                }
                else
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    // Set other default settings if necessary for your application's JSON handling
                }
            });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Assign the service provider to the HttpContext.RequestServices
            httpContext.RequestServices = serviceProvider;
            // --- End of crucial addition ---

            return (httpContext, responseStream);
        }

        /// <summary>
        /// Creates a <see cref="DefaultHttpContext"/> instance with a mocked <see cref="IEndpointOutcomeToHttpMapper"/>
        /// registered in its service provider. Returns both the context and the response stream for test inspection.
        /// </summary>
        /// <param name="mapperMock">An optional pre-configured mock for <see cref="IEndpointOutcomeToHttpMapper"/>.</param>
        /// <returns>A tuple of the <see cref="DefaultHttpContext"/> and its <see cref="MemoryStream"/> response body.</returns>
        public static (DefaultHttpContext context, MemoryStream responseStream) CreateHttpContextWithMapper(Mock<IEndpointOutcomeToHttpMapper>? mapperMock = null)
        {
            // Call the base CreateHttpContext, which now handles the common service provider setup.
            // Note: If the mapper's JSON options are critical for this specific helper, you might need to pass them.
            var (context, responseStream) = CreateHttpContext();
            var services = new ServiceCollection(); // Create a new ServiceCollection for mapper-specific services

            mapperMock ??= new Mock<IEndpointOutcomeToHttpMapper>();
            services.AddSingleton(mapperMock.Object);

            // Merge services from the base context with mapper-specific services
            // This is a common pattern if you have multiple layers of DI setup.
            // For simplicity, if you only add the mapper here, you might just replace RequestServices.
            // However, if CreateHttpContext already built a provider, you should extend it.
            // A more robust way would be to pass the existing provider to a new ServiceCollection
            // or build a new one and overwrite. For now, let's assume overwriting is fine if
            // mapper is the only additional service needed.
            context.RequestServices = services.BuildServiceProvider();
            return (context, responseStream);
        }

        private static void EnsureResponseBodyIsMemoryStream(Stream stream)
        {
            if (stream is not MemoryStream)
            {
                throw new InvalidOperationException("HttpContext.Response.Body must be a MemoryStream for this operation.");
            }
        }

        private static void EnsureResponseBodyIsWritable(Stream stream)
        {
            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("HttpContext.Response.Body must be writable.");
            }
        }
    }
}