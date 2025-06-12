// <copyright file="ProblemDetailsConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Constants for standard Problem Details fields and extensions. These constants are used to
    /// ensure consistent naming across different implementations of Problem Details in the Zentient framework.
    /// </summary>
    public static class ProblemDetailsConstants
    {
        /// <summary>The status field name.</summary>
        public const string Status = "Status";

        /// <summary>The title field name.</summary>
        public const string Title = "Title";

        /// <summary>The detail field name.</summary>
        public const string Detail = "Detail";

        /// <summary>The type field name.</summary>
        public const string Type = "Type";

        /// <summary>The instance field name.</summary>
        public const string Instance = "Instance";

        /// <summary>The base URI for Problem Details.</summary>
        public const string DefaultBaseUri = "about:blank";

        /// <summary>
        /// Contains constants for ProblemDetails extension fields. This class is used for logical grouping
        /// of extension constants related to ProblemDetails in the Zentient framework.
        /// </summary>
        [SuppressMessage("Design", "CA1034:Do not nest type", Justification = "Intentional for logical grouping of ProblemDetails extension constants.")]
        [SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Extensions is a logical grouping for ProblemDetails extension constants.")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Documentation is provided at the class level.")]
        public static class Extensions
        {
            /// <summary>The status code extension field name.</summary>
            public const string StatusCode = nameof(StatusCode);

            /// <summary>The error code extension field name.</summary>
            public const string ErrorCode = nameof(ErrorCode);

            /// <summary>The detail extension field name.</summary>
            public const string Detail = nameof(Detail);

            /// <summary>The data extension field name.</summary>
            public const string Data = nameof(Data);

            /// <summary>The inner errors extension field name.</summary>
            public const string InnerErrors = nameof(InnerErrors);

            /// <summary>The trace identifier extension field name.</summary>
            public const string TraceId = nameof(TraceId);
        }
    }
}
