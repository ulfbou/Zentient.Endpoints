// <copyright file="TransportMetadataBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics.CodeAnalysis;

using Zentient.Endpoints;
using Zentient.Endpoints.Builders;
using Zentient.Endpoints.Http.Extensions;

namespace Zentient.Endpoints.Builders
{
    /// <summary>
    /// Provides extension methods for <see cref="TransportMetadataBuilder"/> to add HTTP-specific metadata in a fluent style.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test/helper extension class.")]
    public static class TransportMetadataBuilderExtensions
    {
        /// <summary>
        /// Adds an HTTP status code hint to the transport metadata.
        /// </summary>
        /// <param name="builder">The <see cref="TransportMetadataBuilder"/> instance.</param>
        /// <param name="statusCodeHint">The HTTP status code to add as a hint.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public static TransportMetadataBuilder WithStatusHint(this TransportMetadataBuilder builder, int statusCodeHint)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata = builder.Metadata.WithHttpStatusCodeHint(statusCodeHint);
            return builder;
        }

        /// <summary>
        /// Adds a <see cref="ProblemDetails"/> override to the transport metadata.
        /// </summary>
        /// <param name="builder">The <see cref="TransportMetadataBuilder"/> instance.</param>
        /// <param name="pd">The <see cref="ProblemDetails"/> to add as an override.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public static TransportMetadataBuilder WithProblemDetails(this TransportMetadataBuilder builder, ProblemDetails pd)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata = builder.Metadata.WithProblemDetailsOverride(pd);
            return builder;
        }

        /// <summary>
        /// Adds a custom header to the transport metadata.
        /// </summary>
        /// <param name="builder">The <see cref="TransportMetadataBuilder"/> instance.</param>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public static TransportMetadataBuilder WithHeader(this TransportMetadataBuilder builder, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata = builder.Metadata.WithHeader(key, value);
            return builder;
        }

        /// <summary>
        /// Adds a Location URI to the transport metadata.
        /// </summary>
        /// <param name="builder">The <see cref="TransportMetadataBuilder"/> instance.</param>
        /// <param name="location">The <see cref="Uri"/> to use for the Location header.</param>
        /// <returns>The <see cref="TransportMetadataBuilder"/> instance for chaining.</returns>
        public static TransportMetadataBuilder WithLocation(this TransportMetadataBuilder builder, Uri location)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Metadata = builder.Metadata.WithLocation(location);
            return builder;
        }
    }
}
