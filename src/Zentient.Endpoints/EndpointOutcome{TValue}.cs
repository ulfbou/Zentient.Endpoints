// <copyright file="EndpointOutcome{TValue}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Zentient.Endpoints;

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, generic outcome of an endpoint operation that produces a value.
    /// This type encapsulates a <see cref="Zentient.Results.IResult{TValue}"/> and <see cref="TransportMetadata"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
    internal sealed class EndpointOutcome<TValue> : IEndpointOutcome<TValue>
        where TValue : notnull
    {
        /// <summary>Gets the underlying generic business result.</summary>
        private readonly IResult<TValue> _innerResult;

        /// <summary>Initializes a new instance of the <see cref="EndpointOutcome{TValue}"/> class.</summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        private EndpointOutcome(IResult<TValue> result, TransportMetadata? transportMetadata = null)
        {
            this._innerResult = result ?? throw new ArgumentNullException(nameof(result));
            this.TransportMetadata = transportMetadata ?? new TransportMetadata();
        }

        /// <inheritdoc/>
        public TValue? Value => this._innerResult.Value;

        /// <inheritdoc/>
        public bool IsSuccess => this._innerResult.IsSuccess;

        /// <inheritdoc/>
        public bool IsFailure => this._innerResult.IsFailure;

        /// <inheritdoc/>
        public IReadOnlyList<ErrorInfo> Errors => this._innerResult.Errors;

        /// <inheritdoc/>
        public IReadOnlyList<string> Messages => this._innerResult.Messages;

        /// <inheritdoc/>
        public string? ErrorMessage => this._innerResult.ErrorMessage;

        /// <inheritdoc/>
        public IResultStatus Status => this._innerResult.Status;

        /// <inheritdoc/>
        public TransportMetadata TransportMetadata { get; }

        /// <summary>
        /// Creates a successful endpoint outcome with a value and optional transport metadata.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static IEndpointOutcome<TValue> Success(TValue value, TransportMetadata? transportMetadata = null) =>
            new EndpointOutcome<TValue>(Result<TValue>.Success(value), transportMetadata);

        /// <summary>
        /// Creates a successful endpoint outcome without a value (for Unit or NoContent scenarios) with optional transport metadata.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome{TValue}"/> representing no content.</returns>
        /// <remarks>Typically used when TValue is Unit, or when a 204 No Content is desired.</remarks>
        public static IEndpointOutcome<TValue> NoContent(TransportMetadata? transportMetadata = null)
        {
            if (typeof(TValue) == typeof(Unit))
            {
                return new EndpointOutcome<TValue>(Result<TValue>.NoContent(), transportMetadata);
            }

            return new EndpointOutcome<TValue>(Result<TValue>.Success(value: default!, Results.ResultStatuses.NoContent), transportMetadata);
        }

        /// <summary>
        /// Creates a failed endpoint outcome from an <see cref="ErrorInfo"/> with optional transport metadata.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is <c>null</c>.</exception>
        public static IEndpointOutcome<TValue> From(ErrorInfo error, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome<TValue>(Result<TValue>.Failure(default, error, ResultStatuses.BadRequest), transportMetadata);

        /// <summary>
        /// Creates an endpoint outcome from an <see cref="IResult{TValue}"/> with optional transport metadata.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>An <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IEndpointOutcome<TValue> From(IResult<TValue> result, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome<TValue>(result, transportMetadata);
    }
}
