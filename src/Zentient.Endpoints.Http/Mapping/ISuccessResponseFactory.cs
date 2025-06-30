// <copyright file="ISuccessResponseFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Generic;

using Zentient.Endpoints.Http.Models;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Defines a factory for creating success response payloads,
    /// allowing customization of the serialized success body.
    /// </summary>
    public interface ISuccessResponseFactory
    {
        /// <summary>
        /// Creates the final object to be serialized as the success response body for a generic
        /// outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The original generic endpoint outcome.</param>
        /// <param name="statusCode">The HTTP status code determined for the response.</param>
        /// <param name="statusDescription">The description corresponding to the status code.</param>
        /// <param name="messages">Any messages associated with the outcome.</param>
        /// <param name="data">The data payload from the outcome.</param>
        /// <returns>An object representing the success response body.</returns>
        object CreateSuccessResponse<TValue>(
            IEndpointOutcome<TValue> outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages,
            TValue? data);

        /// <summary>
        /// Creates the final object to be serialized as the success response body for a non-generic
        /// outcome (e.g., IEndpointOutcome&lt;Unit&gt;).
        /// </summary>
        /// <param name="outcome">The original non-generic endpoint outcome.</param>
        /// <param name="statusCode">The HTTP status code determined for the response.</param>
        /// <param name="statusDescription">The description corresponding to the status code.</param>
        /// <param name="messages">Any messages associated with the outcome.</param>
        /// <returns>An object representing the success response body.</returns>
        object CreateSuccessResponse(
            IEndpointOutcome outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages);
    }
}
