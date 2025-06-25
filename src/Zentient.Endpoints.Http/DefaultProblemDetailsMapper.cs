// <copyright file="DefaultProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zentient.Results;
using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Options;
using Microsoft.Extensions.Hosting;
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
            _problemTypeUriGenerator = problemTypeUriGenerator ?? throw new ArgumentNullException(nameof(problemTypeUriGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _problemDetailsOptions = options?.Value.ProblemDetails ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance
        /// asynchronously.
        /// </summary>
        /// <param name="errorInfo">
        /// The <see cref="ErrorInfo"/> to map. If <see langword="null"/>, a generic
        /// internal server error ProblemDetails will be returned.
        /// </param>
        /// <param name="httpContext">
        /// The current <see cref="HttpContext"/>, providing additional context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, containing the
        /// <see cref="ProblemDetails"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="httpContext"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="errorInfo"/> has <see cref="ErrorCategory.None"/>, indicating
        /// an issue in upstream result handling.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
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
                    Type = await _problemTypeUriGenerator.Generate(ErrorCodes.InternalServerError, httpContext)
                        .ConfigureAwait(false),
                    Extensions = defaultExtensions,
                };
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
            string problemTypeUriString = await _problemTypeUriGenerator.Generate(errorInfo.Code, httpContext)
                .ConfigureAwait(false);

            var extensions = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
            {
                extensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            if (_problemDetailsOptions.IncludeErrorCodeInExtensions && !string.IsNullOrEmpty(errorInfo.Code))
            {
                extensions[ProblemDetailsConstants.Extensions.ErrorCode] = errorInfo.Code;
            }

            if (!string.IsNullOrEmpty(errorInfo.Detail) && !extensions.ContainsKey(ProblemDetailsConstants.Extensions.Detail))
            {
                extensions[ProblemDetailsConstants.Extensions.Detail] = errorInfo.Detail;
            }

            if (errorInfo.Metadata != null && errorInfo.Metadata.Any())
            {
                foreach (var kvp in errorInfo.Metadata)
                {
                    if (!extensions.ContainsKey(kvp.Key))
                    {
                        extensions[kvp.Key] = kvp.Value;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "ProblemDetails extension key '{Key}' from ErrorInfo metadata conflicts with an existing extension. " +
                            "Value will not be overwritten by ErrorInfo.Metadata.", kvp.Key);
                    }
                }
            }

            if (errorInfo.InnerErrors.Any())
            {
                var mappedInnerErrors = errorInfo.InnerErrors
                    .Select(inner => new Dictionary<string, object?>
                    {
                        [JsonConstants.ErrorInfo.Category] = inner.Category.ToString().ToUpperInvariant(),
                        [JsonConstants.ErrorInfo.Code] = inner.Code,
                        [JsonConstants.ErrorInfo.Message] = inner.Message,
                        [JsonConstants.ErrorInfo.Detail] = inner.Detail,
                        // No JsonConstants for inner errors' Metadata/InnerErrors, so not recursively mapping for now.
                    })
                    .ToList();
                extensions[ProblemDetailsConstants.Extensions.InnerErrors] = mappedInnerErrors;
            }

            if (_problemDetailsOptions.IncludeStackTrace && _environment.IsDevelopment())
            {
                if (errorInfo.Metadata != null
                    && errorInfo.Metadata.TryGetValue(MetadataKeys.ExceptionStackTrace, out var stackTrace)
                    && stackTrace is string st)
                {
                    extensions[MetadataKeys.ExceptionStackTrace] = st;
                }
                else
                {
                    _logger.LogWarning(
                        "ProblemDetails.IncludeStackTrace is true in development, but no '{StackTraceKey}' key found in ErrorInfo metadata for Problem Details.",
                        MetadataKeys.ExceptionStackTrace);
                }
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

            return problemDetails;
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
            if (_problemDetailsOptions.CategoryToStatusCodeMap.TryGetValue(category.ToString(), out int statusCodeFromMap))
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
