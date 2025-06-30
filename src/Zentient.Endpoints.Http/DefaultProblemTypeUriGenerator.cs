// <copyright file="DefaultProblemTypeUriGenerator.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Globalization;

using Microsoft.AspNetCore.Http;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides a default implementation of <see cref="IProblemTypeUriGenerator"/>
    /// that constructs a problem type URI based on a configurable base URI and the error code.
    /// </summary>
    internal sealed class DefaultProblemTypeUriGenerator : IProblemTypeUriGenerator
    {
        private static readonly Uri DefaultProblemTypeBaseUri = new Uri(ProblemDetailsConstants.DefaultBaseUri);
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
        /// <param name="httpContext">The current HTTP context, which may be used to access request-specific information.</param>
        /// <returns>A <see cref="Uri"/> representing the full URI for the problem type.</returns>
        public ValueTask<string> Generate(string? errorCode, HttpContext httpContext)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                return ValueTask.FromResult(this._baseUri.ToString());
            }

            string normalizedErrorCode = errorCode.ToUpperInvariant().Replace(' ', '-');

            // If the base URI is the default, but the HttpContext has a host, use it to build a more specific URI
            if (!this._baseUri.Equals(DefaultProblemTypeBaseUri) || httpContext.Request?.Host.HasValue != true)
            {
                var scheme = httpContext.Request!.Scheme
                    ?? "http";
                var host = httpContext.Request.Host.Value
                    ?? "localhost";
                var pathBase = httpContext.Request.PathBase.HasValue
                    ? httpContext.Request.PathBase.Value.TrimEnd('/')
                    : string.Empty;
                var uri = $"{scheme}://{host}{pathBase}/errors/{normalizedErrorCode}";
                return ValueTask.FromResult(uri);
            }

            return ValueTask.FromResult(new Uri(this._baseUri, normalizedErrorCode).ToString());
        }
    }
}
