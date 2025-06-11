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
        /// <value>The HTTP status code, or <c>null</c> if not specified.</value>
        public int? HttpStatusCode { get; init; }

        /// <summary>
        /// Gets the suggested gRPC status code for the response.
        /// </summary>
        /// <value>The gRPC status code, or <c>null</c> if not specified.</value>
        public int? GrpcStatusCode { get; init; }

        /// <summary>
        /// Gets the optional Orleans grain error code for distributed scenarios.
        /// </summary>
        /// <value>The Orleans grain error code, or <c>null</c> if not specified.</value>
        public string? OrleansGrainErrorCode { get; init; }

        /// <summary>
        /// Gets the <see cref="ProblemDetails"/> instance describing the error, if any.
        /// </summary>
        /// <value>The <see cref="ProblemDetails"/> instance, or <c>null</c> if not specified.</value>
        public ProblemDetails? ProblemDetails { get; init; }

        /// <summary>
        /// Gets the collection of transport-level tags or metadata.
        /// </summary>
        /// <value>A dictionary of tags, where the key is a string and the value is an object.</value>
        public IDictionary<string, object> Tags { get; init; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a default <see cref="TransportMetadata"/> instance.
        /// The <see cref="HttpStatusCode"/> will be <c>null</c> by default,
        /// deferring the final status code decision to the specific transport mapper
        /// based on the success or failure state of the <see cref="Zentient.Results.IResult"/>.
        /// </summary>
        /// <param name="httpStatusCode">Optional: A specific HTTP status code to include. If <c>null</c>,
        /// the transport adapter's default for the current context (success or failure) will be used.</param>
        /// <param name="problemDetails">Optional: A <see cref="ProblemDetails"/> instance to associate.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with default or specified values.</returns>
        public static TransportMetadata Default(int? httpStatusCode = null, ProblemDetails? problemDetails = null)
        {
            return new TransportMetadata
            {
                HttpStatusCode = httpStatusCode,
                ProblemDetails = problemDetails,
            };
        }
    }
}
