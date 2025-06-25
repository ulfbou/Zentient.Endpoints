// <copyright file="DefaultSuccessResponseFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Zentient.Endpoints.Http.Models;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Provides a default implementation of <see cref="ISuccessResponseFactory"/>
    /// creating <see cref="SuccessResponse{TData}"/> objects.
    /// </summary>
    internal class DefaultSuccessResponseFactory : ISuccessResponseFactory
    {
        /// <inheritdoc />
        public object CreateSuccessResponse<TValue>(
            IEndpointOutcome<TValue> outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages,
            TValue? data) where TValue : notnull
        {
            // Prefer messages provided as a parameter if not null or empty,
            // otherwise use outcome's messages.
            var effectiveMessages = messages?.Any() == true ? messages : outcome.Messages;

            // Prefer statusDescription provided as a parameter if not null or empty.
            // Otherwise, use outcome's status description. If that's also null,
            // derive from statusCode.
            var effectiveStatusDescription = string.IsNullOrEmpty(statusDescription)
                ? (outcome.Status?.Description ?? ResultStatuses.GetStatus(statusCode).Description)
                : statusDescription;

            // Determine the final data to be included in the response payload.
            // The 'data' parameter (from the mapper) takes highest precedence.
            // If it's null or default(TValue), then fall back to the value from the outcome.
            TValue? finalData = data;
            if (finalData is null ||
                EqualityComparer<TValue>.Default.Equals(finalData, default(TValue)))
            {
                finalData = outcome.Value;
            }

            return new SuccessResponse<TValue>(
                finalData,
                statusCode,
                effectiveStatusDescription,
                effectiveMessages
            );
        }

        /// <inheritdoc />
        public object CreateSuccessResponse(
            IEndpointOutcome outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages)
        {
            // Prefer messages provided as a parameter if not null or empty,
            // otherwise use outcome's messages.
            var effectiveMessages = messages?.Any() == true ? messages : outcome.Messages;

            // Prefer statusDescription provided as a parameter if not null or empty.
            // Otherwise, use outcome's status description. If that's also null,
            // derive from statusCode.
            var effectiveStatusDescription = string.IsNullOrEmpty(statusDescription)
                ? (outcome.Status?.Description ?? ResultStatuses.GetStatus(statusCode).Description)
                : statusDescription;

            // For non-generic outcomes (IEndpointOutcome without a specific TValue)
            // and for outcomes explicitly representing 'Unit' (no content),
            // the data payload should be null.
            object? dataPayload = null;

            // Check if the underlying concrete outcome class holds a Unit value.
            // Access GetValueAsObject() via the concrete base class EndpointOutcome.
            if (outcome is EndpointOutcome concreteOutcome &&
                concreteOutcome.GetValueAsObject() is Unit)
            {
                dataPayload = null;
            }

            return new SuccessResponse<object?>(
                dataPayload,
                statusCode,
                effectiveStatusDescription,
                effectiveMessages
            );
        }
    }
}
