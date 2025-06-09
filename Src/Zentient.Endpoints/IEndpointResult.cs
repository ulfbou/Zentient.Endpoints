// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEndpointResult.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// <license>Licensed under the MIT License. See LICENSE in the project root for license information.</license>
// --------------------------------------------------------------------------------------------------------------------

using Zentient.Results;

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Defines the common contract for an operation's outcome at the application's boundary (endpoint),
    /// providing access to the underlying business result and transport-agnostic metadata.
    /// This interface serves as the base for both generic and non-generic endpoint results,
    /// enabling polymorphic handling by endpoint filters and adapters.
    /// </summary>
    /// <remarks>
    /// <c>IEndpointResult</c> acts as the bridge between internal <c>Zentient.Results</c>
    /// and external transport-specific responses. It encapsulates the core business outcome
    /// while allowing for additional metadata relevant to the transport layer.
    /// </remarks>
    public interface IEndpointResult
    {
        /// <summary>
        /// Gets the underlying business result of the operation.
        /// This provides the core success/failure state, messages, and errors
        /// from the application logic, independent of the transport.
        /// </summary>
        /// <value>An instance of <see cref="IResult"/>.</value>
        IResult BaseResult { get; }

        /// <summary>
        /// Gets the transport-agnostic metadata associated with this endpoint result.
        /// This includes information like suggested HTTP status codes, headers, or
        /// gRPC tags, which can be used by transport adapters to construct the final response.
        /// </summary>
        /// <value>Transport-agnostic metadata object.</value>
        object BaseTransport { get; }
    }
}
