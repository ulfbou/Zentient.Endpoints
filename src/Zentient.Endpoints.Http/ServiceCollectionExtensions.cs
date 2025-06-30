// <copyright file="ServiceCollectionExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options.Validation;

using Zentient.Endpoints.Http.Filters;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Options;
using Zentient.Endpoints.Http.Validation;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to add
    /// Zentient.Endpoints.Http services and filters, configured via robust options.
    /// These methods are designed for maintainable, testable, and production-ready
    /// integration into ASP.NET Core applications.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds necessary services for Zentient.Endpoints.Http to the service collection.
        /// This includes core HTTP mappers, response factories, and conditionally registers
        /// the <see cref="NormalizeEndpointOutcomeFilter"/> globally based on configuration.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> to add the services to. Must not be
        /// <see langword="null"/>.
        /// </param>
        /// <param name="configureOptions">
        /// An optional action to configure the <see cref="EndpointsHttpOptions"/>.
        /// Use this to customize Problem Details behavior, global filter registration,
        /// and other settings.
        /// </param>
        /// <param name="configureJsonOptions">
        /// An optional action to further configure the <see cref="JsonSerializerOptions"/> used by
        /// <see cref="Zentient.Endpoints.Http" />.
        /// This allows fine-tuning serialization behavior without replacing the entire options
        /// object.
        /// </param>
        /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddZentientEndpointsHttp(
            this IServiceCollection services,
            Action<EndpointsHttpOptions>? configureOptions = null,
            Action<JsonSerializerOptions>? configureJsonOptions = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            var optionsBuilder = services.AddOptions<EndpointsHttpOptions>()
                                         .Configure(options => configureOptions?.Invoke(options))
                                         .ApplyZentientConventions();
            services.AddSingleton<IValidateOptions<EndpointsHttpOptions>, EndpointsHttpOptionsValidator>();

            if (configureJsonOptions != null)
            {
                optionsBuilder.PostConfigure(options =>
                {
                    configureJsonOptions.Invoke(options.JsonSerializerOptions);
                });
            }

            services.PostConfigure<EndpointsHttpOptions>(options =>
            {
                if (options.ProblemDetails.BaseTypeUri is { } uri && !uri.ToString().EndsWith('/'))
                {
                    options.ProblemDetails.BaseTypeUri = new Uri(uri.ToString() + "/");
                }
            });

            services.TryAddScoped<IEndpointOutcomeToHttpMapper, EndpointOutcomeToHttpMapper>();
            services.TryAddScoped<IProblemDetailsMapper, DefaultProblemDetailsMapper>();
            services.TryAddScoped<IProblemTypeUriGenerator, DefaultProblemTypeUriGenerator>();
            services.TryAddScoped<ISuccessResponseFactory, DefaultSuccessResponseFactory>();

            var configuredOptionsForFilterCheck = new EndpointsHttpOptions();
            configureOptions?.Invoke(configuredOptionsForFilterCheck);

            if (configuredOptionsForFilterCheck.AddNormalizeEndpointOutcomeFilterGlobally)
            {
                services.TryAddScoped<IEndpointFilter, NormalizeEndpointOutcomeFilter>();
            }

            return services;
        }

        /// <summary>
        /// Adds the <see cref="NormalizeEndpointOutcomeFilter"/> to the <see cref="RouteHandlerBuilder"/>,
        /// ensuring that any <see cref="Zentient.Endpoints.IEndpointOutcome"/> returned by the endpoint
        /// is correctly mapped to an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// This method provides explicit, per-endpoint filter registration.
        /// </summary>
        /// <param name="builder">The <see cref="RouteHandlerBuilder"/> to add the filter to. Must not be <see langword="null"/>.</param>
        /// <returns>The <see cref="RouteHandlerBuilder"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        public static RouteHandlerBuilder WithNormalizeEndpointOutcomeFilter(
            this RouteHandlerBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddEndpointFilter<NormalizeEndpointOutcomeFilter>();
            return builder;
        }
    }
}
