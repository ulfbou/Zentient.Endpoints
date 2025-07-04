// <copyright file="IEndpointOutcomeToHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Defines a contract for mapping an <see cref="IEndpointOutcome"/> to a
    /// <see cref="Microsoft.AspNetCore.Http.IResult"/>.
    /// </summary>
    public interface IEndpointOutcomeToHttpMapper
    {
        /// <summary>
        /// Maps the given endpoint outcome to an ASP.NET Core
        /// <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <param name="outcome">The endpoint outcome to map.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation,
        /// containing the mapped <see cref="Microsoft.AspNetCore.Http.IResult"/>.</returns>
        Task<Microsoft.AspNetCore.Http.IResult> Map(
      IEndpointOutcome outcome,
      HttpContext httpContext,
      CancellationToken ct = default);
    }
}
