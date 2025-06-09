// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NormalizeEndpointResultFilter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System; // For ArgumentNullException
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Zentient.Endpoints.Core; // For IEndpointResult

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// An ASP.NET Core <see cref="IEndpointFilter"/> that intercepts <see cref="EndpointResult{TResult}"/>
    /// instances returned by endpoint handlers and converts them into appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>
    /// responses for the HTTP pipeline.
    /// </summary>
    /// <remarks>
    /// This filter enables a clean separation between business logic outcomes (<see cref="EndpointResult{TResult}"/>)
    /// and HTTP-specific responses, ensuring consistency across your Minimal APIs and MVC actions.
    /// </remarks>
    public sealed class NormalizeEndpointResultFilter : IEndpointFilter
    {
        private readonly IEndpointResultToHttpResultMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeEndpointResultFilter"/> class.
        /// </summary>
        /// <param name="mapper">The mapper service used to convert <see cref="IEndpointResult"/> to <see cref="Microsoft.AspNetCore.Http.IResult"/>.</param>
        public NormalizeEndpointResultFilter(IEndpointResultToHttpResultMapper mapper)
        {
            this._mapper = mapper;
        }

        /// <summary>
        /// Executes the endpoint filter.
        /// </summary>
        /// <param name="context">The endpoint filter invocation context.</param>
        /// <param name="next">The next filter in the pipeline.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(next, nameof(next));

            var result = await next(context).ConfigureAwait(false);

            if (result is IEndpointResult endpointResult)
            {
                return this._mapper.Map(endpointResult, context.HttpContext);
            }

            return result;
        }
    }
}
