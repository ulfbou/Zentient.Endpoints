// <copyright file="DefaultProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zentient.Results;
using Zentient.Results.Constants;

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
    internal sealed class DefaultProblemDetailsMapper : IProblemDetailsMapper
    {
        private readonly IProblemTypeUriGenerator _problemTypeUriGenerator;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProblemDetailsMapper"/> class.
        /// </summary>
        /// <param name="problemTypeUriGenerator">The problem type URI generator to use. If <c>null</c>, a default <see cref="DefaultProblemTypeUriGenerator"/> is used.</param>
        public DefaultProblemDetailsMapper(IProblemTypeUriGenerator? problemTypeUriGenerator = null)
        {
            this._problemTypeUriGenerator = problemTypeUriGenerator ?? new DefaultProblemTypeUriGenerator();
            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // <-- THIS IS THE CULPRIT
                WriteIndented = false,
            };
        }

        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance asynchronously.
        /// </summary>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> to map. If <c>null</c>, a generic internal server error ProblemDetails will be returned.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, providing additional context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="ProblemDetails"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpContext"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="errorInfo"/> has <see cref="ErrorCategory.None"/>, indicating an issue in upstream result handling.</exception>
        public Task<ProblemDetails> Map(ErrorInfo? errorInfo, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (errorInfo == null)
            {
                var defaultExtensions = new Dictionary<string, object?>
                {
                    [ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier,
                };

                return Task.FromResult(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Internal Server Error",
                        Detail = "An unexpected error occurred and no specific error information was provided.",
                        Instance = httpContext.Request.Path,
                        Type = this._problemTypeUriGenerator?.GenerateProblemTypeUri(ErrorCodes.InternalServerError)?.ToString(),
                        Extensions = defaultExtensions,
                    });
            }

            if (errorInfo.Category == ErrorCategory.None)
            {
                throw new InvalidOperationException(
                    $"Cannot map ErrorCategory.None to ProblemDetails. " +
                    $"The '{nameof(DefaultProblemDetailsMapper)}' expects an actual error category. " +
                    $"This indicates an issue in the upstream result handling where a non-error was passed for problem mapping. " +
                    $"ErrorInfo Code: {errorInfo.Code ?? "N/A"}, Message: {errorInfo.Message ?? "N/A"}");
            }

            int statusCode = GetHttpStatusCode(errorInfo.Category);
            string? problemTypeUriString = this._problemTypeUriGenerator?.GenerateProblemTypeUri(errorInfo.Code)?.ToString();
            var extensions = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
            {
                extensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            if (!string.IsNullOrEmpty(errorInfo.Code))
            {
                extensions[ProblemDetailsConstants.Extensions.ErrorCode] = errorInfo.Code;
            }

            if (!string.IsNullOrEmpty(errorInfo.Detail))
            {
                // TODO: Consider renaming ProblemDetailsConstants.Extensions.Detail to something more specific
                extensions[ProblemDetailsConstants.Extensions.Detail] = errorInfo.Detail;
            }

            foreach (var kvp in errorInfo.Metadata)
            {
                if (!extensions.ContainsKey(kvp.Key))
                {
                    extensions[kvp.Key] = kvp.Value;
                }
            }

            if (errorInfo.InnerErrors.Any())
            {
                var mappedInnerErrors = errorInfo.InnerErrors
                    .Select(inner => new Dictionary<string, object?>
                    {
                        // TODO: Use JsonConstants when 0.4.1 is released
                        { "category", inner.Category.ToString().ToUpperInvariant() },
                        { "code", inner.Code },
                        { "message", inner.Message },
                        { "detail", inner.Detail },
                    })
                    .ToList();
                extensions[ProblemDetailsConstants.Extensions.InnerErrors] = mappedInnerErrors;
            }

            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ResultStatuses.GetStatus(statusCode, "An Error Occurred").Description,
                Detail = errorInfo.Message,
                Type = problemTypeUriString,
                Instance = httpContext.Request.Path,
                Extensions = extensions,
            };

            return Task.FromResult(problemDetails);
        }

        /// <summary>
        /// Converts an <see cref="ErrorCategory"/> to an appropriate HTTP status code.
        /// </summary>
        /// <param name="category">The error category from <see cref="Zentient.Results.ErrorInfo"/>.</param>
        /// <returns>The corresponding HTTP status code.</returns>
        private static int GetHttpStatusCode(ErrorCategory category) => category switch
        {
            ErrorCategory.Validation => ResultStatuses.BadRequest.Code,
            ErrorCategory.Request => ResultStatuses.BadRequest.Code,
            ErrorCategory.BusinessLogic => ResultStatuses.BadRequest.Code,
            ErrorCategory.Authentication => ResultStatuses.Unauthorized.Code,
            ErrorCategory.Authorization => ResultStatuses.Forbidden.Code,
            ErrorCategory.NotFound => ResultStatuses.NotFound.Code,
            ErrorCategory.ResourceGone => ResultStatuses.Gone.Code,
            ErrorCategory.Conflict => ResultStatuses.Conflict.Code,
            ErrorCategory.Concurrency => ResultStatuses.Conflict.Code,
            ErrorCategory.TooManyRequests => ResultStatuses.TooManyRequests.Code,
            ErrorCategory.RateLimit => ResultStatuses.TooManyRequests.Code,
            ErrorCategory.Timeout => ResultStatuses.RequestTimeout.Code,
            ErrorCategory.Security => ResultStatuses.Forbidden.Code,
            ErrorCategory.NotImplemented => ResultStatuses.NotImplemented.Code,
            ErrorCategory.ServiceUnavailable => ResultStatuses.ServiceUnavailable.Code,
            ErrorCategory.Network => ResultStatuses.ServiceUnavailable.Code,
            ErrorCategory.ExternalService => ResultStatuses.BadGateway.Code,
            ErrorCategory.Database => ResultStatuses.InternalServerError.Code,
            ErrorCategory.Exception => ResultStatuses.InternalServerError.Code,
            ErrorCategory.InternalServerError => ResultStatuses.InternalServerError.Code,
            ErrorCategory.General => ResultStatuses.InternalServerError.Code,
            ErrorCategory.None => ResultStatuses.InternalServerError.Code,
            ErrorCategory.ProblemDetails => ResultStatuses.BadRequest.Code,
            _ => ResultStatuses.InternalServerError.Code,
        };
    }
}
