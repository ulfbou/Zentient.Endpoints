// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultProblemTypeUriGenerator.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides a default implementation of <see cref="IProblemTypeUriGenerator"/>
    /// that constructs a problem type URI based on a configurable base URI and the error code.
    /// </summary>
    public sealed class DefaultProblemTypeUriGenerator : IProblemTypeUriGenerator
    {
        private static readonly Uri DefaultProblemTypeBaseUri = new Uri("about:blank");
        private readonly Uri _baseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProblemTypeUriGenerator"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI for problem types (e.g., "https://yourdomain.com/errors").
        /// Defaults to "about:blank" if not provided.</param>
        public DefaultProblemTypeUriGenerator(Uri? baseUri = null)
        {
            if (baseUri == null || string.IsNullOrWhiteSpace(baseUri.OriginalString))
            {
                this._baseUri = DefaultProblemTypeBaseUri;
            }
            else
            {
                // Ensure baseUri ends with a '/' for consistent path concatenation.
                // Uri constructor handles validation, but we can append a trailing slash if missing.
                // Fixed CA1865: Using char overload for EndsWith
                this._baseUri = baseUri.OriginalString.EndsWith('/')
                    ? baseUri
                    : new Uri($"{baseUri.OriginalString}/");
            }
        }

        /// <summary>
        /// Generates the 'type' URI for a given problem code.
        /// The URI will be constructed as "{BaseUri}/{errorCode.ToUpperInvariant().Replace(' ', '-')}".
        /// If the error code is null or empty, it returns the base URI ("about:blank" or configured base).
        /// </summary>
        /// <param name="errorCode">The specific error code (e.g., "VALIDATION_FAILED", "ITEM_NOT_FOUND").</param>
        /// <returns>A <see cref="Uri"/> representing the full URI for the problem type.</returns>
        public Uri? GenerateProblemTypeUri(string? errorCode)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                return this._baseUri;
            }

            string normalizedErrorCode = errorCode.ToUpperInvariant().Replace(' ', '-');

            if (this._baseUri.Equals(DefaultProblemTypeBaseUri))
            {
                // If the base URI is "about:blank", we don't append specific codes,
                // as RFC 7807 suggests "about:blank" for generic cases, not specific types.
                // For specific types, a proper URI base is expected.
                return this._baseUri;
            }

            return new Uri(this._baseUri, normalizedErrorCode);
        }
    }
}
