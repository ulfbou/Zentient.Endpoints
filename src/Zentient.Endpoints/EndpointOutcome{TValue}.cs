// <copyright file="EndpointOutcome{TValue}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, generic outcome of an endpoint operation that produces a value.
    /// This type encapsulates a <see cref="Zentient.Results.IResult{TValue}"/> and
    /// <see cref="TransportMetadata"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
    internal sealed class EndpointOutcome<TValue> : EndpointOutcome, IEndpointOutcome<TValue>
        where TValue : notnull // TValue must be non-nullable as per design
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome{TValue}"/> class.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="metadata">Optional transport metadata.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is <see langword="null" />.
        /// </exception>
        internal EndpointOutcome(IResult<TValue> result, TransportMetadata? metadata = null)
            : base(result, metadata)
        { }

        /// <inheritdoc/>
        public TValue? Value
        {
            get
            {
                if (_innerResult.IsFailure)
                {
                    return default;
                }
                IResult<TValue> result = (IResult<TValue>)_innerResult;
                return result.Value;
            }
        }

        /// <summary>
        /// Creates a successful endpoint outcome with a value and optional transport metadata.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static IEndpointOutcome<TValue> Success(
            TValue value,
            TransportMetadata? transportMetadata = null) =>
            new EndpointOutcome<TValue>(Result<TValue>.Success(value), transportMetadata);

        /// <summary>
        /// Creates a successful endpoint outcome without a value (for Unit or NoContent scenarios)
        /// with optional transport metadata.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>
        /// A successful <see cref="IEndpointOutcome{TValue}"/> representing no content.
        /// </returns>
        /// <remarks>
        /// Typically used when TValue is Unit, or when a 204 No Content is desired.
        /// For non-Unit TValue, this will create a Result with default(TValue) and 204 status.
        /// The HTTP mapper should handle 204s by suppressing the body regardless of TValue.
        /// </remarks>
        public static IEndpointOutcome<TValue> NoContent(TransportMetadata? transportMetadata = null)
        {
            if (typeof(TValue) == typeof(Unit))
            {
                // For Unit, use the specific NoContent result
                return new EndpointOutcome<TValue>(Result<TValue>.NoContent(), transportMetadata);
            }

            // For other TValue, create a success result with default value and NoContent status
            // The mapper will handle the 204 and typically suppress the body.
            return new EndpointOutcome<TValue>(Result<TValue>.Success(default(TValue)!, Results.ResultStatuses.NoContent), transportMetadata);
        }

        /// <summary>
        /// Creates a failed endpoint outcome from an <see cref="ErrorInfo"/>
        /// with optional transport metadata.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="error"/> is <see langword="null" />.
        /// </exception>
        public new static IEndpointOutcome<TValue> From(
            ErrorInfo error,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));

            // For failure, the value is typically not meaningful, so default(TValue) is used.
            return new EndpointOutcome<TValue>(
                Result<TValue>.Failure(default(TValue), error, ResultStatuses.BadRequest),
                transportMetadata);
        }

        /// <summary>
        /// Creates an endpoint outcome from an <see cref="Zentient.Results.IResult{TValue}"/>
        /// with optional transport metadata.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>An <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is <see langword="null" />.
        /// </exception>
        public static IEndpointOutcome<TValue> From(
            IResult<TValue> result,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return new EndpointOutcome<TValue>(result, transportMetadata);
        }

        /// <inheritdoc/>
        internal override object? GetValueAsObject() =>
            ((IResult<TValue>)_innerResult).Value;
    }
}
