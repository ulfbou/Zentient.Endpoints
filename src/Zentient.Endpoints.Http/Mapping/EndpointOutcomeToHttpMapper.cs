// <copyright file="EndpointOutcomeToHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

using Zentient.Endpoints;
using Zentient.Endpoints.Http.Models;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Default implementation of <see cref="IEndpointOutcomeToHttpMapper"/>, mapping
    /// <see cref="IEndpointOutcome"/> to ASP.NET Core 
    /// <see cref="Microsoft.AspNetCore.Http.IResult"/>, and applying HTTP-specific metadata for
    /// accurate response generation.
    /// </summary>
    internal sealed class EndpointOutcomeToHttpMapper : IEndpointOutcomeToHttpMapper
    {
        private readonly IProblemDetailsMapper _problemDetailsMapper;
        private readonly ISuccessResponseFactory _successResponseFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public EndpointOutcomeToHttpMapper(
            IProblemDetailsMapper problemDetailsMapper,
            ISuccessResponseFactory successResponseFactory)
        {
            this._problemDetailsMapper = problemDetailsMapper ?? throw new ArgumentNullException(nameof(problemDetailsMapper));
            this._successResponseFactory = successResponseFactory ?? throw new ArgumentNullException(nameof(successResponseFactory));
            this._jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            };
        }

        /// <summary>
        /// Maps asynchronously an <see cref="IEndpointOutcome"/> to an ASP.NET Core 
        /// <see cref="Microsoft.AspNetCore.Http.IResult"/>, using HTTP-specific metadata for
        /// accurate response generation.
        /// </summary>
        /// <param name="outcome">The endpoint outcome to map.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <param name="ct">A cancellation token for the operation.</param>
        /// <returns>
        /// A task representing the asynchronous operation, containing the
        /// mapped <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </returns>
        public async Task<Microsoft.AspNetCore.Http.IResult> Map(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            Microsoft.AspNetCore.Http.IResult result = outcome.IsSuccess
                ? await CreateSuccessResult(outcome, httpContext, ct).ConfigureAwait(false)
                : await CreateFailureResult(outcome, httpContext, ct).ConfigureAwait(false);

            var metadata = outcome.Metadata;
            ImmutableList<KeyValuePair<string, string>> headers = metadata.GetHeaders() ?? ImmutableList<KeyValuePair<string, string>>.Empty;
            var location = metadata.GetLocation();

            return (headers.IsEmpty) && location is null
                ? result
                : new HeaderWrappedResult(result, headers, location);
        }

        private Task<Microsoft.AspNetCore.Http.IResult> CreateSuccessResult(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct)
        {
            var metadata = outcome.Metadata;
            var statusCode = metadata.GetHttpStatusCodeHint() ?? StatusCodes.Status200OK;
            var messages = outcome.Messages?.ToImmutableList() ?? ImmutableList<string>.Empty;
            var statusDescription = ResultStatuses.GetStatus(statusCode).Description;

            object? value = null;
            var outcomeType = outcome.GetType();
            bool isGeneric = false;

            if (outcomeType.IsGenericType && outcomeType.GetGenericTypeDefinition().Name.StartsWith("IEndpointOutcome", StringComparison.Ordinal))
            {
                isGeneric = true;
                value = outcomeType.GetProperty("Value")?.GetValue(outcome);
            }

            // Handle Unit/no-content
            if (statusCode == ResultStatuses.NoContent.Code && (value is Unit || value == null) && messages.Count == 0)
            {
                return Task.FromResult(Microsoft.AspNetCore.Http.Results.StatusCode(statusCode));
            }

            object responsePayload;

            if (isGeneric && value is not Unit)
            {
                var method = typeof(ISuccessResponseFactory).GetMethod("CreateSuccessResponse", new[] {
                    typeof(IEndpointOutcome<>).MakeGenericType(outcomeType.GetGenericArguments()[0]),
                    typeof(int),
                    typeof(string),
                    typeof(IReadOnlyList<string>),
                    outcomeType.GetGenericArguments()[0].IsValueType
                        ? typeof(Nullable<>).MakeGenericType(outcomeType.GetGenericArguments()[0])
                        : outcomeType.GetGenericArguments()[0]
                });

                if (method != null)
                {
                    var genericArg = outcomeType.GetGenericArguments()[0];
                    object? safeValue = value;
                    if (value is null && genericArg.IsValueType && Nullable.GetUnderlyingType(genericArg) is null)
                    {
                        safeValue = Activator.CreateInstance(genericArg);
                    }

                    responsePayload = method.Invoke(_successResponseFactory, new object[]
                    {
                        outcome,
                        statusCode,
                        statusDescription,
                        messages,
                        safeValue!
                    })!;
                }
                else
                {
                    responsePayload = _successResponseFactory.CreateSuccessResponse(
                        outcome,
                        statusCode,
                        statusDescription,
                        messages
                    );
                }
            }
            else
            {
                responsePayload = _successResponseFactory.CreateSuccessResponse(
                    outcome,
                    statusCode,
                    statusDescription,
                    messages
                );
            }

            return Task.FromResult(Microsoft.AspNetCore.Http.Results.Json(responsePayload, statusCode: statusCode));
        }

        private async Task<Microsoft.AspNetCore.Http.IResult> CreateFailureResult(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct)
        {
            var metadata = outcome.Metadata;

            ErrorInfo errorInfo = (outcome.Errors != null && outcome.Errors.Count > 0)
                ? outcome.Errors[0]
                : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unexpected error occurred.");

            ProblemDetails? problem = metadata.GetProblemDetailsOverride()
                ?? await _problemDetailsMapper.Map(errorInfo, httpContext).ConfigureAwait(false);

            int statusCode = metadata.GetHttpStatusCodeHint()
                ?? problem.Status
                ?? ResultStatuses.InternalServerError.Code;

            if (problem.Status != statusCode)
            {
                problem.Status = statusCode;
            }

            var result = Microsoft.AspNetCore.Http.Results.Problem(
                detail: problem.Detail,
                instance: problem.Instance,
                statusCode: problem.Status,
                title: problem.Title,
                type: problem.Type,
                extensions: problem.Extensions);

            return result;
        }
    }
}
