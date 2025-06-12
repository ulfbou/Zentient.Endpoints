// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zentient.Results; // For ErrorInfo

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Defines the contract for mapping a <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance.
    /// This allows for customization of how application errors are translated into HTTP Problem Details responses.
    /// </summary>
    public interface IProblemDetailsMapper
    {
        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance.
        /// </summary>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> to map.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, providing additional context for the mapping.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance representing the given error.</returns>
        ProblemDetails Map(ErrorInfo? errorInfo, HttpContext httpContext);
    }
}
