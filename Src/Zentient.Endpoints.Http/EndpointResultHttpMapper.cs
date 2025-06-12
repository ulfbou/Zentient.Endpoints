// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointResultHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Zentient.Endpoints.Core;
using Zentient.Results;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Implements <see cref="IEndpointResultToHttpResultMapper"/> to convert
    /// <see cref="IEndpointResult"/> instances into ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/> objects.
    /// </summary>
    /// <remarks>
    /// This mapper handles both successful results and errors, leveraging <see cref="IProblemDetailsMapper"/>
    /// for robust error representation via HTTP Problem Details.
    /// </remarks>
    public sealed class EndpointResultHttpMapper : IEndpointResultToHttpResultMapper
    {
        private readonly IProblemDetailsMapper _problemDetailsMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointResultHttpMapper"/> class.
        /// </summary>
        /// <param name="problemDetailsMapper">The service used to map <see cref="ErrorInfo"/> to <see cref="ProblemDetails"/>.</param>
        public EndpointResultHttpMapper(IProblemDetailsMapper problemDetailsMapper)
        {
            this._problemDetailsMapper = problemDetailsMapper;
        }

        /// <summary>
        /// Maps the given <see cref="IEndpointResult"/> to an appropriate ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <param name="endpointResult">The endpoint result to map.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> instance.</returns>
        public Microsoft.AspNetCore.Http.IResult Map(IEndpointResult endpointResult, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (endpointResult.BaseResult.IsSuccess)
            {
                return HandleSuccessfulResult(endpointResult);
            }
            else
            {
                return this.HandleFailedResult(endpointResult, httpContext);
            }
        }

        /// <summary>
        /// Handles a successful <see cref="IEndpointResult"/>, returning an appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <param name="endpointResult">The successful endpoint result.</param>
        /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> for a successful operation.</returns>
        private static Microsoft.AspNetCore.Http.IResult HandleSuccessfulResult(IEndpointResult endpointResult)
        {
            object? value = null;
            Type? resultType = null;
            int httpStatusCode;

            Type endpointResultType = endpointResult.GetType();
            Type? iEndpointResultGeneric = endpointResultType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEndpointResult<>));

            if (iEndpointResultGeneric != null)
            {
                resultType = iEndpointResultGeneric.GetGenericArguments()[0];
                PropertyInfo? resultProp = iEndpointResultGeneric.GetProperty("Result");
                value = resultProp?.GetValue(endpointResult);
            }
            else if (endpointResult is IEndpointResult<Unit>)
            {
                resultType = typeof(Unit);
                value = Unit.Value;
            }
            else
            {
                value = null;
            }

            if (value is Unit || value == null)
            {
                httpStatusCode = endpointResult.BaseTransport.HttpStatusCode ?? ResultStatuses.NoContent.Code;
                return new EmptyResultWithStatusCode(httpStatusCode, MediaTypeNames.Application.Json);
            }

            httpStatusCode = endpointResult.BaseTransport.HttpStatusCode ?? ResultStatuses.Success.Code;
            return new NewtonsoftJsonResult(value, httpStatusCode, MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Handles a failed <see cref="IEndpointResult"/>, returning an appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>
        /// (typically a <see cref="ProblemDetails"/> response).
        /// </summary>
        /// <param name="endpointResult">The failed endpoint result.</param>
        /// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
        /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> for a failed operation.</returns>
        private NewtonsoftJsonResult HandleFailedResult(IEndpointResult endpointResult, HttpContext httpContext)
        {
            ErrorInfo errorInfo = endpointResult.BaseResult.Errors != null && endpointResult.BaseResult.Errors.Count > 0
                ? endpointResult.BaseResult.Errors[0]
                : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unexpected error occurred.");

            ProblemDetails problemDetails = endpointResult.BaseTransport.ProblemDetails
                ?? this._problemDetailsMapper.Map(errorInfo, httpContext);

            return new NewtonsoftJsonResult(problemDetails, problemDetails.Status);
        }
    }
}
