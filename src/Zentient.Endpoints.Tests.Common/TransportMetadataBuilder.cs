// <copyright file="TransportMetadataHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zentient.Endpoints.Http.Extensions;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides a fluent builder for creating <see cref="TransportMetadata"/> instances for Zentient Endpoints HTTP-related tests.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    public sealed class TransportMetadataBuilder
    {
        private TransportMetadata _metadata = new TransportMetadata();

        /// <summary>
        /// Adds a logger to the transport metadata.
        /// </summary>
        public TransportMetadataBuilder WithLogger(ILogger logger)
        {
            _metadata = _metadata.WithLogger(logger);
            return this;
        }

        /// <summary>
        /// Adds an HTTP status code hint to the transport metadata.
        /// </summary>
        public TransportMetadataBuilder WithStatusHint(int statusCode)
        {
            _metadata = _metadata.WithHttpStatusCodeHint(statusCode);
            return this;
        }

        /// <summary>
        /// Adds a ProblemDetails override to the transport metadata.
        /// </summary>
        public TransportMetadataBuilder WithProblemDetails(ProblemDetails pd)
        {
            _metadata = _metadata.WithProblemDetailsOverride(pd);
            return this;
        }

        /// <summary>
        /// Adds a custom header to the transport metadata.
        /// </summary>
        public TransportMetadataBuilder WithHeader(string key, string value)
        {
            _metadata = _metadata.WithHeader(key, value);
            return this;
        }

        /// <summary>
        /// Adds a Location URI to the transport metadata.
        /// </summary>
        public TransportMetadataBuilder WithLocation(Uri location)
        {
            _metadata = _metadata.WithLocation(location);
            return this;
        }

        /// <summary>
        /// Builds and returns the configured <see cref="TransportMetadata"/>.
        /// </summary>
        public TransportMetadata Build() => _metadata;

        /// <summary>
        /// Static helper for legacy code: builds a TransportMetadata using the builder pattern.
        /// </summary>
        public static TransportMetadata CreateTransportMetadata(
            int? httpStatusCodeHint = null,
            ProblemDetails? problemDetailsOverride = null,
            ILogger? logger = null,
            System.Collections.Immutable.ImmutableDictionary<string, string>? headers = null,
            Uri? locationUri = null)
        {
            var builder = new TransportMetadataBuilder();
            if (logger != null)
            {
                builder.WithLogger(logger);
            }

            if (httpStatusCodeHint.HasValue)
            {
                builder.WithStatusHint(httpStatusCodeHint.Value);
            }

            if (problemDetailsOverride != null)
            {
                builder.WithProblemDetails(problemDetailsOverride);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    builder.WithHeader(header.Key, header.Value);
                }
            }

            if (locationUri != null)
            {
                builder.WithLocation(locationUri);
            }

            return builder.Build();
        }

        /// <summary>
        /// This method is obsolete. Use your own logger mock or NullLogger in tests.
        /// </summary>
        [Obsolete("Use your own logger mock or NullLogger<T> in tests.")]
        public static ILogger CreateMockLogger()
        {
            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }
    }
}
