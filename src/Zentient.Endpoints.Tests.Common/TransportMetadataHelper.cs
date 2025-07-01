// <copyright file="TransportMetadataHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable; // For ImmutableDictionary
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;             // For ILogger

using Moq;

using Zentient.Endpoints.Http.Extensions;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides sophisticated helper methods for creating mock objects and test contexts
    /// for Zentient Endpoints HTTP-related tests.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    public static class TransportMetadataHelper
    {
        /// <summary>
        /// Creates a <see cref="TransportMetadata"/> instance, fluently applying HTTP-specific tags.
        /// This helper uses the same extension methods as application code for consistency.
        /// </summary>
        /// <param name="httpStatusCodeHint">Optional HTTP status code hint.</param>
        /// <param name="problemDetailsOverride">Optional ProblemDetails object to override default mapping.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> instance to include in metadata.</param>
        /// <param name="headers">Optional custom HTTP headers to include.</param>
        /// <param name="locationUri">Optional URI for the Location header.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with specified tags.</returns>
        public static TransportMetadata CreateTransportMetadata(
            int? httpStatusCodeHint = null,
            ProblemDetails? problemDetailsOverride = null,
            ILogger? logger = null,
            ImmutableDictionary<string, string>? headers = null,
            Uri? locationUri = null)
        {
            TransportMetadata metadata = new TransportMetadata();

            if (logger != null)
            {
                metadata = metadata.WithLogger(logger);
            }

            if (httpStatusCodeHint.HasValue)
            {
                metadata = metadata.WithHttpStatusCodeHint(httpStatusCodeHint.Value);
            }

            if (problemDetailsOverride != null)
            {
                metadata = metadata.WithProblemDetailsOverride(problemDetailsOverride);
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    metadata = metadata.WithHeader(header.Key, header.Value);
                }
            }

            if (locationUri != null)
            {
                metadata = metadata.WithLocation(locationUri);
            }

            return metadata;
        }

        /// <summary>
        /// Creates a mock <see cref="ILogger"/> instance for testing purposes.
        /// This is useful for scenarios where you need to inject a logger
        /// without requiring a real logging implementation.
        /// </summary>
        /// <returns>A mocked <see cref="ILogger"/> object.</returns>
        public static ILogger CreateMockLogger()
        {
            var loggerMock = new Mock<ILogger>();
            return loggerMock.Object;
        }
    }
}
