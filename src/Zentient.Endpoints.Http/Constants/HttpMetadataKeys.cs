// <copyright file="HttpMetadataKeys.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;

using Microsoft.Extensions.Logging;

namespace Zentient.Endpoints.Http.Constants
{
    /// <summary>
    /// Provides constant keys for HTTP-related metadata tags used in transport metadata.
    /// </summary>
    public static class HttpMetadataKeys
    {
        /// <summary>The key for the HTTP status code hint in transport metadata.</summary>
        public const string HttpStatusCodeHint = "http.status_code_hint";

        /// <summary>The key for the ProblemDetails override in transport metadata.</summary>
        public const string ProblemDetailsOverride = "http.problem_details_override";

        /// <summary>The key for custom HTTP headers in transport metadata.</summary>
        public const string Headers = "http.headers";

        /// <summary>The key for the Location URI in transport metadata.</summary>
        public const string LocationUri = "http.location_uri";

        /// <summary>The key for the <see cref="ILogger"/> instance in transport metadata.</summary>
        public const string Logger = "Zentient.Endpoints.Http.Logger";
    }
}
