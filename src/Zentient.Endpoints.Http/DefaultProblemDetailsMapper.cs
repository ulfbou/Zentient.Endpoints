// <copyright file="DefaultProblemDetailsMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

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
    internal sealed class DefaultProblemDetailsMapper : IProblemDetailsMapper
    {
        private readonly IProblemTypeUriGenerator _problemTypeUriGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProblemDetailsMapper"/> class.
        /// </summary>
        /// <param name="problemTypeUriGenerator">The problem type URI generator to use. If <c>null</c>, a default <see cref="DefaultProblemTypeUriGenerator"/> is used.</param>
        public DefaultProblemDetailsMapper(IProblemTypeUriGenerator? problemTypeUriGenerator = null)
        {
            this._problemTypeUriGenerator = problemTypeUriGenerator ?? new DefaultProblemTypeUriGenerator();
        }

        /// <summary>
        /// Maps an <see cref="ErrorInfo"/> object to a <see cref="ProblemDetails"/> instance asynchronously.
        /// </summary>
        /// <param name="errorInfo">The <see cref="ErrorInfo"/> to map. If <c>null</c>, a generic internal server error ProblemDetails will be returned.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>, providing additional context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="ProblemDetails"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpContext"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="errorInfo"/> has <see cref="ErrorCategory.None"/>, indicating an issue in upstream result handling.</exception>
        public async Task<Microsoft.AspNetCore.Mvc.ProblemDetails> Map(ErrorInfo? errorInfo, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (errorInfo == null)
            {
                var defaultExtensions = new Dictionary<string, object?>();

                if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
                {
                    defaultExtensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
                }

                return await Task.FromResult(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = ResultStatuses.Error.Code,
                    Title = ResultStatuses.Error.Description,
                    Detail = "No error information was provided.",
                    Instance = httpContext.Request.Path,
                    Type = this._problemTypeUriGenerator?.GenerateProblemTypeUri(null)?.ToString(),
                    Extensions = defaultExtensions,
                }).ConfigureAwait(false);
            }

            ErrorInfo actualErrorInfo = (ErrorInfo)errorInfo;

            if (actualErrorInfo.Category == ErrorCategory.None)
            {
                throw new InvalidOperationException(
                    $"Cannot map ErrorCategory.None to ProblemDetails. " +
                    $"The '{nameof(DefaultProblemDetailsMapper)}' expects an actual error category. " +
                    $"This indicates an issue in the upstream result handling where a non-error was passed for problem mapping. " +
                    $"ErrorInfo Code: {actualErrorInfo.Code ?? "N/A"}, Message: {actualErrorInfo.Message ?? "N/A"}");
            }

            int statusCode = GetHttpStatusCode(actualErrorInfo.Category);
            var populatedExtensions = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(actualErrorInfo.Code))
            {
                populatedExtensions[ProblemDetailsConstants.Extensions.ErrorCode] = actualErrorInfo.Code;
            }

            if (!string.IsNullOrEmpty(actualErrorInfo.Detail))
            {
                populatedExtensions[ProblemDetailsConstants.Extensions.Detail] = actualErrorInfo.Detail;
            }

            if (actualErrorInfo.Data is IReadOnlyList<object> dataList && dataList.Count > 0)
            {
                populatedExtensions[ProblemDetailsConstants.Extensions.Data] = dataList;
            }

            if (actualErrorInfo.InnerErrors is IReadOnlyList<ErrorInfo> innerErrorsList && innerErrorsList.Count > 0)
            {
                populatedExtensions[ProblemDetailsConstants.Extensions.InnerErrors] = innerErrorsList;
            }

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier))
            {
                populatedExtensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            foreach (KeyValuePair<string, object?> kvp in actualErrorInfo.Extensions)
            {
                if (!populatedExtensions.ContainsKey(kvp.Key))
                {
                    populatedExtensions[kvp.Key] = kvp.Value;
                }
            }

            string? problemTypeUriString = this._problemTypeUriGenerator?.GenerateProblemTypeUri(actualErrorInfo.Code)?.ToString();

            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = ResultStatuses.GetStatus(statusCode, "An Error Occurred").Description,
                Detail = actualErrorInfo.Message,
                Type = problemTypeUriString,
                Instance = httpContext.Request.Path,
                Extensions = populatedExtensions,
            };

            return await Task.FromResult(problemDetails).ConfigureAwait(false);
        }

        /// <summary>
        /// Converts an <see cref="ErrorCategory"/> to an appropriate HTTP status code.
        /// </summary>
        /// <param name="category">The error category from <see cref="Zentient.Results.ErrorInfo"/>.</param>
        /// <returns>The corresponding HTTP status code.</returns>
        private static int GetHttpStatusCode(ErrorCategory category) => category switch
        {
            ErrorCategory.Validation => ResultStatuses.BadRequest.Code,
            ErrorCategory.NotFound => ResultStatuses.NotFound.Code,
            ErrorCategory.Conflict => ResultStatuses.Conflict.Code,
            ErrorCategory.Unauthorized => ResultStatuses.Unauthorized.Code,
            ErrorCategory.Forbidden => ResultStatuses.Forbidden.Code,
            ErrorCategory.Concurrency => ResultStatuses.Conflict.Code,
            ErrorCategory.ServiceUnavailable => ResultStatuses.ServiceUnavailable.Code,
            ErrorCategory.Timeout => ResultStatuses.RequestTimeout.Code,
            ErrorCategory.TooManyRequests => ResultStatuses.TooManyRequests.Code,
            ErrorCategory.InternalServerError => ResultStatuses.Error.Code,
            ErrorCategory.None => ResultStatuses.Error.Code,
            ErrorCategory.General => ResultStatuses.Error.Code,
            ErrorCategory.Authentication => ResultStatuses.Unauthorized.Code,
            ErrorCategory.Authorization => ResultStatuses.Forbidden.Code,
            ErrorCategory.Exception => ResultStatuses.Error.Code,
            ErrorCategory.Network => ResultStatuses.ServiceUnavailable.Code,
            ErrorCategory.Database => ResultStatuses.Error.Code,
            ErrorCategory.Security => ResultStatuses.Forbidden.Code,
            ErrorCategory.Request => ResultStatuses.BadRequest.Code,
            ErrorCategory.ExternalService => 502,
            ErrorCategory.BusinessLogic => ResultStatuses.BadRequest.Code,
            ErrorCategory.ProblemDetails => ResultStatuses.BadRequest.Code,
            _ => ResultStatuses.Error.Code,
        };
    }
}
