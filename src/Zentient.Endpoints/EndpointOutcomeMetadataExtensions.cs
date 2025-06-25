// <copyright file="EndpointOutcomeMetadataExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using Zentient.Results;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointOutcome"/> and
    /// <see cref="IEndpointOutcome{TValue}"/> to allow fluent metadata modification,
    /// leveraging the internal virtual methods on concrete types.
    /// </summary>
    public static class EndpointOutcomeMetadataExtensions
    {
        /// <summary>
        /// Creates a new <see cref="IEndpointOutcome{TValue}"/> with additional metadata.
        /// This method allows for chaining metadata modifications without altering the original
        /// outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
        /// <param name="outcome">The original endpoint outcome.</param>
        /// <param name="metadataTransform">
        /// A function that takes the current metadata and returns a modified version.
        /// </param>
        /// <returns>
        /// A new <see cref="IEndpointOutcome{TValue}"/> instance with the updated metadata.
        /// </returns>
        /// <remarks>
        /// This method is designed to be used in a fluent manner, allowing for easy extension of
        /// the outcome's metadata without modifying the original instance. It is particularly
        /// useful for adding transport-specific hints, such as HTTP status codes, gRPC status,
        /// or other metadata that may be relevant to the transport layer.
        /// Example usage:
        /// <code>
        /// var outcome = endpointOutcome.WithMetadata(m => m.WithHeader("X-Custom-Header", "Value"));
        /// </code>
        /// The provided function should return a modified version of the metadata, allowing for flexible and
        /// extensible metadata management.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="outcome"/> or <paramref name="metadataTransform"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the provided <paramref name="outcome"/> is not a known concrete implementation.
        /// </exception>
        internal static IEndpointOutcome<TValue> WithMetadata<TValue>(
            this IEndpointOutcome<TValue> outcome,
            Func<TransportMetadata, TransportMetadata> metadataTransform) where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(metadataTransform, nameof(metadataTransform));

            if (outcome is EndpointOutcome<TValue> concreteOutcome)
            {
                return concreteOutcome.WithMetadata(metadataTransform);
            }

            throw new InvalidOperationException($"Cannot apply metadata transform to outcome of type {outcome.GetType().Name}. " +
                                                $"Expected an instance of {typeof(EndpointOutcome<TValue>).Name}.");
        }

        /// <summary>
        /// Creates a new <see cref="IEndpointOutcome"/> with additional metadata.
        /// This method allows for chaining metadata modifications without altering the original
        /// outcome.
        /// </summary>
        /// <param name="outcome">The original endpoint outcome.</param>
        /// <param name="metadataTransform">
        /// A function that takes the current metadata and returns a modified version.
        /// </param>
        /// <returns>
        /// A new <see cref="IEndpointOutcome"/> instance with the updated metadata.
        /// </returns>
        /// <remarks>
        /// This method is designed to be used in a fluent manner, allowing for easy extension of
        /// the outcome's metadata without modifying the original instance. It is particularly
        /// useful for adding transport-specific hints, such as HTTP status codes, gRPC status,
        /// or other metadata that may be relevant to the transport layer.
        /// Example usage:
        /// <code>
        /// var outcome = endpointOutcome.WithMetadata(m => m.WithHeader("X-Custom-Header", "Value"));
        /// </code>
        /// The provided function should return a modified version of the metadata, allowing for
        /// flexible and extensible metadata management.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="outcome"/> or <paramref name="metadataTransform"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the provided <paramref name="outcome"/> is not a known concrete implementation.
        /// </exception>
        internal static IEndpointOutcome WithMetadata(
            this IEndpointOutcome outcome,
            Func<TransportMetadata, TransportMetadata> metadataTransform)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(metadataTransform, nameof(metadataTransform));

            if (outcome is EndpointOutcome concreteOutcome)
            {
                return concreteOutcome.WithMetadata(metadataTransform);
            }

            throw new InvalidOperationException($"Cannot apply metadata transform to outcome of type {outcome.GetType().Name}. " +
                                                $"Expected an instance of {typeof(EndpointOutcome).Name} or its derivative.");
        }
    }
}
