// <copyright file="EndpointOutcomeHttpContextExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Zentient.Endpoints.Extensions;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Extensions
{
    /// <summary>
    /// Internal extension methods to enrich <see cref="IEndpointOutcome"/> metadata
    /// with specific services resolved from the <see cref="HttpContext"/>.
    /// These are typically used within ASP.NET Core pipeline components like filters.
    /// </summary>
    internal static class EndpointOutcomeHttpContextExtensions
    {
        /// <summary>
        /// Enriches the <see cref="IEndpointOutcome"/>'s metadata with request-scoped services,
        /// such as an <see cref="ILogger"/> instance, extracted from the provided
        /// <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="outcome">The endpoint outcome to enrich.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> instance with enriched metadata.</returns>
        public static IEndpointOutcome WithRequestServices(
            this IEndpointOutcome outcome,
            HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(httpContext.GetEndpoint()?.DisplayName ?? "Zentient.Endpoints.Http");

            return outcome.WithMetadata(m => m.SetTag("Logger", logger));
        }

        /// <summary>
        /// Enriches a generic <see cref="IEndpointOutcome{TValue}"/>'s metadata
        /// with request-scoped services.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The generic endpoint outcome to enrich.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>
        /// A new <see cref="IEndpointOutcome{TValue}"/> instance with enriched metadata.
        /// </returns>
        public static IEndpointOutcome<TValue> WithRequestServices<TValue>(
            this IEndpointOutcome<TValue> outcome,
            HttpContext httpContext) where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(httpContext.GetEndpoint()?.DisplayName ?? "Zentient.Endpoints.Http");

            return outcome.WithMetadata(m => m.SetTag("Logger", logger));
        }
    }
}
