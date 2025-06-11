// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to register Zentient.Endpoints.Http services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Zentient.Endpoints.Http services to the specified <see cref="IServiceCollection"/>.
        /// This includes default implementations for <see cref="IProblemDetailsMapper"/> and <see cref="IEndpointResultToHttpResultMapper"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddZentientEndpointsHttp(
            [NotNull] this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped<IProblemTypeUriGenerator, DefaultProblemTypeUriGenerator>();
            services.TryAddScoped<IProblemDetailsMapper, DefaultProblemDetailsMapper>();
            services.TryAddScoped<IEndpointResultToHttpResultMapper, EndpointResultHttpMapper>();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="NormalizeEndpointResultFilter"/> to the <see cref="RouteHandlerBuilder"/>,
        /// ensuring that any <see cref="Zentient.Endpoints.Core.IEndpointResult"/> returned by the endpoint
        /// is correctly mapped to an ASP.NET Core <see cref="IResult"/>.
        /// </summary>
        /// <param name="builder">The <see cref="RouteHandlerBuilder"/> to add the filter to.</param>
        /// <returns>The <see cref="RouteHandlerBuilder"/> so that additional calls can be chained.</returns>
        public static RouteHandlerBuilder WithNormalizeEndpointResultFilter(
                [NotNull] this RouteHandlerBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddEndpointFilter<NormalizeEndpointResultFilter>();
            return builder;
        }
    }
}
