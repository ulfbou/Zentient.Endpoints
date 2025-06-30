// <copyright file="SuccessResponse{TData}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Models
{
    /// <summary>
    /// Represents a standardized success response structure for API endpoints, encapsulating the
    /// data, messages, and status. This class is designed to be serialized into a consistent JSON format.
    /// </summary>
    /// <typeparam name="TData">The type of the primary data being returned.</typeparam>
    public sealed class SuccessResponse<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessResponse{TData}"/> class.
        /// </summary>
        /// <param name="data">The primary data to include in the response.</param>
        /// <param name="message">
        /// An optional human-readable single message describing the overall status.
        /// This field's presence is typically controlled by
        /// <see cref="SuccessResponseOptions.IncludeSingleMessageField" />.
        /// </param>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="statusDescription">
        /// The human-readable description of the HTTP status code. If null, a default description
        /// will be derived from the status code using <see cref="ResultStatuses"/>.
        /// </param>
        /// <param name="messages">Optional additional messages associated with the result.</param>
        public SuccessResponse(
            TData? data,
            string? message,
            int statusCode,
            string? statusDescription,
            IReadOnlyList<string>? messages = null)
        {
            this.Data = data;
            this.Message = message;
            this.StatusCode = statusCode;
            this.StatusDescription = statusDescription
                ?? ResultStatuses.GetStatus(statusCode).Description;
            this.Messages = messages ?? Array.Empty<string>();
        }

        /// <summary>Gets the primary data returned by the operation.</summary>
        /// <value>
        /// An instance of <typeparamref name="TData"/> containing the result of the operation,
        /// or null if no data is available.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Data)]
        public TData? Data { get; init; }

        /// <summary>
        /// Gets a human-readable message describing the overall status of the operation.
        /// This field's presence is controlled by the factory based on configuration.
        /// </summary>
        /// <value>
        /// A string containing a message that provides additional context about the operation's
        /// success, or null if no message is provided, based on the
        /// <see cref="SuccessResponseOptions"/> property IncludeSingleMessageField configuration.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Message)]
        public string? Message { get; init; }

        /// <summary>
        /// Gets a list of additional messages (e.g., warnings or informational notes) associated
        /// with the result.
        /// </summary>
        /// <value>
        /// A read-only list of strings containing any additional messages related to the operation,
        /// or an empty list if no additional messages are available.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.Messages)]
        public IReadOnlyList<string> Messages { get; init; }

        /// <summary>Gets the HTTP status code (e.g., 200, 201).</summary>
        /// <value>
        /// An integer representing the HTTP status code for the response, indicating the outcome of
        /// the operation.
        /// </value>
        [JsonPropertyName(ResponseJsonProperties.StatusCode)]
        public int StatusCode { get; init; }

        /// <summary>Gets a human-readable description of the HTTP status code.</summary>
        /// <value>The description corresponding to the HTTP status code, or null if not set.</value>
        [JsonPropertyName(ResponseJsonProperties.StatusDescription)]
        public string StatusDescription { get; init; }
    }
}
