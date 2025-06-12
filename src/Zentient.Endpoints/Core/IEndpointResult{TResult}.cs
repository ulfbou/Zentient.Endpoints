// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEndpointResult{TResult}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Zentient.Results;

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Defines the contract for an operation's outcome at the application's boundary (endpoint),
    /// providing access to a strongly-typed business result and transport-agnostic metadata.
    /// This interface extends <see cref="IEndpointResult"/> to allow for generic handling
    /// of endpoint results with a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The type of the business result, which must be notnull.</typeparam>
    /// <remarks>
    /// <see cref="IEndpointResult{TResult}"/> serves as the bridge between internal <c>Zentient.Results</c> and
    /// external transport-specific responses, encapsulating the core business outcome
    /// while allowing for additional metadata relevant to the transport layer.
    /// This interface is designed to be used in scenarios where the result type is known
    /// at compile time, enabling strong typing and better integration with the application's logic.
    /// It is particularly useful in scenarios where the endpoint result needs to be processed
    /// by filters or adapters that require knowledge of the specific result type.
    /// </remarks>
    public interface IEndpointResult<out TResult> : IEndpointResult
        where TResult : notnull
    {
        /// <summary>
        /// Gets the strongly-typed value of the business result.
        /// </summary>
        /// <value>The value of the result, of type <c>TResult</c>.</value>
        TResult Result { get; }
    }
}
