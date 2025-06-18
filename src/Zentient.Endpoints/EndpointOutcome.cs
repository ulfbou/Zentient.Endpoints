// <copyright file="EndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, non-generic outcome of an endpoint operation.
    /// This type encapsulates a <see cref="IResult"/> and <see cref="TransportMetadata"/>.
    /// </summary>
    internal sealed class EndpointOutcome : IEndpointOutcome
    {
        /// <summary>Gets the underlying business result.</summary>
        private readonly IResult _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome"/> class.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        private EndpointOutcome(IResult result, TransportMetadata? transportMetadata = null)
        {
            this._innerResult = result ?? throw new ArgumentNullException(nameof(result));
            this.TransportMetadata = transportMetadata ?? new TransportMetadata();
        }

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
        /// Creates a successful endpoint outcome with optional transport metadata.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Success(TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Success(), transportMetadata);

        /// <summary>
        /// Creates a failed endpoint outcome from an <see cref="ErrorInfo"/> with optional transport metadata.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is <c>null</c>.</exception>
        public static IEndpointOutcome From(ErrorInfo error, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Failure(error), transportMetadata);

        /// <summary>
        /// Creates an endpoint outcome from an <see cref="IResult"/> with optional transport metadata.
        /// </summary>
        /// <param name="result">The underlying business result.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>An <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IEndpointOutcome From(IResult result, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(result, transportMetadata);
    }
}
