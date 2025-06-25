// <copyright file="NormalizeEndpointOutcomeFilter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Zentient.Endpoints.Http.Mapping; // For IEndpointOutcomeToHttpMapper
using Zentient.Results;

namespace Zentient.Endpoints.Http.Filters
{
    /// <summary>
    /// An ASP.NET Core endpoint filter that intercepts <see cref="IEndpointOutcome"/> results
    /// and maps them to <see cref="Microsoft.AspNetCore.Http.IResult"/> using the configured mapper.
    /// It also enriches the outcome's metadata with request-scoped services like ILogger.
    /// </summary>
    public class NormalizeEndpointOutcomeFilter : IEndpointFilter
    {
        private readonly IEndpointOutcomeToHttpMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeEndpointOutcomeFilter"/> class.
        /// </summary>
        /// <param name="mapper">The mapper responsible for converting outcomes to HTTP results.</param>
        public NormalizeEndpointOutcomeFilter(IEndpointOutcomeToHttpMapper mapper)
        {
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Invokes the filter, checking if the endpoint result is an <see cref="IEndpointOutcome"/>
        /// and mapping it accordingly.
        /// </summary>
        /// <param name="context">The endpoint filter context.</param>
        /// <param name="next">The delegate to call the next filter or the endpoint itself.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation,
        /// containing the final HTTP result.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            var rawResult = await next(context).ConfigureAwait(false);

            if (rawResult is not IEndpointOutcome outcome)
            {
                return rawResult;
            }

            outcome = outcome.WithRequestServices(context.HttpContext);
            return await this._mapper.Map(outcome, context.HttpContext, context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
    }
}
