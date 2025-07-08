// <copyright file="PostConfigureNormalizeEndpointOutcomeFilter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Zentient.Endpoints.Http.Filters;
using Zentient.Endpoints.Http.Options;

namespace Zentient.Endpoints.Http.Options
{
    internal class PostConfigureNormalizeEndpointOutcomeFilter : IPostConfigureOptions<EndpointsHttpOptions>
    {
        private readonly IServiceCollection _services;

        public PostConfigureNormalizeEndpointOutcomeFilter(IServiceCollection services)
        {
            _services = services;
        }

        public void PostConfigure(string? name, EndpointsHttpOptions options)
        {
            if (options.AddNormalizeEndpointOutcomeFilterGlobally)
            {
                _services.TryAddScoped<IEndpointFilter, NormalizeEndpointOutcomeFilter>();
            }
        }
    }
}
