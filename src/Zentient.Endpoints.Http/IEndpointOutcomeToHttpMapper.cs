// <copyright file="IEndpointOutcomeToHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

using Zentient.Endpoints;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Defines the contract for a mapper that converts <see cref="IEndpointOutcome"/>
    /// instances to ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> asynchronously.
    /// </summary>
    public interface IEndpointOutcomeToHttpMapper
    {
        /// <summary>
        /// Maps an <see cref="IEndpointOutcome"/> to an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> asynchronously.
        /// </summary>
        /// <param name="endpointResult">The endpoint result to map.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the mapped HTTP response.</returns>
        Task<Microsoft.AspNetCore.Http.IResult> Map(IEndpointOutcome endpointResult, HttpContext httpContext);
    }
}
