// <copyright file="SuccessResponse{TData}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

using Zentient.Endpoints.Http.Constants;
using Zentient.Results;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Zentient.Endpoints.Http.Models
{
    /// <summary>
    /// Represents a standardized success response structure for API endpoints, encapsulating the
    /// data, messages, and status. This class is designed to be serialized into a consistent JSON format.
    /// </summary>
    /// <typeparam name="TData">The type of the primary data being returned.</typeparam>
    internal sealed class SuccessResponse<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessResponse{TData}"/> class.
        /// </summary>
        /// <param name="data">The primary data to include in the response.</param>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="statusDescription">
        /// The human-readable description of the HTTP status code. If null, a default description
        /// will be derived from the status code using <see cref="ResultStatuses"/>.
        /// </param>
        /// <param name="messages">Optional additional messages associated with the result.</param>
        public SuccessResponse(
            TData? data,
            int statusCode,
            string? statusDescription, // Made nullable to match parameter behavior
            IReadOnlyList<string>? messages = null)
        {
            Data = data;
            StatusCode = statusCode;
            // Ensure StatusDescription is never null by falling back to ResultStatuses
            StatusDescription = statusDescription ?? ResultStatuses.GetStatus(statusCode).Description;
            Messages = messages ?? Array.Empty<string>();
            Message = Messages.Any() ? Messages[0] : null;
        }

        /// <summary>Gets the primary data returned by the operation.</summary>
        /// <value>
        /// An instance of <typeparamref name="TData"/> containing the result of the operation,
        /// or null if no data is available.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Data)]
        public TData? Data { get; init; } // Changed to init-only for immutability after construction

        /// <summary>
        /// Gets a human-readable message describing the overall status of the operation.
        /// This typically contains the first message from the <see cref="Messages"/> list, if available.
        /// </summary>
        /// <value>
        /// A string containing a message that provides additional context about the operation's
        /// success, or null if no message is provided.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Message)]
        public string? Message { get; }

        /// <summary>
        /// Gets a list of additional messages (e.g., warnings or informational notes) associated
        /// with the result.
        /// </summary>
        /// <value>
        /// A read-only list of strings containing any additional messages related to the operation,
        /// or an empty list if no additional messages are available.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Messages)]
        public IReadOnlyList<string> Messages { get; }

        /// <summary>Gets the HTTP status code (e.g., 200, 201).</summary>
        /// <value>
        /// An integer representing the HTTP status code for the response, indicating the outcome of
        /// the operation.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.StatusCode)]
        public int StatusCode { get; }

        /// <summary>Gets a human-readable description of the HTTP status code.</summary>
        /// <value>The description corresponding to the HTTP status code, or null if not set.</value>
        [JsonPropertyName(ResponseJsonProperties.StatusDescription)]
        public string StatusDescription { get; } // Changed to get-only as it's always set in constructor
    }
}
