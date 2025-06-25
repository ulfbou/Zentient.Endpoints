// <copyright file="ResponseConstants.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Zentient.Endpoints.Http.Constants
{
    /// <summary>
    /// Provides constant strings for JSON property names in standardized API success responses.
    /// These are typically used with <see cref="JsonPropertyNameAttribute"/>.
    /// </summary>
    internal static class ResponseJsonProperties
    {
        /// <summary>The JSON property name for the primary data payload in a response.</summary>
        internal const string Data = "data";

        /// <summary>The JSON property name for a single human-readable message in a response.</summary>
        internal const string Message = "message";

        /// <summary>The JSON property name for a list of human-readable messages in a response.</summary>
        internal const string Messages = "messages";

        /// <summary>The JSON property name for the HTTP status code in a response (e.g., 200, 201).</summary>
        internal const string StatusCode = "statusCode";

        /// <summary>The JSON property name for the human-readable description of the HTTP status code.</summary>
        internal const string StatusDescription = "statusDescription";
    }
}
