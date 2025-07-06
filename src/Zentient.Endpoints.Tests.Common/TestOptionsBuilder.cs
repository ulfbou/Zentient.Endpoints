// <copyright file="TestOptionsBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Zentient.Endpoints.Http.Options;
using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A factory for creating configured instances of <see cref="IOptions{EndpointsHttpOptions}"/>
    /// for testing purposes, offering various common configurations.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public static class TestOptionsFactory
    {
        /// <summary>
        /// Creates <see cref="IOptions{EndpointsHttpOptions}"/> with default settings.
        /// </summary>
        /// <returns>An <see cref="IOptions{EndpointsHttpOptions}"/> instance.</returns>
        public static IOptions<EndpointsHttpOptions> CreateDefault()
        {
            var options = new EndpointsHttpOptions();
            return Options.Create(options);
        }

        /// <summary>
        /// Creates <see cref="IOptions{EndpointsHttpOptions}"/> with custom configurations.
        /// </summary>
        /// <param name="configure">An action to configure the <see cref="EndpointsHttpOptions"/> instance.</param>
        /// <returns>An <see cref="IOptions{EndpointsHttpOptions}"/> instance.</returns>
        public static IOptions<EndpointsHttpOptions> CreateCustom(Action<EndpointsHttpOptions> configure)
        {
            var options = new EndpointsHttpOptions();
            configure?.Invoke(options);
            return Options.Create(options);
        }

        /// <summary>
        /// Creates <see cref="IOptions{EndpointsHttpOptions}"/> configured for Problem Details testing.
        /// </summary>
        /// <param name="baseTypeUri">The base URI for problem types. Defaults to null.</param>
        /// <param name="includeStackTrace">Whether to include stack traces. Defaults to false.</param>
        /// <param name="includeErrorCodeInExtensions">Whether to include error codes in extensions. Defaults to true.</param>
        /// <param name="includeErrorInfoMessagesInDetail">Whether to include error info messages in detail. Defaults to true.</param>
        /// <param name="categoryToStatusCodeMap">Custom category to status code mappings.</param>
        /// <returns>An <see cref="IOptions{EndpointsHttpOptions}"/> instance.</returns>
        public static IOptions<EndpointsHttpOptions> CreateProblemDetailsOptions(
            Uri? baseTypeUri = null,
            bool includeStackTrace = false,
            bool includeErrorCodeInExtensions = true,
            bool includeErrorInfoMessagesInDetail = true,
            Dictionary<string, int>? categoryToStatusCodeMap = null)
        {
            return CreateCustom(opts =>
            {
                if (baseTypeUri != null)
                {
                    opts.ProblemDetails.BaseTypeUri = baseTypeUri;
                }
                opts.ProblemDetails.IncludeStackTrace = includeStackTrace;
                opts.ProblemDetails.IncludeErrorCodeInExtensions = includeErrorCodeInExtensions;
                opts.ProblemDetails.IncludeErrorInfoMessagesInDetail = includeErrorInfoMessagesInDetail;

                // Workaround for init-only property: clear and add entries instead of assignment
                var map = opts.ProblemDetails.CategoryToStatusCodeMap;
                map.Clear();
                if (categoryToStatusCodeMap != null)
                {
                    foreach (var kvp in categoryToStatusCodeMap)
                    {
                        map[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Provide some default mappings if none are provided
                    map[ErrorCategory.Validation.ToString()] = StatusCodes.Status400BadRequest;
                    map[ErrorCategory.NotFound.ToString()] = StatusCodes.Status404NotFound;
                    map[ErrorCategory.Authentication.ToString()] = StatusCodes.Status401Unauthorized;
                    map[ErrorCategory.Authorization.ToString()] = StatusCodes.Status403Forbidden;
                    map[ErrorCategory.Conflict.ToString()] = StatusCodes.Status409Conflict;
                    map[ErrorCategory.InternalServerError.ToString()] = StatusCodes.Status500InternalServerError;
                }
            });
        }

        /// <summary>
        /// Creates <see cref="IOptions{EndpointsHttpOptions}"/> configured for Success Response testing.
        /// </summary>
        /// <param name="defaultOkStatusCode">Default 2xx status code. Defaults to 200.</param>
        /// <param name="defaultNoContentStatusCode">Default no content status code. Defaults to 204.</param>
        /// <param name="includeSingleMessageField">Whether to include a single message field. Defaults to true.</param>
        /// <returns>An <see cref="IOptions{EndpointsHttpOptions}"/> instance.</returns>
        public static IOptions<EndpointsHttpOptions> CreateSuccessResponseOptions(
            int defaultOkStatusCode = StatusCodes.Status200OK,
            int defaultNoContentStatusCode = StatusCodes.Status204NoContent,
            bool includeSingleMessageField = true)
        {
            return CreateCustom(opts =>
            {
                opts.SuccessResponse.DefaultOkStatusCode = defaultOkStatusCode;
                opts.SuccessResponse.DefaultNoContentStatusCode = defaultNoContentStatusCode;
                opts.SuccessResponse.IncludeSingleMessageField = includeSingleMessageField;
            });
        }

        /// <summary>
        /// Creates <see cref="IOptions{EndpointsHttpOptions}"/> with custom JSON serialization options.
        /// </summary>
        /// <param name="configureJson">An action to configure the <see cref="JsonSerializerOptions"/>.</param>
        /// <returns>An <see cref="IOptions{EndpointsHttpOptions}"/> instance.</returns>
        public static IOptions<EndpointsHttpOptions> CreateWithCustomJsonSerializer(Action<JsonSerializerOptions> configureJson)
        {
            return CreateCustom(opts =>
            {
                configureJson?.Invoke(opts.JsonSerializerOptions);
            });
        }
    }
}
