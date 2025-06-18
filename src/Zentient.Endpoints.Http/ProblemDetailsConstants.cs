// <copyright file="ProblemDetailsConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides constants related to <see cref="ProblemDetails"/> for consistent key naming
    /// and default values.
    /// </summary>
    public static class ProblemDetailsConstants
    {
        /// <summary>The constant string for "Status" property name in ProblemDetails.</summary>
        public const string Status = "status";

        /// <summary>The constant string for "Title" property name in ProblemDetails.</summary>
        public const string Title = "title";

        /// <summary>The constant string for "Detail" property name in ProblemDetails.</summary>
        public const string Detail = "detail";

        /// <summary>The constant string for "Type" property name in ProblemDetails.</summary>
        public const string Type = "type";

        /// <summary>The constant string for "Instance" property name in ProblemDetails.</summary>
        public const string Instance = "instance";

        /// <summary>The default base URI for problem types as per RFC 7807.</summary>
        public const string DefaultBaseUri = "about:blank";

        /// <summary>The constant string for the "Extensions" key in ProblemDetails.</summary>
        public const string ExtensionsKey = "extensions";

        /// <summary>
        /// Provides constant strings for common extension keys used within <see cref="ProblemDetails.Extensions"/>.
        /// </summary>
        [SuppressMessage("Design", "CA1034:Do not nest type", Justification = "Intentional for logical grouping of ProblemDetails extension constants.")]
        [SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Extensions is a logical grouping for ProblemDetails extension constants.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Documentation is provided at the class level.")]
        public static class Extensions
        {
            /// <summary>The constant string for "statusCode" extension key.</summary>
            public const string StatusCode = "statuscode";

            /// <summary>The constant string for "errorCode" extension key.</summary>
            public const string ErrorCode = "errorcode";

            /// <summary>The constant string for "detail" extension key.</summary>
            public const string Detail = "detail";

            /// <summary>The constant string for "data" extension key.</summary>
            public const string Data = "data";

            /// <summary>The constant string for "innerErrors" extension key.</summary>
            public const string InnerErrors = "innererrors";

            /// <summary>The constant string for "traceId" extension key.</summary>
            public const string TraceId = "traceid";
        }
    }
}
