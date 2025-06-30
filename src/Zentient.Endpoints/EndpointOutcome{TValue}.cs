// <copyright file="EndpointOutcome{TValue}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents the concrete, generic outcome of an endpoint operation that produces a value.
    /// This type encapsulates a <see cref="Zentient.Results.IResult{TValue}"/> and
    /// <see cref="TransportMetadata"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
    internal sealed class EndpointOutcome<TValue> : EndpointOutcome, IEndpointOutcome<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcome{TValue}"/> class.
        /// </summary>
        /// <param name="result">The underlying business result. Cannot be <see langword="null" />.</param>
        /// <param name="metadata">Optional transport metadata. If <see langword="null"/>, a new <see cref="TransportMetadata"/> instance is created.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="result"/> is <see langword="null" />.
        /// </exception>
        internal EndpointOutcome(IResult<TValue> result, TransportMetadata? metadata = null)
            : base(result, metadata)
        { }

        /// <inheritdoc/>
        public TValue? Value =>
            ((IResult<TValue>)((IEndpointOutcomeInternal)this)
                .GetUnderlyingResult()).Value;

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
        /// Creates a successful endpoint outcome with a value and a specific status.
        /// </summary>
        /// <param name="value">The value to encapsulate.</param>
        /// <param name="status">The success status (e.g., Created, Accepted).</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A successful <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static IEndpointOutcome<TValue> Success(
            TValue value,
            IResultStatus status,
            TransportMetadata? transportMetadata = null) =>
            new EndpointOutcome<TValue>(Result<TValue>.Success(value, status), transportMetadata);

        /// <summary>
        /// Creates a successful endpoint outcome indicating no content.
        /// </summary>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>
        /// A successful <see cref="IEndpointOutcome{TValue}"/> representing no content.
        /// </returns>
        /// <remarks>
        /// Delegates to <c>Result&lt;TValue&gt;.NoContent()</c> for consistent handling
        /// of no-content scenarios across all TValue types.
        /// </remarks>
        public static IEndpointOutcome<TValue> NoContent(TransportMetadata? transportMetadata = null)
        {
            return new EndpointOutcome<TValue>(Result<TValue>.NoContent(), transportMetadata);
        }

        /// <summary>
        /// Creates a failed generic endpoint outcome from a single <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="error">The error information. Cannot be <see langword="null" />.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is <see langword="null" />.</exception>
        public static new IEndpointOutcome<TValue> FromError(
            ErrorInfo error,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            return new EndpointOutcome<TValue>(Result<TValue>.Failure(error), transportMetadata);
        }

        /// <summary>
        /// Creates a failed generic endpoint outcome from a collection of <see cref="ErrorInfo"/> instances.
        /// </summary>
        /// <param name="errors">The collection of error information. Cannot be <see langword="null" /> or empty.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static new IEndpointOutcome<TValue> FromErrors(
            IEnumerable<ErrorInfo> errors,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));

            if (!errors.Any())
            {
                throw new ArgumentException("Errors collection cannot be empty.", nameof(errors));
            }

            return new EndpointOutcome<TValue>(Result<TValue>.Failure(errors), transportMetadata);
        }

        /// <summary>
        /// Creates an endpoint outcome from an <see cref="Zentient.Results.IResult{TValue}"/>
        /// with optional transport metadata.
        /// </summary>
        /// <param name="result">The underlying business result. Cannot be <see langword="null" />.</param>
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

        /// <summary>
        /// Creates a failed generic endpoint outcome representing a "Not Found" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.NotFound"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.NotFound"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static new IEndpointOutcome<TValue> NotFound(
            string message = ResultStatusConstants.Description.NotFound,
            string? code = null,
            TransportMetadata? transportMetadata = null)
            => new EndpointOutcome<TValue>(Result<TValue>.NotFound(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed generic endpoint outcome representing an "Unauthorized" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.Unauthorized"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.Unauthorized"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static new IEndpointOutcome<TValue> Unauthorized(
            string message = ResultStatusConstants.Description.Unauthorized,
            string? code = null,
            TransportMetadata? transportMetadata = null)
            => new EndpointOutcome<TValue>(Result<TValue>.Unauthorized(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed generic endpoint outcome representing a "Forbidden" scenario.
        /// </summary>
        /// <param name="message">A descriptive error message. Defaults to <see cref="ResultStatusConstants.Description.Forbidden"/>.</param>
        /// <param name="code">Optional error code. Defaults to <see cref="ResultStatusConstants.Code.Forbidden"/>.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        public static new IEndpointOutcome<TValue> Forbidden(
            string message = ResultStatusConstants.Description.Forbidden,
            string? code = null,
            TransportMetadata? transportMetadata = null)
            => new EndpointOutcome<TValue>(Result<TValue>.Forbidden(message, code), transportMetadata);

        /// <summary>
        /// Creates a failed generic endpoint outcome from an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The exception to convert into an error. Cannot be <see langword="null" />.</param>
        /// <param name="transportMetadata">Optional transport metadata.</param>
        /// <returns>A failed <see cref="IEndpointOutcome{TValue}"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is <see langword="null" />.</exception>
        public static new IEndpointOutcome<TValue> FromException(
            Exception ex,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(ex, nameof(ex));
            return new EndpointOutcome<TValue>(Result<TValue>.FromException(default, ex, null), transportMetadata);
        }

        /// <summary>
        /// Creates a new <see cref="IEndpointOutcome{TValue}"/> instance with updated metadata.
        /// </summary>
        /// <param name="metadataTransform">A function to transform the current metadata.</param>
        /// <returns>A new <see cref="IEndpointOutcome{TValue}"/> with the transformed metadata.</returns>
        internal override EndpointOutcome WithMetadataInternal(Func<TransportMetadata, TransportMetadata> metadataTransform)
        {
            ArgumentNullException.ThrowIfNull(metadataTransform, nameof(metadataTransform));
            var newMetadata = metadataTransform(Metadata);
            // Ensure we pass IResult<TValue> to the generic constructor
            return new EndpointOutcome<TValue>((IResult<TValue>)UnderlyingResult, newMetadata);
        }

        /// <inheritdoc />
        public override string ToString() => GetType().ToString();


        /// <inheritdoc/>
        internal override object? GetValueAsObject() => ((IResult<TValue>)UnderlyingResult).Value;
    }
}
