// <copyright file="EndpointOutcomeToHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Extensions;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// Default implementation of <see cref="IEndpointOutcomeToHttpMapper"/>, mapping
    /// <see cref="IEndpointOutcome"/> to ASP.NET Core
    /// <see cref="Microsoft.AspNetCore.Http.IResult"/>, and applying HTTP-specific metadata for
    /// accurate response generation.
    /// </summary>
    public sealed class EndpointOutcomeToHttpMapper : IEndpointOutcomeToHttpMapper
    {
        private readonly IProblemDetailsMapper _problemDetailsMapper;
        private readonly IProblemTypeUriGenerator _problemTypeUriGenerator;
        private readonly ISuccessResponseFactory _successResponseFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly SuccessResponseOptions _successResponseOptions;
        private readonly Options.ProblemDetailsOptions _problemDetailsOptions;
        private readonly bool _isDevelopment;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcomeToHttpMapper"/> class.
        /// </summary>
        /// <param name="problemDetailsMapper">
        /// The <see cref="IProblemDetailsMapper"/> to use for mapping errors to
        /// <see cref="ProblemDetails"/>.
        /// </param>
        /// <param name="problemTypeUriGenerator">
        /// The <see cref="IProblemTypeUriGenerator"/> to use for generating URIs for problem types
        /// from error codes.
        /// </param>
        /// <param name="successResponseFactory">
        /// The <see cref="ISuccessResponseFactory"/> to use for creating success responses
        /// from endpoint outcomes.
        /// </param>
        /// <param name="options">
        /// The <see cref="IOptions{EndpointsHttpOptions}"/> containing configuration
        /// for the mapper, including JSON serialization options and HTTP-specific settings.
        /// </param>
        /// <param name="environment">
        /// The <see cref="IWebHostEnvironment"/> to determine the application's environment
        /// (e.g., Development for including stack traces).
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="options"/>, <paramref name="problemDetailsMapper"/>, or
        /// <paramref name="successResponseFactory"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// This constructor initializes the mapper with the provided
        /// <paramref name="problemDetailsMapper"/> and
        /// <paramref name="successResponseFactory"/>, and configures the JSON serialization
        /// options based on the provided <paramref name="options"/>.
        /// If the JSON serializer options are not set in the options, it defaults to a
        /// new instance with camelCase naming policy, indented formatting disabled, and null value
        /// ignoring behavior set to <see cref="JsonIgnoreCondition.WhenWritingNull"/>,
        /// including a <see cref="JsonStringEnumConverter"/>.
        /// </remarks>
        public EndpointOutcomeToHttpMapper(
            IProblemDetailsMapper problemDetailsMapper,
            IProblemTypeUriGenerator problemTypeUriGenerator,
            ISuccessResponseFactory successResponseFactory,
            IOptions<EndpointsHttpOptions> options,
            IWebHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(problemDetailsMapper, nameof(problemDetailsMapper));
            ArgumentNullException.ThrowIfNull(problemTypeUriGenerator, nameof(problemTypeUriGenerator));
            ArgumentNullException.ThrowIfNull(successResponseFactory, nameof(successResponseFactory));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            ArgumentNullException.ThrowIfNull(environment, nameof(environment));

            this._problemDetailsMapper = problemDetailsMapper;
            this._problemTypeUriGenerator = problemTypeUriGenerator;
            this._successResponseFactory = successResponseFactory;
            this._isDevelopment = string.Equals(environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);
            this._jsonSerializerOptions = options.Value.JsonSerializerOptions ?? new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = this._isDevelopment,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            if (!this._jsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                this._jsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }

            this._successResponseOptions = options.Value.SuccessResponse ?? new SuccessResponseOptions();
            this._problemDetailsOptions = options.Value.ProblemDetails ?? new Options.ProblemDetailsOptions();
        }

        /// <summary>Gets the JSON serializer options used by this mapper.</summary>
        /// <value>The <see cref="JsonSerializerOptions"/> used for serializing responses.</value>
        internal JsonSerializerOptions JsonSerializerOptions => this._jsonSerializerOptions;

        /// <summary>Gets the success response options used by this mapper.</summary>
        /// <value>The <see cref="SuccessResponseOptions"/> that define default status codes and response behavior.</value>
        internal SuccessResponseOptions SuccessResponseOptions => this._successResponseOptions;

        /// <summary>Gets the problem details options used by this mapper.</summary>
        /// <value>The <see cref="Options.ProblemDetailsOptions"/> that define how problem details are generated.</value>
        internal Options.ProblemDetailsOptions ProblemDetailsOptions => this._problemDetailsOptions;

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
                ? await this.CreateSuccessResult(outcome, httpContext, ct).ConfigureAwait(false)
                : await this.CreateFailureResult(outcome, httpContext, ct).ConfigureAwait(false);

            var metadata = outcome.Metadata;
            ImmutableDictionary<string, string> headers = metadata.GetHeaders().ToImmutableDictionary();
            var location = metadata.GetLocationUri();

            return headers.IsEmpty && location is null
                ? result
                : new HeaderWrappedResult(result, headers, location);
        }

        private Task<Microsoft.AspNetCore.Http.IResult> CreateSuccessResult(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct)
        {
            var metadata = outcome.Metadata;
            var statusCode = metadata.GetHttpStatusCodeHint()
                ?? this._successResponseOptions.DefaultOkStatusCode;
            var messages = outcome.Messages?.ToImmutableList()
                ?? ImmutableList<string>.Empty;
            var statusDescription = ResultStatuses.GetStatus(statusCode).Description;

            object? value = null;
            bool isGenericOutcomeWithConcreteValue = false;

            if (outcome.GetType().IsGenericType
                && outcome.GetType().GetGenericTypeDefinition() == typeof(EndpointOutcome<>))
            {
                var valueProperty = outcome.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    value = valueProperty.GetValue(outcome);
                    if (value is not null && value.GetType() != typeof(Unit))
                    {
                        isGenericOutcomeWithConcreteValue = true;
                    }
                }
            }

            if (statusCode == this._successResponseOptions.DefaultNoContentStatusCode
                && (value is Unit || value == null)
                && messages.Count == 0)
            {
                return Task.FromResult(Microsoft.AspNetCore.Http.Results.StatusCode(statusCode));
            }

            object? responsePayload;

            if (isGenericOutcomeWithConcreteValue)
            {
                responsePayload = this._successResponseFactory.CreateSuccessResponse(
                    (dynamic)outcome,
                    statusCode,
                    statusDescription,
                    messages,
                    value);
            }
            else
            {
                responsePayload = this._successResponseFactory.CreateSuccessResponse(
                    outcome,
                    statusCode,
                    statusDescription,
                    messages);
            }

            if (responsePayload is null && statusCode == this._successResponseOptions.DefaultNoContentStatusCode)
            {
                return Task.FromResult(Microsoft.AspNetCore.Http.Results.StatusCode(statusCode));
            }

            return Task.FromResult(Microsoft.AspNetCore.Http.Results.Json(
                responsePayload,
                this._jsonSerializerOptions,
                statusCode: statusCode));
        }

        private async Task<Microsoft.AspNetCore.Http.IResult> CreateFailureResult(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct)
        {
            var metadata = outcome.Metadata;
            ErrorInfo errorInfo = (outcome.Errors != null && outcome.Errors.Count > 0)
                ? outcome.Errors[0]
                : new ErrorInfo(
                    ErrorCategory.InternalServerError,
                    code: ResultStatuses.InternalServerError.Code.ToString(CultureInfo.InvariantCulture),
                    message: ResultStatuses.InternalServerError.Description);

            ProblemDetails? problem = metadata.GetProblemDetailsOverride();
            bool usedOverride = problem is not null;

            if (!usedOverride)
            {
                problem = await this._problemDetailsMapper.Map(errorInfo, httpContext).ConfigureAwait(false);
            }

            int statusCode = metadata.GetHttpStatusCodeHint()
                ?? problem?.Status
                ?? ResultStatuses.InternalServerError.Code;

            if (problem!.Status == null || problem.Status != statusCode)
            {
                problem.Status = statusCode;
            }

            problem.Extensions ??= new Dictionary<string, object?>();

            if (!usedOverride)
            {
                problem.Title = ResultStatuses.GetStatus(statusCode, "An Error Occurred").Description;
                problem.Detail = errorInfo.Message;
            }

            problem.Type = await this._problemTypeUriGenerator.Generate(errorInfo.Code, httpContext).ConfigureAwait(false);
            problem.Instance = httpContext.Request.Path;

            if (this._problemDetailsOptions.IncludeErrorCodeInExtensions
                && !string.IsNullOrEmpty(errorInfo.Code)
                && !problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.ErrorCode))
            {
                problem.Extensions[ProblemDetailsConstants.Extensions.ErrorCode] = errorInfo.Code;
            }

            if (!string.IsNullOrEmpty(errorInfo.Detail)
                && !problem.Extensions.ContainsKey(ProblemDetailsConstants.Detail))
            {
                problem.Extensions[ProblemDetailsConstants.Detail] = errorInfo.Detail;
            }

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier)
                && !problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.TraceId))
            {
                problem.Extensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            if (this._problemDetailsOptions.IncludeStackTrace && this._isDevelopment)
            {
                if (errorInfo.Metadata != null
                    && errorInfo.Metadata.TryGetValue(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, out var stackTrace)
                    && stackTrace is string st)
                {
                    if (!problem.Extensions.ContainsKey(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace))
                    {
                        problem.Extensions[Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace] = st;
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
                    })
                    .ToList();
                if (!problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.InnerErrors))
                {
                    problem.Extensions[ProblemDetailsConstants.Extensions.InnerErrors] = mappedInnerErrors;
                }
            }

            return Microsoft.AspNetCore.Http.Results.Content(
                JsonSerializer.Serialize(problem, this._jsonSerializerOptions),
                contentType: "application/problem+json",
                statusCode: problem.Status);
        }
    }
}
