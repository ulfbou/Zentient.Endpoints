// <copyright file="IEndpointOutcome{TValue}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Defines the contract for the comprehensive outcome of an endpoint operation
    /// that produces a value on success, encapsulating the business result and
    /// transport-agnostic metadata.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
    public interface IEndpointOutcome<out TValue> : IEndpointOutcome // Renamed from IEndpointOutcome<TResult>
        where TValue : notnull // Maintain the notnull constraint
    {
        /// <summary>
        /// Gets the value produced by the operation if it was successful.
        /// This property may be null even on success if the operation explicitly returns a null value.
        /// </summary>
        /// <remarks>Proxies <see cref="IResult{T}.Value"/> from the underlying business result.</remarks>
        /// <value>The value produced by the operation, or <see langword="null"/> if the operation was successful, but no value was produced.</value>
        TValue? Value { get; }
    }
}
