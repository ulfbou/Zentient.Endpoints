// <copyright file="ServiceCollectionExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

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
        /// This includes default implementations for <see cref="IProblemTypeUriGenerator"/>,
        /// <see cref="IProblemDetailsMapper"/>, and <see cref="IEndpointOutcomeToHttpMapper"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddZentientEndpointsHttp(
            this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped<IProblemTypeUriGenerator, DefaultProblemTypeUriGenerator>();
            services.TryAddScoped<IProblemDetailsMapper, DefaultProblemDetailsMapper>();
            services.TryAddScoped<IEndpointOutcomeToHttpMapper, EndpointOutcomeHttpMapper>();

            return services;
        }

        /// <summary>
        /// Adds the <see cref="NormalizeEndpointOutcomeFilter"/> to the <see cref="RouteHandlerBuilder"/>,
        /// ensuring that any <see cref="Zentient.Endpoints.IEndpointOutcome"/> returned by the endpoint
        /// is correctly mapped to an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <param name="builder">The <see cref="RouteHandlerBuilder"/> to add the filter to.</param>
        /// <returns>The <see cref="RouteHandlerBuilder"/> so that additional calls can be chained.</returns>
        public static RouteHandlerBuilder WithNormalizeEndpointOutcomeFilter(
            [NotNull] this RouteHandlerBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.AddEndpointFilter<NormalizeEndpointOutcomeFilter>();
            return builder;
        }
    }
}
