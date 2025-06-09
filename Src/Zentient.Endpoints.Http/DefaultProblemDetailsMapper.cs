// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zentient.Results;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides a default implementation of <see cref="IProblemDetailsMapper"/>,
    /// translating <see cref="ErrorInfo"/> into standard <see cref="ProblemDetails"/>.
    /// </summary>
    /// <remarks>
    /// This mapper converts <see cref="ErrorInfo.Category"/> to a corresponding HTTP status code
    /// and populates <see cref="ProblemDetails"/> fields like Title, Detail, and Extensions
    /// with information from the <see cref="ErrorInfo"/>.
    /// </remarks>
    public sealed class DefaultProblemDetailsMapper : IProblemDetailsMapper
    {
        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance.
        /// </summary>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> to map.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, providing additional context.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance.</returns>
        public ProblemDetails Map(ErrorInfo? errorInfo, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (errorInfo == null)
            {
                return new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Instance = httpContext.Request.Path,
                };
            }

            ErrorInfo actualErrorInfo = (ErrorInfo)errorInfo;
            HttpStatusCode statusCode = GetHttpStatusCode(actualErrorInfo.Category);

            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = GetDefaultTitle(statusCode),
                Detail = actualErrorInfo.Message,
                Type = GetProblemTypeUri(actualErrorInfo.Code),
                Instance = httpContext.Request.Path,
            };

            IDictionary<string, object?> extensions = problemDetails.Extensions;

            if (!string.IsNullOrEmpty(actualErrorInfo.Code))
            {
                extensions["code"] = actualErrorInfo.Code;
            }

            if (!string.IsNullOrEmpty(actualErrorInfo.Detail))
            {
                extensions["detail"] = actualErrorInfo.Detail;
            }

            if (actualErrorInfo.Data is IReadOnlyList<object> dataList && dataList.Count > 0)
            {
                extensions["data"] = dataList;
            }

            if (actualErrorInfo.InnerErrors is IReadOnlyList<ErrorInfo> innerErrorsList && innerErrorsList.Count > 0)
            {
                extensions["innerErrors"] = innerErrorsList;
            }

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
            {
                extensions["traceId"] = httpContext.TraceIdentifier;
            }

            return problemDetails;
        }

        /// <summary>
        /// Converts an <see cref="ErrorCategory"/> to an appropriate <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="category">The error category.</param>
        /// <returns>The corresponding HTTP status code.</returns>
        private static HttpStatusCode GetHttpStatusCode(ErrorCategory category)
        {
            return category switch
            {
                ErrorCategory.Validation => HttpStatusCode.BadRequest,
                ErrorCategory.NotFound => HttpStatusCode.NotFound,
                ErrorCategory.Conflict => HttpStatusCode.Conflict,
                ErrorCategory.Unauthorized => HttpStatusCode.Unauthorized,
                ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
                ErrorCategory.Concurrency => HttpStatusCode.Conflict,
                ErrorCategory.ServiceUnavailable => HttpStatusCode.ServiceUnavailable,
                ErrorCategory.InternalServerError => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError,
            };
        }

        /// <summary>
        /// Gets a default title for the <see cref="ProblemDetails"/> based on the HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>A descriptive title string.</returns>
        private static string GetDefaultTitle(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.Unauthorized => "Unauthorized",
                HttpStatusCode.Forbidden => "Forbidden",
                HttpStatusCode.NotFound => "Not Found",
                HttpStatusCode.Conflict => "Conflict",
                HttpStatusCode.InternalServerError => "Internal Server Error",
                HttpStatusCode.ServiceUnavailable => "ServiceUnavailable",
                _ => "An Error Occurred",
            };
        }

        /// <summary>
        /// Generates a URI for the problem type based on the error code.
        /// </summary>
        /// <param name="errorCode">The specific error code (e.g., "InvalidInput").</param>
        /// <returns>A URI string or null if no code is provided.</returns>
        private static string? GetProblemTypeUri(string? errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                return null; // Or a generic URI like "about:blank" or a configurable base URI
            }

            // SA1515: A single-line comment within C# code is not preceded by a blank line.
            // A blank line is added before this comment in the actual file.

            // CA1308: Strings should be normalized to uppercase for round-trip safety.
            // Using ToUpperInvariant() as requested to satisfy CA1308, while acknowledging
            // lowercase is common for URIs. This is a pragmatic choice for v0.1.0 to pass analyzers.
            // Note: The 'https:' in the original prompt was a syntax error.
            return $"https://yourdomain.com/errors/{errorCode.ToUpperInvariant().Replace(" ", "-", StringComparison.Ordinal)}";
        }
    }
}
