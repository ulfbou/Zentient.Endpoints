// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Defines the common contract for an operation's outcome at the application's boundary (endpoint),
    /// providing access to the underlying business result and transport-agnostic metadata.
    /// This interface serves as the base for both generic and non-generic endpoint results,
    /// enabling polymorphic handling by endpoint filters and adapters.
    /// </summary>
    /// <remarks>
    /// <c>IEndpointOutcome</c> acts as the bridge between internal <c>Zentient.Results</c>
    /// and external transport-specific responses. It encapsulates the core business outcome
    /// while allowing for additional metadata relevant to the transport layer.
    /// </remarks>
    public interface IEndpointOutcome
    {
        /// <summary>Gets a value indicating whether the endpoint operation was successful.</summary>
        /// <remarks>Proxies <see cref="IResult.IsSuccess"/> from the underlying business result.</remarks>
        /// <value><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</value>
        bool IsSuccess { get; }

        /// <summary>Gets a value indicating whether the endpoint operation failed.</summary>
        /// <remarks>Proxies <see cref="IResult.IsFailure"/> from the underlying business result.</remarks>
        /// <value><see langword="true"/> if the operation failed; otherwise, <see langword="false"/>.</value>
        bool IsFailure { get; }

        /// <summary>Gets a read-only list of detailed error information if the operation failed. Empty if successful.</summary>
        /// <remarks>Proxies <see cref="IResult.Errors"/> from the underlying business result.</remarks>
        /// <value>A read-only list of <see cref="ErrorInfo"/> objects representing errors, or an empty list if there are no errors.</value>
        IReadOnlyList<ErrorInfo> Errors { get; }

        /// <summary>Gets a read-only list of messages associated with the result (success or failure).</summary>
        /// <remarks>Proxies <see cref="IResult.Messages"/> from the underlying business result.</remarks>
        /// <value>A read-only list of strings containing messages, which may include warnings or informational notes.</value>
        IReadOnlyList<string> Messages { get; }

        /// <summary>Gets the message of the first error if the operation failed; otherwise, null.</summary>
        /// <remarks>Proxies <see cref="IResult.ErrorMessage"/> from the underlying business result.</remarks>
        /// <value>The message of the first error, or <see langword="null"/> if there are no errors.</value>
        string? ErrorMessage { get; }

        /// <summary>Gets the semantic status of the result, providing contextual information (e.g., HTTP-like status codes).</summary>
        /// <remarks>Proxies <see cref="IResult.Status"/> from the underlying business result.</remarks>
        /// <value>An <see cref="IResultStatus"/> instance representing the status of the operation, such as success or various error categories.</value>
        IResultStatus Status { get; }

        /// <summary>Gets transport-agnostic metadata associated with this endpoint outcome.</summary>
        /// <remarks>
        /// This metadata includes hints for HTTP status codes, gRPC status, and other transport-specific information.
        /// It is used by transport adapters to map the endpoint outcome to the appropriate response format.
        /// </remarks>
        /// <value>A <see cref="Endpoints.TransportMetadata"/> instance containing transport-specific metadata.</value>
        TransportMetadata TransportMetadata { get; }
    }
}
