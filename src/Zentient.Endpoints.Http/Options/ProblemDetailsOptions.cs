// <copyright file="ProblemDetailsOptions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

using Microsoft.AspNetCore.Http;

namespace Zentient.Endpoints.Http.Options
{
    /// <summary>
    /// Provides options for configuring Problem Details (RFC 9457) generation within
    /// Zentient.Endpoints.Http.
    /// </summary>
    public class ProblemDetailsOptions
    {
        /// <summary>
        /// Gets or sets the base URI for constructing the 'type' field in Problem Details.
        /// <para>
        /// Defaults to <c>null</c>. If set, the <see cref="DefaultProblemTypeUriGenerator"/>
        /// will append the error code to this base URI (e.g., "https://example.com/problems/invalid-input").
        /// </para>
        /// <para>If <c>null</c>, the 'type' field will often use "about:blank" or be omitted based on context.</para>
        /// </summary>
        /// <value>
        /// The base URI for Problem Details type URIs, or <see langword="null"/> if not set.
        /// </value>
        public Uri? BaseTypeUri { get; set; }

        /// <summary>
        /// Gets or sets the default title for Problem Details responses.
        /// </summary>
        /// <value>
        /// The default title to use in Problem Details responses, or <see langword="null"/> if not set.
        /// </value>
        public string? DefaultTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include exception stack traces in
        /// Problem Details responses.
        /// <para>Defaults to <c>false</c>. Highly recommended to set to <c>true</c> only
        /// in development environments for debugging purposes.</para>
        /// </summary>
        /// <value>
        /// <c>true</c> to include stack traces in Problem Details responses; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeStackTrace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the internal error code in
        /// the Problem Details 'extensions' field.
        /// <para>Defaults to <c>true</c>.</para>
        /// </summary>
        /// <value>
        /// <c>true</c> to include the error code in the extensions; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeErrorCodeInExtensions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include internal error messages (e.g., from
        /// <see cref="Zentient.Results.ErrorInfo.Message"/>) in the Problem Details 'detail' field.
        /// <para>Defaults to <c>true</c>.</para>
        /// </summary>
        /// <value>
        /// <c>true</c> to include error info messages in the detail field; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeErrorInfoMessagesInDetail { get; set; } = true;

        /// <summary>
        /// Gets a custom mapping for <see cref="Zentient.Results.ErrorInfo.Category"/>
        /// to specific HTTP status codes.
        /// <para>Defaults to a standard mapping (e.g., Unauthorized -> 401, Forbidden -> 403, Validation -> 400).</para>
        /// </summary>
        /// <remarks>
        /// This dictionary allows overriding or extending the default category-to-status code mapping.
        /// Keys are <see cref="Zentient.Results.ErrorInfo.Category"/> strings, values are HTTP status codes.
        /// </remarks>
        /// <value>
        /// A dictionary mapping error category names to HTTP status codes.
        /// </value>
        public Dictionary<string, int> CategoryToStatusCodeMap { get; init; } = new Dictionary<string, int>
        {
            // Populate with sensible defaults, perhaps derived from ResultStatuses
            // Example:
            // { "Validation", 400 },
            // { "Unauthorized", 401 },
            // { "Forbidden", 403 },
            // { "NotFound", 404 },
            // { "Conflict", 409 },
            // { "InternalError", 500 },
        };

        /// <summary>
        /// Creates a deep copy of the current <see cref="ProblemDetailsOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="ProblemDetailsOptions"/> instance with the same settings as the original.</returns>
        public ProblemDetailsOptions Clone()
        {
            return new ProblemDetailsOptions
            {
                BaseTypeUri = this.BaseTypeUri,
                DefaultTitle = this.DefaultTitle,
                IncludeStackTrace = this.IncludeStackTrace,
                IncludeErrorCodeInExtensions = this.IncludeErrorCodeInExtensions,
                IncludeErrorInfoMessagesInDetail = this.IncludeErrorInfoMessagesInDetail,
                CategoryToStatusCodeMap = new Dictionary<string, int>(this.CategoryToStatusCodeMap)
            };
        }
    }
}
