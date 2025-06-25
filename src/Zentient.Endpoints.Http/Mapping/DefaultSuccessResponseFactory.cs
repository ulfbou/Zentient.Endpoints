// <copyright file="DefaultSuccessResponseFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Options;

using Zentient.Endpoints;
using Zentient.Endpoints.Http.Models;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;
namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Default implementation of <see cref="ISuccessResponseFactory"/>, responsible for
    /// creating standardized success response payloads (<see cref="SuccessResponse{TData}"/>)
    /// from <see cref="IEndpointOutcome"/> instances.
    /// </summary>
    /// <remarks>
    /// This factory uses configured <see cref="SuccessResponseOptions"/> to control aspects
    /// like whether a single message is included in a dedicated 'message' field or always
    /// within the 'messages' array.
    /// </remarks>
    internal sealed class DefaultSuccessResponseFactory : ISuccessResponseFactory
    {
        private readonly SuccessResponseOptions _successResponseOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSuccessResponseFactory"/> class.
        /// </summary>
        /// <param name="options">
        /// The <see cref="IOptions{ZentientEndpointsHttpOptions}"/> containing configuration
        /// for success responses.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        public DefaultSuccessResponseFactory(IOptions<EndpointsHttpOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            if (options.Value.SuccessResponse is null)
            {
                throw new ArgumentException(
                    "SuccessResponse options must be configured.",
                    nameof(options)
                );
            }

            _successResponseOptions = options.Value.SuccessResponse;
        }

        /// <summary>
        /// Creates a standardized success response payload from a generic endpoint outcome
        /// that contains a value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value contained in the outcome.</typeparam>
        /// <param name="outcome">The generic endpoint outcome (e.g., <see cref="IEndpointOutcome{TValue}"/>).</param>
        /// <param name="statusCode">The HTTP status code to include in the response.</param>
        /// <param name="statusDescription">The human-readable description of the HTTP status code.</param>
        /// <param name="messages">A read-only list of messages associated with the outcome.</param>
        /// <param name="value">The actual data value from the outcome.</param>
        /// <returns>A new <see cref="SuccessResponse{TData}"/> instance.</returns>
        public object CreateSuccessResponse<TValue>(
            IEndpointOutcome<TValue> outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages,
            TValue? value)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentOutOfRangeException.ThrowIfNegative(statusCode, nameof(statusCode));
            ArgumentNullException.ThrowIfNull(statusDescription, nameof(statusDescription));
            ArgumentNullException.ThrowIfNull(messages, nameof(messages));

            var singleMessage = messages.Count == 1 && _successResponseOptions.IncludeSingleMessageField
                                ? messages[0]
                                : null;

            var messagesArray = messages.Any() ? messages : null;

            return new SuccessResponse<TValue>(
                data: value,
                message: singleMessage,
                messages: messagesArray,
                statusCode: statusCode,
                statusDescription: statusDescription
            );
        }

        /// <summary>
        /// Creates a standardized success response payload from a non-generic endpoint outcome
        /// or a generic outcome without a meaningful data value (e.g., <see cref="Unit"/>).
        /// </summary>
        /// <param name="outcome">The non-generic endpoint outcome (e.g., <see cref="IEndpointOutcome"/>).</param>
        /// <param name="statusCode">The HTTP status code to include in the response.</param>
        /// <param name="statusDescription">The human-readable description of the HTTP status code.</param>
        /// <param name="messages">A read-only list of messages associated with the outcome.</param>
        /// <returns>A new <see cref="SuccessResponse{TData}"/> instance (where TData is <see cref="object"/>).</returns>
        public object CreateSuccessResponse(
            IEndpointOutcome outcome,
            int statusCode,
            string statusDescription,
            IReadOnlyList<string> messages)
        {
            var singleMessage = messages.Count == 1 && _successResponseOptions.IncludeSingleMessageField
                                ? messages[0]
                                : null;

            var messagesArray = messages.Any() ? messages : null;

            return new SuccessResponse<Unit>(
                data: Unit.Value,
                message: singleMessage,
                messages: messagesArray,
                statusCode: statusCode,
                statusDescription: statusDescription
            );
        }
    }
}
