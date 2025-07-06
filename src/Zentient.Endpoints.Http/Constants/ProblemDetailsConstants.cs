// <copyright file="ProblemDetailsConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Endpoints.Http.Constants
{
    /// <summary>
    /// Provides constants related to <see cref="ProblemDetails"/> for consistent key naming
    /// and default values, as per RFC 9457 (supersedes RFC 7807).
    /// </summary>
    public static class ProblemDetailsConstants
    {
        /// <summary>The property name for the HTTP status code in ProblemDetails.</summary>
        public const string Status = "status";

        /// <summary>The property name for the problem title in ProblemDetails.</summary>
        public const string Title = "title";

        /// <summary>The property name for the detailed description in ProblemDetails.</summary>
        public const string Detail = "detail";

        /// <summary>The property name for the problem type URI in ProblemDetails.</summary>
        public const string Type = "type";

        /// <summary>The property name for the instance URI in ProblemDetails.</summary>
        public const string Instance = "instance";

        /// <summary>
        /// The default base URI for problem types as per RFC 9457 ("about:blank" for unclassified problems).
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> representing the default base URI for problem types.
        /// This is typically used when no specific error code is provided,
        /// indicating a generic or unclassified problem.
        /// </value>
        public static readonly Uri DefaultBaseUri = new Uri("about:blank", UriKind.Absolute);

        /// <summary>The property name for the extensions dictionary in ProblemDetails.</summary>
        public const string ExtensionsKey = "extensions";

        /// <summary>
        /// Provides constant strings for common extension keys used within <see cref="ProblemDetails.Extensions"/>.
        /// </summary>
        /// <remarks>
        /// These keys are designed to be used in the <see cref="ProblemDetails.Extensions"/> dictionary
        /// and are typically serialized as camelCase in JSON (e.g., "errorCode", "innerErrors").
        /// </remarks>
        [SuppressMessage("Design", "CA1034:Do not nest type", Justification = "Logical grouping for ProblemDetails extension constants.")]
        [SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Extensions is a logical grouping.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Documentation provided at the class level.")]
        public static class Extensions
        {
            /// <summary>The key for the error code extension (e.g., "INVALID_INPUT", "ITEM_NOT_FOUND").</summary>
            public const string ErrorCode = "errorcode";

            /// <summary>The key for additional arbitrary data or context relevant to the problem.</summary>
            public const string Data = "data";

            /// <summary>The key for nested or inner errors, often an array of ProblemDetails or custom error objects.</summary>
            public const string InnerErrors = "innererrors";

            /// <summary>The key for the trace identifier extension, linking to request diagnostics (e.g., "traceId").</summary>
            public const string TraceId = "traceid";
        }

        /// <summary>
        /// A set of keys that are either standard Problem Details fields (RFC 9457) or well-known extension keys.
        /// Implementations should generally avoid overwriting or using these keys for arbitrary purposes
        /// to maintain consistency and compliance.
        /// </summary>
        public static readonly HashSet<string> ReservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Status,
            Title,
            Detail,
            Type,
            Instance,
            ExtensionsKey, // The key for the extensions dictionary itself

            // Common extension keys that are 'reserved' for specific purposes
            Extensions.ErrorCode,
            Extensions.Data,
            Extensions.InnerErrors,
            Extensions.TraceId
        };
    }
}
