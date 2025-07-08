// <copyright file="IProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zentient.Results;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Defines the contract for a mapper that converts <see cref="ErrorInfo"/>
    /// instances to ASP.NET Core <see cref="ProblemDetails"/> asynchronously.
    /// </summary>
    public interface IProblemDetailsMapper
    {
        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance asynchronously.
        /// </summary>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> to map. Can be <c>null</c> for generic errors.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, providing additional context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="ProblemDetails"/> instance.</returns>
        Task<ProblemDetails> Map(ErrorInfo? errorInfo, HttpContext httpContext);
    }
}
