// <copyright file="EndpointOutcomeHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Zentient.Endpoints;
using Zentient.Results;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides a default implementation of <see cref="IEndpointOutcomeToHttpMapper"/>,
    /// mapping <see cref="IEndpointOutcome"/> instances to ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
    /// </summary>
    internal sealed class EndpointOutcomeHttpMapper : IEndpointOutcomeToHttpMapper
    {
        private readonly IProblemDetailsMapper _problemDetailsMapper;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcomeHttpMapper"/> class.
        /// </summary>
        /// <param name="problemDetailsMapper">The mapper for converting <see cref="ErrorInfo"/> to <see cref="ProblemDetails"/>.</param>
        public EndpointOutcomeHttpMapper(IProblemDetailsMapper problemDetailsMapper)
        {
            this._problemDetailsMapper = problemDetailsMapper;
            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false, // Set to true for readable output during debugging if needed

                // This is the crucial setting.
                // WhenWritingNull generally means null properties are skipped.
                // For ProblemDetails.Extensions (which is a Dictionary),
                // sometimes an empty dictionary might be treated as "default" or "null" depending on context.
                // To guarantee serialization of the "extensions" property even if the dictionary is empty,
                // or contains only properties that might be considered "default" by some rules,
                // set it to JsonIgnoreCondition.Never.
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            };
        }

        /// <summary>
        /// Maps an <see cref="IEndpointOutcome"/> to an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> asynchronously.
        /// </summary>
        /// <param name="endpointResult">The endpoint result to map.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the mapped HTTP response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpointResult"/> or <paramref name="httpContext"/> is <c>null</c>.</exception>
        public async Task<Microsoft.AspNetCore.Http.IResult> Map(IEndpointOutcome endpointResult, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            return endpointResult.IsSuccess
                ? HandleSuccessfulResult(endpointResult, this._jsonSerializerOptions)
                : await this.HandleFailedResultAsync(endpointResult, httpContext).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles successful <see cref="IEndpointOutcome"/> and converts them to appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// This now constructs a <see cref="SuccessResponse{TData}"/> to include messages and status description.
        /// </summary>
        /// <param name="endpointResult">The successful endpoint result.</param>
        /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/> to use for serialization.</param>
        /// <returns>A <see cref="Microsoft.AspNetCore.Http.IResult"/> for the successful response.</returns>
        private static Microsoft.AspNetCore.Http.IResult HandleSuccessfulResult(IEndpointOutcome endpointResult, JsonSerializerOptions serializerOptions)
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            ArgumentNullException.ThrowIfNull(serializerOptions, nameof(serializerOptions));

            int httpStatusCode = endpointResult.TransportMetadata.HttpStatusCode
                ?? endpointResult.Status.Code;
            IResultStatus resultStatus = endpointResult.Status;
            IReadOnlyList<string> messages = endpointResult.Messages;

            object? value = null;
            if (endpointResult is IEndpointOutcome<object> genericEndpointOutcome)
            {
                value = genericEndpointOutcome.Value;
            }

            if (httpStatusCode == ResultStatuses.NoContent.Code && (value is Unit || value == null) && messages.Count == 0)
            {
                // Use Results.StatusCode for empty 204 response
                return Microsoft.AspNetCore.Http.Results.StatusCode(httpStatusCode);
            }

            var successResponse = new SuccessResponse<object?>(
                        data: value is Unit ? null : value,
                        statusCode: httpStatusCode,
                        statusDescription: resultStatus.Description,
                        messages: messages);

            // Use Results.Json for System.Text.Json serialization
            return Microsoft.AspNetCore.Http.Results.Json(successResponse, serializerOptions, MediaTypeNames.Application.Json, statusCode: httpStatusCode);
        }

        /// <summary>
        /// Handles failed <see cref="IEndpointOutcome"/> and converts them to <see cref="ProblemDetails"/>
        /// wrapped in a JSON result.
        /// </summary>
        /// <param name="endpointResult">The failed endpoint result.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="ProblemDetails"/>.</returns>
        private async Task<Microsoft.AspNetCore.Http.IResult> HandleFailedResultAsync(IEndpointOutcome endpointResult, HttpContext httpContext)
        {
            ErrorInfo errorInfo = endpointResult.Errors != null && endpointResult.Errors.Any()
                ? endpointResult.Errors[0]
                : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unexpected error occurred.");

            ProblemDetails problemDetails = endpointResult.TransportMetadata.ProblemDetails
                ?? await this._problemDetailsMapper.Map(errorInfo, httpContext).ConfigureAwait(false);

            // Use Results.Json for ProblemDetails with application/problem+json
            return Microsoft.AspNetCore.Http.Results.Json(problemDetails, this._jsonSerializerOptions, MediaTypeNames.Application.ProblemJson, statusCode: problemDetails.Status);
        }
    }
}
