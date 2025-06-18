// <copyright file="NormalizeEndpointOutcomeFilter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Zentient.Endpoints;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// An <see cref="IEndpointFilter"/> that normalizes <see cref="IEndpointOutcome"/> instances
    /// returned by Minimal API endpoints into proper <see cref="Microsoft.AspNetCore.Http.IResult"/> objects.
    /// </summary>
    public sealed class NormalizeEndpointOutcomeFilter : IEndpointFilter
    {
        private readonly IEndpointOutcomeToHttpMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeEndpointOutcomeFilter"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IEndpointOutcomeToHttpMapper"/> used to convert endpoint results to HTTP results.</param>
        public NormalizeEndpointOutcomeFilter(IEndpointOutcomeToHttpMapper mapper)
        {
            this._mapper = mapper;
        }

        /// <inheritdoc />
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            object? result = await next(context).ConfigureAwait(false);

            if (result is IEndpointOutcome endpointResult)
            {
                return await this._mapper.Map(endpointResult, context.HttpContext).ConfigureAwait(false);
            }

            return result;
        }
    }
}
