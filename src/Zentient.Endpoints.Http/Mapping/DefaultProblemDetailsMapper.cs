// <copyright file="DefaultProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Provides a default implementation of <see cref="IProblemDetailsMapper"/>,
    /// translating <see cref="ErrorInfo"/> into standard <see cref="ProblemDetails"/>.
    /// </summary>
    /// <remarks>
    /// This mapper converts <see cref="ErrorInfo.Category"/> to a corresponding HTTP status code
    /// and populates <see cref="ProblemDetails"/> fields like Title, Detail, and Extensions
    /// with information from the <see cref="ErrorInfo"/>, respecting configurations from
    /// <see cref="ProblemDetailsOptions"/>.
    /// </remarks>
    internal sealed class DefaultProblemDetailsMapper : IProblemDetailsMapper
    {
        private readonly IProblemTypeUriGenerator _problemTypeUriGenerator;
        private readonly ILogger<DefaultProblemDetailsMapper> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly Zentient.Endpoints.Http.Options.ProblemDetailsOptions _problemDetailsOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProblemDetailsMapper"/> class.
        /// </summary>
        /// <param name="problemTypeUriGenerator">The problem type URI generator to use.</param>
        /// <param name="logger">The logger for diagnostic messages.</param>
        /// <param name="environment">
        /// The web host environment, used to determine development mode.
        /// </param>
        /// <param name="options">
        /// The <see cref="IOptions{EndpointsHttpOptions}"/> containing configuration for Problem
        /// Details, including base URI, stack trace inclusion, and category-to-status code mapping.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="problemTypeUriGenerator"/>, <paramref name="logger"/>,
        /// <paramref name="environment"/>, or <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        public DefaultProblemDetailsMapper(
            IProblemTypeUriGenerator problemTypeUriGenerator,
            ILogger<DefaultProblemDetailsMapper> logger,
            IWebHostEnvironment environment,
            IOptions<EndpointsHttpOptions> options)
        {
            this._problemTypeUriGenerator = problemTypeUriGenerator ?? throw new ArgumentNullException(nameof(problemTypeUriGenerator));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._problemDetailsOptions = options?.Value.ProblemDetails ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public async Task<ProblemDetails> Map(ErrorInfo? errorInfo, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (errorInfo == null)
            {
                var defaultExtensions = new Dictionary<string, object?>
                {
                    [ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier,
                };

                return new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = ResultStatuses.GetStatus(
                        ResultStatuses.InternalServerError.Code,
                        "An Error Occurred")
                        .Description,
                    Detail = "An unexpected error occurred and no specific error information was provided.",
                    Instance = httpContext.Request.Path,
                    Type = await this._problemTypeUriGenerator.Generate(ErrorCodes.InternalServerError, httpContext)
                        .ConfigureAwait(false),
                    Extensions = defaultExtensions,
                };
            }

            if (errorInfo.Category == ErrorCategory.None)
            {
                throw new InvalidOperationException(
                    $"Cannot map '{ErrorCategory.None}' to ProblemDetails. " +
                    $"This indicates a bug in upstream result handling—non-error passed for error mapping. " +
                    $"Code: {errorInfo.Code ?? "N/A"}, Message: {errorInfo.Message ?? "N/A"}");
            }

            int statusCode = this.GetHttpStatusCode(errorInfo.Category);
            string problemTypeUriString = await this._problemTypeUriGenerator.Generate(errorInfo.Code, httpContext)
                .ConfigureAwait(false);

            var extensions = new Dictionary<string, object?>();
            this.PopulateExtensions(extensions, errorInfo, httpContext);

            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ResultStatuses.GetStatus(statusCode, "An Error Occurred").Description,
                Detail = errorInfo.Message,
                Type = problemTypeUriString,
                Instance = httpContext.Request.Path,
                Extensions = extensions,
            };

            return problemDetails;
        }

        /// <summary>
        /// Populates the extensions dictionary of a <see cref="ProblemDetails"/> instance
        /// based on the provided <see cref="ErrorInfo"/> and <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="extensions">The dictionary to populate with extensions.</param>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> containing error details.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        private void PopulateExtensions(
            Dictionary<string, object?> extensions,
            ErrorInfo errorInfo,
            HttpContext httpContext)
        {
            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
            {
                extensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            if (this._problemDetailsOptions.IncludeErrorCodeInExtensions && !string.IsNullOrEmpty(errorInfo.Code))
            {
                extensions[ProblemDetailsConstants.Extensions.ErrorCode] = errorInfo.Code;
            }

            if (!string.IsNullOrEmpty(errorInfo.Detail) && !extensions.ContainsKey(ProblemDetailsConstants.Detail))
            {
                extensions[ProblemDetailsConstants.Detail] = errorInfo.Detail;
            }

            if (errorInfo.Metadata is { Count: > 0 })
            {
                var metadata = errorInfo.Metadata.AsEnumerable().Where(kvp => kvp.Key != Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);

                foreach (var kvp in metadata)
                {
                    if (!extensions.ContainsKey(kvp.Key))
                    {
                        extensions[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "ProblemDetails extension key '{Key}' from ErrorInfo metadata conflicts with an existing extension. Value will not be overwritten by ErrorInfo.Metadata.",
                            kvp.Key);
                    }
                }

                if (this._problemDetailsOptions.IncludeStackTrace && this._environment.IsDevelopment())
                {
                    if (errorInfo.Metadata != null
                        && errorInfo.Metadata.TryGetValue(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, out var stackTrace)
                        && stackTrace is string st)
                    {
                        extensions[Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace] = st;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "ProblemDetails.IncludeStackTrace is true in development, but no '{StackTraceKey}' key found in ErrorInfo metadata for Problem Details.",
                            Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);
                    }
                }
            }

            if (errorInfo.InnerErrors is { Count: > 0 })
            {
                var mappedInnerErrors = errorInfo.InnerErrors
                    .Select(inner => new Dictionary<string, object?>
                    {
                        [JsonConstants.ErrorInfo.Category] = inner.Category.ToString().ToUpperInvariant(),
                        [JsonConstants.ErrorInfo.Code] = inner.Code,
                        [JsonConstants.ErrorInfo.Message] = inner.Message,
                        [JsonConstants.ErrorInfo.Detail] = inner.Detail,
                    })
                    .ToList();
                extensions[ProblemDetailsConstants.Extensions.InnerErrors] = mappedInnerErrors;
            }

            if (this._problemDetailsOptions.IncludeStackTrace && this._environment.IsDevelopment())
            {
                if (errorInfo.Metadata != null
                    && errorInfo.Metadata.TryGetValue(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, out var stackTrace)
                    && stackTrace is string st)
                {
                    extensions[Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace] = st;
                }
                else
                {
                    this._logger.LogWarning(
                        "ProblemDetails.IncludeStackTrace is true in development, but no '{StackTraceKey}' key found in ErrorInfo metadata for Problem Details.",
                        Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);
                }
            }
        }

        /// <summary>
        /// Converts an <see cref="ErrorCategory"/> to an appropriate HTTP status code, prioritizing
        /// the custom mapping defined in <see cref="ProblemDetailsOptions"/> via its
        /// <c>CategoryToStatusCodeMap</c> property.
        /// </summary>
        /// <param name="category">
        /// The error category from <see cref="Zentient.Results.ErrorInfo"/>.
        /// </param>
        /// <returns>The corresponding HTTP status code.</returns>
        private int GetHttpStatusCode(ErrorCategory category)
        {
            if (this._problemDetailsOptions.CategoryToStatusCodeMap.TryGetValue(category.ToString(), out int statusCodeFromMap))
            {
                return statusCodeFromMap;
            }

            return category switch
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
}
