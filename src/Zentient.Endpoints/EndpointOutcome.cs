// <copyright file="EndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, non-generic outcome of an endpoint operation.
    /// This type encapsulates an <see cref="Zentient.Results.IResult"/> and
    /// <see cref="TransportMetadata"/>.
    /// </summary>
    public class EndpointOutcome : IEndpointOutcome, IEndpointOutcomeInternal, IEquatable<EndpointOutcome>
    {
        /// <summary>Gets the underlying business result. Initialized during construction.</summary>
        private readonly IResult _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome"/> class.
        /// </summary>
        /// <param name="result">The underlying business result. Cannot be <see langword="null" />.</param>
        /// <param name="metadata">Optional transport metadata. If <see langword="null"/>, a new <see cref="TransportMetadata"/> instance is created.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <see langword="null" />.</exception>
        protected EndpointOutcome(IResult result, TransportMetadata? metadata = null)
        {
            this._innerResult = result ?? throw new ArgumentNullException(nameof(result));
            this.Metadata = metadata ?? new TransportMetadata();
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
        public TransportMetadata Metadata { get; }

        /// <summary>
        /// Creates a successful non-generic endpoint outcome.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Success(TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Success(), transportMetadata);

        /// <summary>
        /// Creates a successful non-generic endpoint outcome with a specific status.
        /// </summary>
        /// <param name="status">The success status (e.g., Created, NoContent).</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Success(IResultStatus status, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Success(status), transportMetadata);

        /// <summary>
        /// Creates a non-generic endpoint outcome from an existing <see cref="Zentient.Results.IResult"/>.
        /// </summary>
        /// <param name="result">The underlying business result. Cannot be <see langword="null" />.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>An <see cref="IEndpointOutcome"/> representing the outcome.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <see langword="null" />.</exception>
        public static IEndpointOutcome From(IResult result, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return new EndpointOutcome(result, transportMetadata);
        }

        /// <summary>
        /// Creates a failed non-generic endpoint outcome from a single <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="error">The error information. Cannot be <see langword="null" />.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is <see langword="null" />.</exception>
        public static IEndpointOutcome FromError(ErrorInfo error, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new EndpointOutcome(Result.Failure(error), transportMetadata);
        }

        /// <summary>
        /// Creates a failed non-generic endpoint outcome from a collection of <see cref="ErrorInfo"/> instances.
        /// </summary>
        /// <param name="errors">The collection of error information. Cannot be <see langword="null" /> or empty.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static IEndpointOutcome FromErrors(IEnumerable<ErrorInfo> errors, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            if (!errors.Any())
            {
                throw new ArgumentException("Errors collection cannot be empty.", nameof(errors));
            }

            return new EndpointOutcome(Result.Failure(errors), transportMetadata);
        }

        /// <summary>
        /// Creates a failed non-generic endpoint outcome representing a "Not Found" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.NotFound"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.NotFound"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome NotFound(string message = ResultStatusConstants.Description.NotFound, string? code = null, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.NotFound(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed non-generic endpoint outcome representing an "Unauthorized" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.Unauthorized"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.Unauthorized"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Unauthorized(string message = ResultStatusConstants.Description.Unauthorized, string? code = null, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Unauthorized(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed non-generic endpoint outcome representing a "Forbidden" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.Forbidden"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.Forbidden"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        public static IEndpointOutcome Forbidden(string message = ResultStatusConstants.Description.Forbidden, string? code = null, TransportMetadata? transportMetadata = null)
            => new EndpointOutcome(Result.Forbidden(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed non-generic endpoint outcome from an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The exception to convert into an error. Cannot be <see langword="null" />.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is <see langword="null" />.</exception>
        public static IEndpointOutcome FromException(Exception ex, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return new EndpointOutcome(Result.FromException(ex), transportMetadata);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is IEndpointOutcome other)
            {
                return this.Status.Equals(other.Status)
                    && this.Errors.SequenceEqual(other.Errors)
                    && this.Messages.SequenceEqual(other.Messages)
                    && string.Equals(this.ErrorMessage, other.ErrorMessage, StringComparison.Ordinal)
                    && this.Metadata.Equals(other.Metadata);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(EndpointOutcome? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            return this.Status.Equals(other.Status)
                && this.Errors.SequenceEqual(other.Errors)
                && this.Messages.SequenceEqual(other.Messages)
                && string.Equals(this.ErrorMessage, other.ErrorMessage, StringComparison.Ordinal)
                && this.Metadata.Equals(other.Metadata);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hash = HashCode.Combine(this.Status);

            var errors = this.Errors;
            for (int i = 0, count = errors.Count; i < count; i++)
            {
                hash = HashCode.Combine(hash, errors[i]);
            }

            var messages = this.Messages;
            for (int i = 0, count = messages.Count; i < count; i++)
            {
                hash = HashCode.Combine(hash, messages[i]);
            }

            hash = HashCode.Combine(hash, this.ErrorMessage);
            hash = HashCode.Combine(hash, this.Metadata);
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString() => "Zentient.Endpoints.EndpointOutcome";

        /// <inheritdoc />
        IResult IEndpointOutcomeInternal.GetUnderlyingResult()
            => this._innerResult;

        /// <summary>
        /// Creates a new <see cref="IEndpointOutcome"/> instance with updated metadata.
        /// </summary>
        /// <param name="metadataTransform">A function to transform the current metadata.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> with the transformed metadata.</returns>
        internal virtual EndpointOutcome WithMetadataInternal(Func<TransportMetadata, TransportMetadata> metadataTransform)
        {
            ArgumentNullException.ThrowIfNull(metadataTransform, nameof(metadataTransform));
            var newMetadata = metadataTransform(this.Metadata);
            return new EndpointOutcome(this._innerResult, newMetadata);
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
