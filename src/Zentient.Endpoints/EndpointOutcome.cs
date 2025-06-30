// <copyright file="EndpointOutcome.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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
        public IResult UnderlyingResult => _innerResult;

        /// <inheritdoc/>
        public TransportMetadata Metadata { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome"/> class.
        /// </summary>
        /// <param name="result">The underlying business result. Cannot be <see langword="null" />.</param>
        /// <param name="metadata">Optional transport metadata. If <see langword="null"/>, a new <see cref="TransportMetadata"/> instance is created.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <see langword="null" />.</exception>
        protected EndpointOutcome(IResult result, TransportMetadata? metadata = null)
        {
            _innerResult = result ?? throw new ArgumentNullException(nameof(result));
            Metadata = metadata ?? new TransportMetadata();
        }

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
                return true;

            // Support value-based equality for IEndpointOutcome (including mocks)
            if (obj is IEndpointOutcome other)
            {
                return Status.Equals(other.Status)
                    && Errors.SequenceEqual(other.Errors)
                    && Messages.SequenceEqual(other.Messages)
                    && string.Equals(ErrorMessage, other.ErrorMessage, StringComparison.Ordinal)
                    && Metadata.Equals(other.Metadata);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(EndpointOutcome? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;

            return Status.Equals(other.Status)
                && Errors.SequenceEqual(other.Errors)
                && Messages.SequenceEqual(other.Messages)
                && string.Equals(ErrorMessage, other.ErrorMessage, StringComparison.Ordinal)
                && Metadata.Equals(other.Metadata);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Status);
            foreach (var error in Errors)
                hash.Add(error);
            foreach (var message in Messages)
                hash.Add(message);
            hash.Add(ErrorMessage);
            hash.Add(Metadata);
            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() => "Zentient.Endpoints.EndpointOutcome";

        /// <summary>
        /// Creates a new <see cref="IEndpointOutcome"/> instance with updated metadata.
        /// </summary>
        /// <param name="metadataTransform">A function to transform the current metadata.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> with the transformed metadata.</returns>
        internal virtual EndpointOutcome WithMetadataInternal(Func<TransportMetadata, TransportMetadata> metadataTransform)
        {
            ArgumentNullException.ThrowIfNull(metadataTransform, nameof(metadataTransform));
            var newMetadata = metadataTransform(Metadata);
            return new EndpointOutcome(_innerResult, newMetadata);
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

        /// <summary>
        /// Gets the underlying <see cref="Zentient.Results.IResult"/> instance.
        /// This is used internally to access the raw result for further processing or inspection.
        /// </summary>
        /// <returns>The underlying <see cref="Zentient.Results.IResult"/> instance.</returns>
        internal virtual IResult GetUnderlyingResult()
        {
            return _innerResult;
        }

        /// <inheritdoc />
        IResult IEndpointOutcomeInternal.GetUnderlyingResult()
            => _innerResult;
    }
}
