// <copyright file="SuccessResponse{TData}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Represents a standardized success response structure for API endpoints, encapsulating the data, messages, and status.
    /// </summary>
    /// <typeparam name="TData">The type of the primary data being returned.</typeparam>
    internal sealed class SuccessResponse<TData>
    {
        /// <summary>Initializes a new instance of the <see cref="SuccessResponse{TData}"/> class.</summary>
        /// <param name="data">The primary data to include in the response.</param>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="statusDescription">The human-readable description of the HTTP status code.</param>
        /// <param name="messages">Optional additional messages associated with the result.</param>
        public SuccessResponse(
            TData? data,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string>? messages = null)
        {
            this.Data = data;
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription;
            this.Messages = messages ?? Array.Empty<string>();
            this.Message = this.Messages.Any() ? this.Messages[0] : null;
        }

        /// <summary>Gets the primary data returned by the operation.</summary>
        /// <value>An instance of <typeparamref name="TData"/> containing the result of the operation, or null if no data is available.</value>
        [JsonPropertyName("data")]
        public TData? Data { get; }

        /// <summary>Gets a human-readable message describing the overall status of the operation.</summary>
        /// <value>A string containing a message that provides additional context about the operation's success, or null if no message is provided.</value>
        [JsonPropertyName("message")]
        public string? Message { get; }

        /// <summary>Gets a list of additional messages (e.g., warnings or informational notes) associated with the result.</summary>
        /// <value>A read-only list of strings containing any additional messages related to the operation, or an empty list if no additional messages are available.</value>
        [JsonPropertyName("messages")]
        public IReadOnlyList<string> Messages { get; }

        /// <summary>Gets the HTTP status code (e.g., 200, 201).</summary>
        /// <value>An integer representing the HTTP status code for the response, indicating the outcome of the operation.</value>
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; }

        /// <summary>Gets the human-readable description of the HTTP status code (e.g., "OK", "Created").</summary>
        /// <value>A string providing a human-readable description of the HTTP status code, giving context to the status code returned.</value>
        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; }
    }
}
