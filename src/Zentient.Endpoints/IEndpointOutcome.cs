// <copyright file="IEndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Results;

using System.Collections.Generic;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the non-generic outcome of an endpoint operation,
    /// providing status, error details, and transport-agnostic metadata.
    /// </summary>
    /// <remarks>
    /// Serves as the public contract for filters and adapters handling
    /// endpoint results without exposing business-logic internals.
    /// </remarks>
    public interface IEndpointOutcome
    {
        /// <summary>Gets a value indicating whether the operation succeeded.</summary>
        /// <value><c>true</c> if the operation was successful; otherwise, <c>false</c>.</value>
        bool IsSuccess { get; }

        /// <summary>Gets a value indicating whether the operation failed.</summary>
        /// <value><c>true</c> if the operation failed; otherwise, <c>false</c>.</value>
        bool IsFailure { get; }

        /// <summary>
        /// Gets a read-only list of <see cref="ErrorInfo"/> instances detailing errors.
        /// </summary>
        /// <value>
        /// A list of <see cref="ErrorInfo"/> objects, or empty if no errors occurred.
        /// </value>
        IReadOnlyList<ErrorInfo> Errors { get; }

        /// <summary>
        /// Gets a read-only list of messages associated with the outcome.
        /// </summary>
        /// <value>
        /// A list of informational or warning messages, or empty if none are present.
        /// </value>
        IReadOnlyList<string> Messages { get; }

        /// <summary>
        /// Gets the first error message, or <see langword="null"/> if the operation succeeded.
        /// </summary>
        /// <value>
        /// The first error message string, or <see langword="null"/> when no errors exist.
        /// </value>
        string? ErrorMessage { get; }

        /// <summary>Gets the semantic status of the business result.</summary>
        /// <value>An <see cref="IResultStatus"/> indicating success or specific failure type.</value>
        IResultStatus Status { get; }

        /// <summary>Gets transport-level hints (e.g., HTTP or gRPC status codes).</summary>
        /// <value>
        /// A <see cref="TransportMetadata"/> instance containing transport hints
        /// for adapters.
        /// </value>
        TransportMetadata Metadata { get; }
    }
}
