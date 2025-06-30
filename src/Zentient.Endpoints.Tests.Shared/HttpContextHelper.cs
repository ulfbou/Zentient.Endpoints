// <copyright file="HttpContextHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Zentient.Endpoints.Http.Mapping;

namespace Zentient.Endpoints.Tests.Shared
{
    /// <summary>
    /// Provides helper methods for working with <see cref="HttpContext"/> in tests,
    /// including deserializing <see cref="ProblemDetails"/> from <see cref="ContentHttpResult"/>
    /// and creating mock <see cref="HttpContext"/> instances.
    /// This class is intended for use in unit tests and should not be used in production code.
    /// It includes methods to create a <see cref="DefaultHttpContext"/> with a writable response body,
    /// and to deserialize <see cref="ProblemDetails"/> from a <see cref="ContentHttpResult"/>.
    /// </summary>
    public static class HttpContextHelper
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
            ArgumentNullException.ThrowIfNull(contentResult, nameof(contentResult));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            ArgumentNullException.ThrowIfNull(serializerOptions, nameof(serializerOptions));

            if (httpContext.Response.Body is not MemoryStream)
            {
                throw new InvalidOperationException("HttpContext.Response.Body must be a MemoryStream for this operation.");
            }

            if (!httpContext.Response.Body.CanWrite)
            {
                throw new InvalidOperationException("HttpContext.Response.Body must be writable.");
            }

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
        /// </summary>
        /// <returns>A new <see cref="DefaultHttpContext"/> instance.</returns>
        public static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/mocked-test-path";
            context.Response.Body = new MemoryStream();
            return context;
        }

        /// <summary>
        /// Creates a <see cref="DefaultHttpContext"/> instance with a mocked <see cref="IEndpointOutcomeToHttpMapper"/>
        /// registered in its service provider. This is useful for testing scenarios where the mapper is resolved from services.
        /// </summary>
        /// <param name="mapperMock">An optional pre-configured mock for <see cref="IEndpointOutcomeToHttpMapper"/>.</param>
        /// <returns>A new <see cref="DefaultHttpContext"/> instance with a service provider.</returns>
        public static DefaultHttpContext CreateHttpContextWithMapper(Mock<IEndpointOutcomeToHttpMapper>? mapperMock = null)
        {
            DefaultHttpContext context = CreateHttpContext();
            var services = new ServiceCollection();

            mapperMock ??= new Mock<IEndpointOutcomeToHttpMapper>();
            services.AddSingleton(mapperMock.Object);

            context.RequestServices = services.BuildServiceProvider();
            return context;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
