// <copyright file="TransportMetadataBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Builders
{
    /// <summary>
    /// Provides a fluent builder for creating <see cref="TransportMetadata"/> instances.
    /// This builder is intended for use in tests and scenarios where programmatic construction
    /// of transport metadata with custom tags and logger is required.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    public sealed class TransportMetadataBuilder
    {
        /// <summary>
        /// Gets a dictionary of arbitrary, request-scoped data or services, referred to as tags.
        /// </summary>
        /// <remarks>
        /// Tags provide a flexible mechanism for associating custom data or services with the transport metadata.
        /// Protocol-specific hints (e.g., HTTP status code, headers, ProblemDetails) are stored here using
        /// well-defined string keys (e.g., "http.status_code", "http.headers").
        /// </remarks>
        /// <value>An immutable dictionary mapping string keys to object values, which may be null.</value>
        private TransportMetadata _metadata = new TransportMetadata();

        /// <summary>
        /// Gets or sets the current <see cref="TransportMetadata"/> instance being built.
        /// </summary>
        /// <value>
        /// The <see cref="TransportMetadata"/> instance that holds the current state of the builder.
        /// </value>
        internal TransportMetadata Metadata
        {
            get => _metadata;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _metadata = value;
            }
        }

        /// <summary>Adds a logger to the transport metadata.</summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to associate with the metadata.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public TransportMetadataBuilder WithLogger(ILogger logger)
        {
            _metadata = _metadata.WithLogger(logger);
            return this;
        }

        /// <summary>
        /// Adds or updates a tag in the transport metadata.
        /// </summary>
        /// <param name="key">The tag key. Must not be null or whitespace.</param>
        /// <param name="value">The tag value to associate with the key.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public TransportMetadataBuilder WithTag(string key, object? value)
        {
            _metadata = _metadata.WithTag(key, value);
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="TransportMetadata"/> instance.
        /// </summary>
        /// <returns>The constructed <see cref="TransportMetadata"/>.</returns>
        public TransportMetadata Build() => _metadata;
    }
}
