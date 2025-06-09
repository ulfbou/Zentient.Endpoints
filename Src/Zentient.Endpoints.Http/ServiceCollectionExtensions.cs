// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder; // For RouteHandlerBuilder
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // For TryAddScoped

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to configure
    /// Zentient.Endpoints HTTP integration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds necessary services for Zentient.Endpoints HTTP integration, including
        /// default Problem Details mapping and the Endpoint Result normalization filter.
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddZentientEndpointsHttp(this IServiceCollection services)
        {
            // Register the default ProblemDetailsMapper, allowing it to be overridden by a custom implementation.
            services.TryAddScoped<IProblemDetailsMapper, DefaultProblemDetailsMapper>();

            // Register the EndpointResultToHttpResultMapper, which uses the IProblemDetailsMapper.
            services.TryAddScoped<IEndpointResultToHttpResultMapper, EndpointResultHttpMapper>();

            // The NormalizeEndpointResultFilter is typically registered as a global endpoint filter
            // using the WithNormalizeEndpointResultFilter extension on RouteHandlerBuilder for Minimal APIs,
            // or implicitly handled in MVC by returning IEndpointResult.
            // No direct registration of the filter as a scoped service is needed here for its intended use case.
            return services;
        }

        /// <summary>
        /// Adds the <see cref="NormalizeEndpointResultFilter"/> as an endpoint filter
        /// to a <see cref="Microsoft.AspNetCore.Builder.RouteHandlerBuilder"/> (e.g., for Minimal APIs).
        /// </summary>
        /// <param name="builder">The <see cref="Microsoft.AspNetCore.Builder.RouteHandlerBuilder"/> to add the filter to.</param>
        /// <returns>The updated <see cref="Microsoft.AspNetCore.Builder.RouteHandlerBuilder"/>.</returns>
        public static RouteHandlerBuilder WithNormalizeEndpointResultFilter(this RouteHandlerBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            return builder.AddEndpointFilter<NormalizeEndpointResultFilter>();
        }
    }
}
