// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEndpointResultToHttpResultMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

using Zentient.Endpoints.Core;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Defines the contract for a service that maps a generic <see cref="IEndpointResult"/>
    /// to an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>. This is the core conversion logic for the HTTP transport.
    /// </summary>
    public interface IEndpointResultToHttpResultMapper
    {
        /// <summary>
        /// Maps the given <see cref="IEndpointResult"/> to an appropriate ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <param name="endpointResult">The endpoint result to map.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> instance suitable for ASP.NET Core response.</returns>
        Microsoft.AspNetCore.Http.IResult Map(IEndpointResult endpointResult, HttpContext httpContext);
    }
}
