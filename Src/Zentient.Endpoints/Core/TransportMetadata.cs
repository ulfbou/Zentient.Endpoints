// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransportMetadata.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Represents transport-agnostic metadata for endpoint results, such as HTTP status codes, gRPC status, and additional tags.
    /// </summary>
    public sealed class TransportMetadata
    {
        /// <summary>
        /// Gets the suggested HTTP status code for the response.
        /// </summary>
        public int? HttpStatusCode { get; init; }

        /// <summary>
        /// Gets the suggested gRPC status code for the response.
        /// </summary>
        public int? GrpcStatusCode { get; init; }

        /// <summary>
        /// Gets the optional Orleans grain error code for distributed scenarios.
        /// </summary>
        public string? OrleansGrainErrorCode { get; init; }

        /// <summary>
        /// Gets the <see cref="ProblemDetails"/> instance describing the error, if any.
        /// </summary>
        public ProblemDetails? ProblemDetails { get; init; }

        /// <summary>
        /// Gets the collection of transport-level tags or metadata.
        /// </summary>
        public Dictionary<string, object> Tags { get; init; } = new();

        /// <summary>
        /// Creates a default <see cref="TransportMetadata"/> instance for an error.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code to use, or <c>null</c> for 500.</param>
        /// <param name="problemDetails">The problem details to use, or <c>null</c>.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance.</returns>
        public static TransportMetadata Default(int? httpStatusCode = null, ProblemDetails? problemDetails = null)
        {
            return new TransportMetadata
            {
                HttpStatusCode = httpStatusCode ?? StatusCodes.Status500InternalServerError,
                ProblemDetails = problemDetails,
            };
        }
    }
}
