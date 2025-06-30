// <copyright file="HttpMetadataKeys.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;

using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Http.Constants
{
    /// <summary>
    /// Provides constant keys for HTTP-related metadata tags used in <see cref="Zentient.Endpoints.TransportMetadata"/>.
    /// These keys are used to store and retrieve protocol-specific hints and data
    /// (such as status codes, headers, and location URIs) in the <see cref="Zentient.Endpoints.TransportMetadata.Tags"/> dictionary.
    /// </summary>
    public static class HttpMetadataKeys
    {
        /// <summary>
        /// The key for the HTTP status code hint in <see cref="Zentient.Endpoints.TransportMetadata"/>.
        /// When present, this value should be an <see cref="int"/> representing the HTTP status code
        /// that adapters should use for the response.
        /// </summary>
        public const string HttpStatusCodeHint = "http.status_code_hint";

        /// <summary>
        /// The key for a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> override in <see cref="Zentient.Endpoints.TransportMetadata"/>.
        /// When present, this value should be a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> instance
        /// that will be used directly as the HTTP error response, bypassing default mapping.
        /// </summary>
        public const string ProblemDetailsOverride = "http.problem_details_override";

        /// <summary>
        /// The key for custom HTTP headers in <see cref="Zentient.Endpoints.TransportMetadata"/>.
        /// The value should be an <see cref="ImmutableDictionary{TKey,TValue}"/> of <see cref="string"/> to <see cref="string"/>
        /// representing headers to be included in the HTTP response.
        /// </summary>
        public const string Headers = "http.headers";

        /// <summary>
        /// The key for custom HTTP response headers in <see cref="Zentient.Endpoints.TransportMetadata"/>.
        /// The value should be an <see cref="ImmutableDictionary{TKey,TValue}"/> of <see cref="string"/> to <see cref="string"/>
        /// representing headers to be included in the HTTP response after the main headers.
        /// </summary>
        public const string ResponseHeaders = "http.response_headers";

        /// <summary>
        /// The key for the Location URI in <see cref="Zentient.Endpoints.TransportMetadata"/>.
        /// The value should be a <see cref="System.Uri"/> or a <see cref="string"/> representing
        /// the URI to be set in the HTTP Location header.
        /// </summary>
        public const string LocationUri = "http.location_uri";

        /// <summary>
        /// The key for the <see cref="ILogger"/> instance in
        /// <see cref="Zentient.Endpoints.TransportMetadata"/>. The value should be an
        /// <see cref="ILogger"/> used for logging within the context of the HTTP response.
        /// </summary>
        public const string Logger = "Zentient.Endpoints.Http.Logger";
    }
}
