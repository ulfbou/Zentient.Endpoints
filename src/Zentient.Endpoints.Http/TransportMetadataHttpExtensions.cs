// <copyright file="TransportMetadataHttpExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zentient;
using Zentient.Endpoints;
using Zentient.Endpoints.Http;
using Zentient.Endpoints.Http.Constants;

namespace Zentient.Endpoints.Http.Extensions
{
    /// <summary>
    /// Provides public HTTP-specific extension methods for <see cref="TransportMetadata"/>
    /// to enrich or retrieve its tags for HTTP response generation. These methods are for
    /// direct manipulation of <see cref="TransportMetadata"/> instances.
    /// </summary>
    public static class TransportMetadataHttpExtensions
    {
        /// <summary>
        /// Fluently sets an HTTP status code hint on the <see cref="TransportMetadata"/> tags.
        /// This hint guides the HTTP mapper in determining the final HTTP status code.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <param name="statusCode">The HTTP status code to hint.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with the updated tag.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        public static TransportMetadata WithHttpStatusCodeHint(
            this TransportMetadata metadata,
            int statusCode)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            return metadata.WithTag(HttpMetadataKeys.HttpStatusCodeHint, statusCode);
        }

        /// <summary>
        /// Fluently attaches a pre-built <see cref="ProblemDetails"/> object to the
        /// <see cref="TransportMetadata"/> tags.
        /// This will bypass the default ProblemDetails mapping logic in the HTTP mapper.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> object to attach.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with the updated tag.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> or <paramref name="problemDetails"/> is null.
        /// </exception>
        public static TransportMetadata WithProblemDetailsOverride(
            this TransportMetadata metadata,
            ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            ArgumentNullException.ThrowIfNull(problemDetails, nameof(problemDetails));
            return metadata.WithTag(HttpMetadataKeys.ProblemDetailsOverride, problemDetails);
        }

        /// <summary>
        /// Fluently adds or updates a custom HTTP header to the <see cref="TransportMetadata"/>
        /// tags. Headers are stored as a nested immutable dictionary.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <param name="key">The header key (e.g., "X-Correlation-ID").</param>
        /// <param name="value">The header value.</param>
        /// <returns>
        /// A new <see cref="TransportMetadata"/> instance with the updated header tag.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> or <paramref name="value"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null or whitespace.
        /// </exception>
        public static TransportMetadata WithHeader(
            this TransportMetadata metadata,
            string key,
            string value)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            metadata.TryGetTag(HttpMetadataKeys.Headers, out ImmutableDictionary<string, string>? currentHeaders);
            currentHeaders ??= ImmutableDictionary<string, string>.Empty;

            var newHeaders = currentHeaders.SetItem(key, value);
            return metadata.WithTag(HttpMetadataKeys.Headers, newHeaders);
        }

        /// <summary>
        /// Fluently sets the Location HTTP header URI on the <see cref="TransportMetadata"/>
        /// tags using a string.
        /// If the provided <paramref name="uriString"/> is null, empty, or invalid, the original
        /// <see cref="TransportMetadata"/> is returned unchanged and a warning is logged
        /// (if an <see cref="ILogger"/> is available).
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <param name="uriString">The URI string for the Location header.</param>
        /// <returns>
        /// A new <see cref="TransportMetadata"/> instance with the updated tag, or the original
        /// instance if invalid.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        /// <remarks>
        /// This method does not throw an exception for invalid URI strings.
        /// </remarks>
        [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerMessage delegates are not used here to keep the code simple and avoid additional boilerplate.")]
        public static TransportMetadata WithLocation(
            this TransportMetadata metadata,
            string uriString)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

            var logger = metadata.GetLogger();

            if (string.IsNullOrWhiteSpace(uriString))
            {
                logger?.LogWarning("WithLocation received null or empty URI string. Location header will not be set.");
                return metadata;
            }

            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            {
                logger?.LogWarning("WithLocation received invalid URI string: '{UriString}'. Location header will not be set.", uriString);
                return metadata;
            }

            return metadata.WithTag(HttpMetadataKeys.LocationUri, uri);
        }

        /// <summary>
        /// Fluently sets the Location HTTP header URI on the <see cref="TransportMetadata"/> tags
        /// using a <see cref="Uri"/> object.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <param name="uri">The <see cref="Uri"/> for the Location header.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with the updated tag.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> or <paramref name="uri"/> is null.
        /// </exception>
        [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerMessage delegates are not used here to keep the code simple and avoid additional boilerplate.")]
        public static TransportMetadata WithLocation(this TransportMetadata metadata, Uri uri)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));
            return metadata.WithTag(HttpMetadataKeys.LocationUri, uri);
        }

        /// <summary>
        /// Retrieves the HTTP status code hint from the <see cref="TransportMetadata"/> tags.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <returns>The HTTP status code hint, or null if not set or not an integer.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        public static int? GetHttpStatusCodeHint(this TransportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            return metadata.TryGetTag(HttpMetadataKeys.HttpStatusCodeHint, out int? statusCode) ? statusCode : null;
        }

        /// <summary>
        /// Retrieves the <see cref="ProblemDetails"/> override object from the
        /// <see cref="TransportMetadata"/> tags, if present.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <returns>
        /// The <see cref="ProblemDetails"/> object if set as an override in the metadata tags;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        public static ProblemDetails? GetProblemDetailsOverride(this TransportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            return metadata.TryGetTag(HttpMetadataKeys.ProblemDetailsOverride, out ProblemDetails? problemDetails) ? problemDetails : null;
        }

        /// <summary>
        /// Retrieves custom HTTP headers from the <see cref="TransportMetadata"/> tags.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <returns>
        /// An <see cref="IReadOnlyDictionary{TKey, TValue}"/> of header names and values,
        /// or an empty dictionary if no headers are set or the tag is not of the correct type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        public static IReadOnlyDictionary<string, string> GetHeaders(this TransportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            return metadata.TryGetTag(HttpMetadataKeys.Headers, out ImmutableDictionary<string, string>? headers)
                ? headers
                : ImmutableDictionary<string, string>.Empty;
        }

        /// <summary>
        /// Retrieves the Location <see cref="Uri"/> from the <see cref="TransportMetadata"/> tags.
        /// </summary>
        /// <param name="metadata">The current transport metadata instance.</param>
        /// <returns>
        /// The Location <see cref="Uri"/>, or null if not set or not a valid URI object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="metadata"/> is null.
        /// </exception>
        public static Uri? GetLocationUri(this TransportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            return metadata.TryGetTag(HttpMetadataKeys.LocationUri, out Uri? uri) ? uri : null;
        }
    }
}
