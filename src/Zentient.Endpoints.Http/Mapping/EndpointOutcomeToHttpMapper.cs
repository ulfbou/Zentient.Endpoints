// <copyright file="EndpointOutcomeToHttpMapper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Extensions;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;
using Zentient.Results.Constants;

using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var messages = outcome.Messages?.ToImmutableList()
                           ?? ImmutableList<string>.Empty;

            object? value = null;
            bool isGenericOutcomeWithConcreteValue = false;
            Type? outcomeValueType = null; // To store the T from IEndpointOutcome<T>

            // Determine if it's a generic outcome and extract the value and its type
            if (outcome.GetType().IsGenericType
                && outcome.GetType().GetGenericTypeDefinition() == typeof(EndpointOutcome<>))
            {
                var valueProperty = outcome.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    value = valueProperty.GetValue(outcome);
                    outcomeValueType = valueProperty.PropertyType; // Get the actual generic type argument

                    // A concrete value is anything other than null AND not a Unit
                    if (value is not null && outcomeValueType != typeof(Unit))
                    {
                        isGenericOutcomeWithConcreteValue = true;
                    }
                }
            }

            int determinedStatusCode;

            var statusCodeHint = metadata.GetHttpStatusCodeHint();
            if (statusCodeHint.HasValue)
            {
                determinedStatusCode = statusCodeHint.Value;
            }
            // If no explicit hint, and it's specifically a Unit outcome, use DefaultNoContentStatusCode.
            else if (outcomeValueType == typeof(Unit))
            {
                determinedStatusCode = this._successResponseOptions.DefaultNoContentStatusCode;
            }
            // For all other success outcomes (no hint, not Unit), use the DefaultOkStatusCode.
            // This handles generic outcomes with concrete values, and non-generic outcomes
            // like EndpointOutcome.Success() which should default to DefaultOkStatusCode (e.g., 200 OK or 201 Created).
            else
            {
                determinedStatusCode = this._successResponseOptions.DefaultOkStatusCode;
            }

            var statusDescription = ResultStatuses.GetStatus(determinedStatusCode).Description;

            // Strict HTTP spec adherence for 204/205: NO BODY ALLOWED.
            if (determinedStatusCode == StatusCodes.Status204NoContent || determinedStatusCode == StatusCodes.Status205ResetContent)
            {
                return Task.FromResult(Microsoft.AspNetCore.Http.Results.StatusCode(determinedStatusCode));
            }

            object? responsePayload;

            // This logic determines which CreateSuccessResponse overload to call on the factory.
            // It should only pass `value` if it's a concrete, non-Unit value.
            if (isGenericOutcomeWithConcreteValue)
            {
                var createSuccessResponseMethod = typeof(ISuccessResponseFactory)
                    .GetMethods()
                    .Where(m => m.Name == nameof(ISuccessResponseFactory.CreateSuccessResponse) && m.IsGenericMethod)
                    .Single();

                var genericMethod = createSuccessResponseMethod.MakeGenericMethod(outcomeValueType!);

                responsePayload = genericMethod.Invoke(
                    this._successResponseFactory,
                    new object?[] { outcome, determinedStatusCode, statusDescription, messages, value });
            }
            else
            {
                responsePayload = this._successResponseFactory.CreateSuccessResponse(
                    outcome,
                    determinedStatusCode,
                    statusDescription,
                    messages);
            }

            return Task.FromResult(Microsoft.AspNetCore.Http.Results.Json(
                responsePayload,
                this._jsonSerializerOptions,
                statusCode: determinedStatusCode));
        }

        private async Task<Microsoft.AspNetCore.Http.IResult> CreateFailureResult(
            IEndpointOutcome outcome,
            HttpContext httpContext,
            CancellationToken ct)
        {
            var errorInfo = GetPrimaryErrorInfo(outcome);
            var (problem, usedOverride) = await GetProblemDetails(outcome, errorInfo, httpContext).ConfigureAwait(false);

            EndpointOutcomeToHttpMapper.SetProblemDetailsStatus(problem, outcome.Metadata, usedOverride);
            await SetProblemDetailsTypeAndInstance(problem, errorInfo, httpContext, usedOverride).ConfigureAwait(false);
            SetProblemDetailsDetail(problem, errorInfo, outcome, usedOverride); // <-- ADD usedOverride HERE
            SetProblemDetailsExtensions(problem, errorInfo, outcome, httpContext);

            return Microsoft.AspNetCore.Http.Results.Content(
                JsonSerializer.Serialize(problem, _jsonSerializerOptions),
                contentType: "application/problem+json",
                statusCode: problem.Status);
        }

        private static ErrorInfo GetPrimaryErrorInfo(IEndpointOutcome outcome)
        {
            return (outcome.Errors != null && outcome.Errors.Count > 0)
                ? outcome.Errors[0]
                : new ErrorInfo(
                    ErrorCategory.InternalServerError,
                    code: Results.Constants.ErrorCodes.InternalServerError,
                    message: ResultStatuses.InternalServerError.Description);
        }

        private async Task<(ProblemDetails Problem, bool UsedOverride)> GetProblemDetails(
            IEndpointOutcome outcome,
            ErrorInfo errorInfo,
            HttpContext httpContext)
        {
            var problem = outcome.Metadata.GetProblemDetailsOverride();
            bool usedOverride = problem is not null;

            if (!usedOverride)
            {
                problem = await _problemDetailsMapper.Map(errorInfo, httpContext).ConfigureAwait(false);
            }

            // Ensure ProblemDetails is not null, even if mapper returns null (shouldn't happen ideally)
            problem ??= new ProblemDetails();

            return (problem, usedOverride);
        }

        private static void SetProblemDetailsStatus(ProblemDetails problem, TransportMetadata metadata, bool usedOverride)
        {
            int statusCode = metadata.GetHttpStatusCodeHint()
                             ?? problem.Status
                             ?? StatusCodes.Status500InternalServerError;

            if (problem.Status == null || problem.Status != statusCode)
            {
                problem.Status = statusCode;
                if (!usedOverride)
                {
                    // Update title if status changed and no override was used.
                    // The _problemDetailsMapper should ideally set this based on the error.
                    // If not, this provides a fallback based on the HTTP status code.
                    problem.Title = ResultStatuses.GetStatus(statusCode, "An Error Occurred").Description;
                }
            }
        }

        private async Task SetProblemDetailsTypeAndInstance(
            ProblemDetails problem,
            ErrorInfo errorInfo,
            HttpContext httpContext,
            bool usedOverride)
        {
            problem.Extensions ??= new Dictionary<string, object?>();

            if (string.IsNullOrEmpty(problem.Type) || (!usedOverride && problem.Type == ProblemDetailsConstants.DefaultBaseUri.AbsoluteUri))
            {
                // Only try to generate type if it's not set by override OR it's a generic default from mapper and not overridden
                string generatedType = await _problemTypeUriGenerator.Generate(errorInfo.Code, httpContext).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(generatedType) && generatedType != ProblemDetailsConstants.DefaultBaseUri.AbsoluteUri)
                {
                    problem.Type = generatedType;
                }
            }

            if (string.IsNullOrEmpty(problem.Instance))
            {
                problem.Instance = httpContext.Request.Path;
            }
        }

        private void SetProblemDetailsDetail(
            ProblemDetails problem,
            ErrorInfo errorInfo,
            IEndpointOutcome outcome,
            bool usedOverride)
        {
            // FIX: If an override was used, assume its detail is authoritative
            // and do not modify it further with errorInfo.Detail or outcome.Messages.
            if (usedOverride)
            {
                return;
            }

            // Original logic below, which now only applies when no override was used.

            // Set problem.Detail from errorInfo.Detail if not an override
            if (string.IsNullOrEmpty(problem.Detail) && !string.IsNullOrEmpty(errorInfo.Detail))
            {
                problem.Detail = errorInfo.Detail;
            }
            // Add outcome messages to detail if option is enabled
            if (_problemDetailsOptions.IncludeErrorInfoMessagesInDetail && outcome.Messages != null && outcome.Messages.Any())
            {
                string messagesDetail = string.Join(" ", outcome.Messages);
                if (!string.IsNullOrEmpty(messagesDetail))
                {
                    if (!string.IsNullOrEmpty(problem.Detail))
                    {
                        problem.Detail += " " + messagesDetail;
                    }
                    else
                    {
                        problem.Detail = messagesDetail;
                    }
                }
            }
        }

        private void SetProblemDetailsExtensions(
            ProblemDetails problem,
            ErrorInfo errorInfo,
            IEndpointOutcome outcome,
            HttpContext httpContext)
        {
            problem.Extensions ??= new Dictionary<string, object?>();

            if (_problemDetailsOptions.IncludeErrorCodeInExtensions &&
                !string.IsNullOrEmpty(errorInfo.Code) &&
                !problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.ErrorCode))
            {
                problem.Extensions[ProblemDetailsConstants.Extensions.ErrorCode] = errorInfo.Code;
            }

            if (!string.IsNullOrEmpty(httpContext.TraceIdentifier) &&
                !problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.TraceId))
            {
                problem.Extensions[ProblemDetailsConstants.Extensions.TraceId] = httpContext.TraceIdentifier;
            }

            if (_problemDetailsOptions.IncludeStackTrace && _isDevelopment)
            {
                if (errorInfo.Metadata != null &&
                    errorInfo.Metadata.TryGetValue(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, out var stackTrace) &&
                    stackTrace is string st &&
                    !problem.Extensions.ContainsKey(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace))
                {
                    problem.Extensions[Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace] = st;
                }
            }

            if (errorInfo.InnerErrors.Any())
            {
                var mappedInnerErrors = errorInfo.InnerErrors
                    .Select(inner => new Dictionary<string, object?>
                    {
                        [Zentient.Results.Constants.JsonConstants.ErrorInfo.Category] = inner.Category.ToString().ToUpperInvariant(),
                        [Zentient.Results.Constants.JsonConstants.ErrorInfo.Code] = inner.Code,
                        [Zentient.Results.Constants.JsonConstants.ErrorInfo.Message] = inner.Message,
                        [Zentient.Results.Constants.JsonConstants.ErrorInfo.Detail] = inner.Detail,
                    })
                    .ToList();

                if (!problem.Extensions.ContainsKey(ProblemDetailsConstants.Extensions.InnerErrors))
                {
                    problem.Extensions[ProblemDetailsConstants.Extensions.InnerErrors] = mappedInnerErrors;
                }
            }
        }
    }
}
