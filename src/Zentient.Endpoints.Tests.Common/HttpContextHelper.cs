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
            ArgumentNullException.ThrowIfNull(contentResult, nameof(contentResult));
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
        /// <returns>A tuple of the <see cref="DefaultHttpContext"/> and its <see cref="MemoryStream"/> response body.</returns>
        public static (DefaultHttpContext context, MemoryStream responseStream) CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/mocked-test-path";
            var responseStream = new MemoryStream();
            context.Response.Body = responseStream;
            return (context, responseStream);
        }

        /// <summary>
        /// Creates a <see cref="DefaultHttpContext"/> instance with a mocked <see cref="IEndpointOutcomeToHttpMapper"/>
        /// registered in its service provider. Returns both the context and the response stream for test inspection.
        /// </summary>
        /// <param name="mapperMock">An optional pre-configured mock for <see cref="IEndpointOutcomeToHttpMapper"/>.</param>
        /// <returns>A tuple of the <see cref="DefaultHttpContext"/> and its <see cref="MemoryStream"/> response body.</returns>
        public static (DefaultHttpContext context, MemoryStream responseStream) CreateHttpContextWithMapper(Mock<IEndpointOutcomeToHttpMapper>? mapperMock = null)
        {
            var (context, responseStream) = CreateHttpContext();
            var services = new ServiceCollection();

            mapperMock ??= new Mock<IEndpointOutcomeToHttpMapper>();
            services.AddSingleton(mapperMock.Object);

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
