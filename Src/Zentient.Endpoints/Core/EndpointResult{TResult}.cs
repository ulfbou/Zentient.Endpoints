// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointResult{TResult}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Zentient.Results;

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Represents the result of an endpoint operation, including the business result and transport metadata.
    /// </summary>
    /// <typeparam name="TResult">The type of the value returned by the operation.</typeparam>
    public sealed class EndpointResult<TResult> : IEndpointResult<TResult>
        where TResult : notnull
    {
        /// <summary>
        /// Gets the internal <see cref="IResult{TResult}"/> representing the business outcome.
        /// </summary>
        required public IResult<TResult> InnerResult { get; init; }

        /// <summary>
        /// Gets the underlying business result of the operation in a non-generic form.
        /// Implements <see cref="IEndpointResult.BaseResult"/>.
        /// </summary>
        public IResult BaseResult => this.InnerResult;

        /// <summary>
        /// Gets the strongly-typed value of the business result.
        /// Implements <see cref="IEndpointResult{TResult}.Result"/>.
        /// </summary>
        public TResult Result => this.InnerResult.IsSuccess
            ? this.InnerResult.Value!
            : throw new InvalidOperationException("Cannot access Result.Value when InnerResult is not successful.");

        /// <summary>
        /// Gets the transport-agnostic metadata associated with this endpoint result.
        /// </summary>
        required public TransportMetadata BaseTransport { get; init; }

        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess => this.InnerResult.IsSuccess;

        /// <summary>
        /// Gets the error associated with the operation, if any.
        /// </summary>
        public ErrorInfo? Error
        {
            get
            {
                IReadOnlyList<ErrorInfo> errors = this.InnerResult.Errors;
                return (errors == null || errors.Count == 0) ? null : errors[0];
            }
        }

        /// <summary>
        /// Creates a successful <see cref="EndpointResult{TResult}"/> with the specified value and optional metadata.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <param name="meta">The transport metadata to use, or <c>null</c> for default.</param>
        /// <returns>A successful <see cref="EndpointResult{TResult}"/>.</returns>
        public static EndpointResult<TResult> From(TResult value, TransportMetadata? meta = null)
        {
            return new EndpointResult<TResult>
            {
                InnerResult = Zentient.Results.Result.Success(value),
                BaseTransport = meta ?? TransportMetadata.Default(),
            };
        }

        /// <summary>
        /// Creates an <see cref="EndpointResult{TResult}"/> from the specified <see cref="IResult{TResult}"/> and optional transport metadata.
        /// </summary>
        /// <param name="result">The business result to wrap.</param>
        /// <param name="meta">The transport metadata to use, or <c>null</c> for default metadata.</param>
        /// <returns>
        /// An <see cref="EndpointResult{TResult}"/> containing the provided result and transport metadata.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static EndpointResult<TResult> From(IResult<TResult> result, TransportMetadata? meta = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            return new EndpointResult<TResult>
            {
                InnerResult = result,
                BaseTransport = meta ?? TransportMetadata.Default(),
            };
        }

        /// <summary>
        /// Creates a failed <see cref="EndpointResult{TResult}"/> with the specified error and optional metadata.
        /// </summary>
        /// <param name="error">The error to return.</param>
        /// <param name="meta">The transport metadata to use, or <c>null</c> for default.</param>
        /// <returns>A failed <see cref="EndpointResult{TResult}"/>.</returns>
        public static EndpointResult<TResult> From(ErrorInfo error, TransportMetadata? meta = null)
        {
            if (error.Equals(default))
            {
                throw new ArgumentNullException(nameof(error), "Error cannot be null.");
            }

            return new EndpointResult<TResult>
            {
                InnerResult = Zentient.Results.Result.Failure<TResult>(error),
                BaseTransport = meta ?? TransportMetadata.Default(),
            };
        }

        /// <summary>
        /// Maps the value of the result to a new type using the specified mapping function.
        /// </summary>
        /// <typeparam name="TNewResult">The type to map the value to.</typeparam>
        /// <param name="map">The mapping function.</param>
        /// <returns>A new <see cref="EndpointResult{TNewResult}"/> with the mapped value and the same transport metadata.</returns>
        public EndpointResult<TNewResult> Map<TNewResult>(Func<TResult, TNewResult> map)
            where TNewResult : notnull
        {
            ArgumentNullException.ThrowIfNull(map, nameof(map));

            return new EndpointResult<TNewResult>
            {
                InnerResult = this.InnerResult.Map(map),
                BaseTransport = this.BaseTransport,
            };
        }

        /// <summary>
        /// Binds the value of the result to a new <see cref="EndpointResult{TNewResult}"/> using the specified binder function.
        /// </summary>
        /// <typeparam name="TNewResult">The type to bind the value to.</typeparam>
        /// <param name="binder">The binder function.</param>
        /// <returns>
        /// The result of the binder function if the operation was successful; otherwise, a failed <see cref="EndpointResult{TNewResult}"/>.
        /// </returns>
        public EndpointResult<TNewResult> Bind<TNewResult>(Func<TResult, EndpointResult<TNewResult>> binder)
            where TNewResult : notnull
        {
            ArgumentNullException.ThrowIfNull(binder, nameof(binder));

            return this.IsSuccess
                ? binder(this.Result)
                : EndpointResult<TNewResult>.From(this.Error ?? throw new InvalidOperationException("No error information available."), this.BaseTransport);
        }

        /// <summary>
        /// Deconstructs the <see cref="EndpointResult{TResult}"/> into its value and transport metadata.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        /// <param name="metadata">The transport metadata associated with the result.</param>
        /// <remarks>
        /// This method allows for easy extraction of the value and metadata
        /// from an <see cref="EndpointResult{TResult}"/> instance,
        /// facilitating pattern matching and tuple deconstruction.
        /// </remarks>
        public void Deconstruct(out TResult value, out TransportMetadata metadata)
        {
            value = this.Result;
            metadata = this.BaseTransport;
        }
    }
}
