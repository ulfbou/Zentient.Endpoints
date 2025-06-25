// <copyright file="TransportMetadata.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Zentient.Endpoints.Constants;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents transport-agnostic metadata associated with an endpoint outcome.
    /// This is a sealed record for immutable, fluent updates.
    /// All protocol-specific hints (e.g., HTTP status codes, headers) are stored in the Tags dictionary.
    /// </summary>
    public sealed record TransportMetadata
    {
        /// <summary>Gets an empty <see cref="TransportMetadata"/> instance with no tags.</summary>
        public static TransportMetadata Empty { get; } = new TransportMetadata(ImmutableDictionary<string, object?>.Empty);

        /// <summary>Gets a dictionary of arbitrary, request-scoped data or services, referred to as tags.</summary>
        /// <remarks>
        /// Tags provide a flexible mechanism for associating custom data or services with the transport metadata.
        /// Protocol-specific hints (e.g., HTTP status code, headers, ProblemDetails) are stored here using
        /// well-defined string keys (e.g., "http.status", "http.headers").
        /// </remarks>
        /// <value>An immutable dictionary mapping string keys to object values, which may be null.</value>
        public ImmutableDictionary<string, object?> Tags { get; init; }

        /// <summary>Initializes a new instance of the <see cref="TransportMetadata"/> record with the specified tags.</summary>
        /// <param name="tags">An immutable dictionary of tags to associate with this metadata.</param>
        private TransportMetadata(ImmutableDictionary<string, object?> tags)
        {
            Tags = tags ?? ImmutableDictionary<string, object?>.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="TransportMetadata"/> record with an empty tags dictionary.</summary>
        /// <remarks>
        /// This constructor initializes the metadata with an empty tags dictionary,
        /// allowing for subsequent fluent updates to add tags as needed.
        /// </remarks>
        public TransportMetadata() : this(ImmutableDictionary<string, object?>.Empty) { }

        /// <summary>
        /// Creates a new <see cref="TransportMetadata"/> instance from a collection of initial tags.
        /// </summary>
        /// <param name="initialTags">A dictionary of key-value pairs for initial tags.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance to associate with the metadata, if desired.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance.</returns>
        public static TransportMetadata From(
            IDictionary<string, object?> initialTags,
            ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(initialTags, nameof(initialTags));

            var builder = initialTags.ToImmutableDictionary().ToBuilder();

            if (logger is not null)
            {
                builder[Constants.MetadataKeys.Logger] = logger;
            }

            return new TransportMetadata(builder.ToImmutable());
        }

        #region ToString
        /// <inheritdoc />
        public override string ToString()
        {
            if (Tags.IsEmpty)
            {
                return "TransportMetadata { Tags: {} }";
            }

            var tagStrings = Tags.Select(kvp =>
            {
                string valueString;

                if (kvp.Value is ProblemDetails pd)
                {
                    valueString = $"ProblemDetails (Type: {pd.Type}, Title: {pd.Title})";
                }
                else if (kvp.Value is ILogger)
                {
                    valueString = "ILogger instance";
                }
                else if (kvp.Value is ImmutableDictionary<string, string> headers)
                {
                    valueString = $"Headers ({headers.Count} items)";
                }
                else if (kvp.Value != null)
                {
                    valueString = kvp.Value.ToString() ?? "(null string)";

                    if (valueString.Length > 100)
                    {
                        valueString = string.Concat(valueString.AsSpan(0, 97), "...");
                    }
                }
                else
                {
                    valueString = "null";
                }

                return $"{kvp.Key}: {valueString}";
            });

            return $"TransportMetadata {{ Tags: {{ {string.Join(", ", tagStrings)} }} }}";
        }
        #endregion

        /// <summary>Returns a new <see cref="TransportMetadata"/> instance with the specified tag set or updated.</summary>
        /// <param name="key">The tag key. Must not be null or whitespace.</param>
        /// <param name="value">The tag value to associate with the key.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with the updated tag.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null, empty, or consists only of white-space characters.
        /// </exception>
        internal TransportMetadata SetTag(string key, object? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            return this with { Tags = Tags.SetItem(key, value) };
        }

        /// <summary>Returns a new <see cref="TransportMetadata"/> instance with the a specified <see cref="ILogger"/> instance added as a tag.</summary>
        /// <param name="logger">The <see cref="ILogger"/> instance to associate with the metadata.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with the logger added as a tag.</returns>
        internal TransportMetadata WithLogger(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            return SetTag(MetadataKeys.Logger, logger);
        }

        /// <summary>Attempts to retrieve a tag value of the specified type from the <see cref="Tags"/> dictionary.</summary>
        /// <typeparam name="T">The expected type of the tag value.</typeparam>
        /// <param name="key">The tag key to look up.</param>
        /// <param name="value">
        /// When this method returns, contains the tag value cast to <typeparamref name="T"/> if found and of the correct type;
        /// otherwise, the default value for <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the tag exists and is of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.
        /// </returns>
        internal bool TryGetTag<T>(string key, [NotNullWhen(true)] out T? value)
        {
            if (Tags.TryGetValue(key, out var objValue) && objValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Retrieves an <see cref="ILogger"/> instance stored in the <see cref="Tags"/> dictionary, if present.
        /// </summary>
        /// <returns>
        /// The <see cref="ILogger"/> instance if found; otherwise, <see langword="null"/>.
        /// </returns>
        internal ILogger? GetLogger() => TryGetTag(MetadataKeys.Logger, out ILogger? logger) ? logger : null;
    }
}
