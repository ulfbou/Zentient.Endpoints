// <copyright file="EndpointOutcomeHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Zentient.Endpoints;
using Zentient.Results;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides a default implementation of <see cref="IEndpointOutcomeToHttpMapper"/>,
    /// mapping <see cref="IEndpointOutcome"/> instances to ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
    /// </summary>
    internal sealed class EndpointOutcomeHttpMapper : IEndpointOutcomeToHttpMapper
    {
        private readonly IProblemDetailsMapper _problemDetailsMapper;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcomeHttpMapper"/> class.
        /// </summary>
        /// <param name="problemDetailsMapper">The mapper for converting <see cref="ErrorInfo"/> to <see cref="ProblemDetails"/>.</param>
        public EndpointOutcomeHttpMapper(IProblemDetailsMapper problemDetailsMapper)
        {
            this._problemDetailsMapper = problemDetailsMapper;
            this._jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
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
                ? HandleSuccessfulResult(endpointResult, this._jsonSerializerSettings)
                : await this.HandleFailedResultAsync(endpointResult, httpContext).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles successful <see cref="IEndpointOutcome"/> and converts them to appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// This now constructs a <see cref="SuccessResponse{TData}"/> to include messages and status description.
        /// </summary>
        /// <param name="endpointResult">The successful endpoint result.</param>
        /// <param name="serializerSettings">The JSON serializer settings to use.</param>
        /// <returns>A <see cref="Microsoft.AspNetCore.Http.IResult"/> for the successful response.</returns>
        private static Microsoft.AspNetCore.Http.IResult HandleSuccessfulResult(IEndpointOutcome endpointResult, JsonSerializerSettings serializerSettings)
        {
            int httpStatusCode = endpointResult.TransportMetadata.HttpStatusCode
                ?? endpointResult.Status.Code;
            IResultStatus resultStatus = endpointResult.Status; // This is the semantic status
            IReadOnlyList<string> messages = endpointResult.Messages;

            object? value = null;
            if (endpointResult is IEndpointOutcome<object> genericEndpointOutcome)
            {
                value = genericEndpointOutcome.Value;
            }

            if (httpStatusCode == ResultStatuses.NoContent.Code && (value is Unit || value == null) && messages.Count == 0)
            {
                return new EmptyResultWithStatusCode(httpStatusCode, MediaTypeNames.Application.Json);
            }

            var successResponse = new SuccessResponse<object?>(
                        data: value is Unit ? null : value,
                        statusCode: httpStatusCode,
                        statusDescription: resultStatus.Description,
                        messages: messages);
            return new NewtonsoftJsonResult(successResponse, httpStatusCode, MediaTypeNames.Application.Json, serializerSettings);
        }

        /// <summary>
        /// Handles failed <see cref="IEndpointOutcome"/> and converts them to <see cref="ProblemDetails"/>
        /// wrapped in a <see cref="NewtonsoftJsonResult"/>.
        /// </summary>
        /// <param name="endpointResult">The failed endpoint result.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the <see cref="ProblemDetails"/>.</returns>
        private async Task<NewtonsoftJsonResult> HandleFailedResultAsync(IEndpointOutcome endpointResult, HttpContext httpContext)
        {
            ErrorInfo errorInfo = endpointResult.Errors != null && endpointResult.Errors.Any()
                ? endpointResult.Errors[0]
                : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unexpected error occurred.");

            ProblemDetails problemDetails = endpointResult.TransportMetadata.ProblemDetails
                ?? await this._problemDetailsMapper.Map(errorInfo, httpContext).ConfigureAwait(false);

            return new NewtonsoftJsonResult(problemDetails, problemDetails.Status, MediaTypeNames.Application.ProblemJson, this._jsonSerializerSettings);
        }
    }
}
