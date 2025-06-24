// <copyright file="EndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, non-generic outcome of an endpoint operation.
    /// This type encapsulates an <see cref="Zentient.Results.IResult"/> and
    /// <see cref="TransportMetadata"/>.
    /// </summary>
    internal class EndpointOutcome : IEndpointOutcome
    {
        /// <summary>Gets the underlying business result.</summary>
        protected readonly IResult _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome"/> class.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="metadata">Optional transport metadata.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is<see langword="null" />.
        /// </exception>
        internal EndpointOutcome(IResult result, TransportMetadata? metadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            _innerResult = result;
            Metadata = metadata ?? new TransportMetadata();
        }

        /// <inheritdoc/>
        public bool IsSuccess => _innerResult.IsSuccess;

        /// <inheritdoc/>
        public bool IsFailure => _innerResult.IsFailure;

        /// <inheritdoc/>
        public IReadOnlyList<ErrorInfo> Errors => _innerResult.Errors;

        /// <inheritdoc/>
        public IReadOnlyList<string> Messages => _innerResult.Messages;

        /// <inheritdoc/>
        public string? ErrorMessage => _innerResult.ErrorMessage;

        /// <inheritdoc/>
        public IResultStatus Status => _innerResult.Status;

        /// <inheritdoc/>
        public TransportMetadata Metadata { get; }

        bool IEndpointOutcome.IsSuccess => throw new NotImplementedException();

        bool IEndpointOutcome.IsFailure => throw new NotImplementedException();

        IReadOnlyList<ErrorInfo> IEndpointOutcome.Errors => throw new NotImplementedException();

        IReadOnlyList<string> IEndpointOutcome.Messages => throw new NotImplementedException();

        string? IEndpointOutcome.ErrorMessage => throw new NotImplementedException();

        IResultStatus IEndpointOutcome.Status => throw new NotImplementedException();

        TransportMetadata IEndpointOutcome.Metadata => throw new NotImplementedException();

        /// <inheritdoc/>
        public IResult GetUnderlyingResult() => _innerResult;

        /// <summary>
        /// Creates a successful endpoint outcome with optional transport metadata.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Success(TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Success(), transportMetadata);

        /// <summary>
        /// Creates a failed endpoint outcome from an <see cref="ErrorInfo"/> with
        /// optional transport metadata.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="error"/> is <see langword="null" />.
        /// </exception>
        public static IEndpointOutcome From(
            ErrorInfo error,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new EndpointOutcome(Result.Failure(error), transportMetadata);
        }

        /// <summary>
        /// Creates an endpoint outcome from an <see cref="Zentient.Results.IResult"/>
        /// with optional transport metadata.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>An <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is <see langword="null" />.
        /// </exception>
        public static IEndpointOutcome From(IResult result, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return new EndpointOutcome(result, transportMetadata);
        }

        /// <summary>
        /// Gets the value of the result as an object.
        /// This is primarily used for transport adapters that need to handle the value generically.
        /// For non-generic outcomes, this will always return <see langword="null"/>.
        /// </summary>
        /// <returns>
        /// The value of the result as an <see cref="object"/>,
        /// or <see langword="null"/> if there is no value.
        /// </returns>
        internal virtual object? GetValueAsObject() => null;
    }
}
