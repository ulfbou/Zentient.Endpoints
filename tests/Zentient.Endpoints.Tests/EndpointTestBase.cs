// <copyright file="EndpointTestBase.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using Zentient.Results;
using Zentient.Results.Constants;
using Zentient.Endpoints.Constants;
using Zentient.Endpoints.Tests.Common;
using Zentient.Endpoints.Http.Mapping;

namespace Zentient.Endpoints.Tests
{
    /// <summary>
    /// Abstract base class for Zentient.Endpoints tests, providing common setup
    /// and helper methods for creating mock/stub dependencies from Zentient.Results.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Use 'is' expression to check for null", Justification = "Legacy code style")]
    public abstract class EndpointTestBase
    {
        /// <summary>Creates a mock IResultStatus with the specified code and description.</summary>
        /// <param name="code">The status code.</param>
        /// <param name="description">The status description.</param>
        /// <returns>A mocked IResultStatus instance.</returns>
        protected static IResultStatus CreateMockResultStatus(int code, string description)
            => ResultMockHelper.CreateMockResultStatus(code, description);

        /// <summary>Creates an ErrorInfo instance with common properties.</summary>
        /// <param name="category">The error category.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="detail">Optional detail message.</param>
        /// <param name="metadata">Optional metadata dictionary.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        /// <returns>An ErrorInfo instance.</returns>
        protected static ErrorInfo CreateErrorInfo(
            ErrorCategory category = ErrorCategory.General,
            string code = ErrorCodes.General,
            string message = "Test Error",
            string? detail = null,
            IImmutableDictionary<string, object?>? metadata = null,
            IImmutableList<ErrorInfo>? innerErrors = null)
            => ResultMockHelper.CreateErrorInfo(category, code, message, detail, metadata, innerErrors);

        // --- IResult / IResult<T> Mocking Helpers ---

        /// <summary>
        /// Creates a mock IResult instance for a successful outcome.
        /// </summary>
        /// <param name="status">Optional status. 
        /// s to OK (200).</param>
        /// <param name="messages">Optional messages.</param>
        /// <returns>A mocked IResult instance representing success.</returns>
        protected static Mock<IResult> CreateMockSuccessfulResult(
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
            => ResultMockHelper.CreateMockSuccessfulResult(status, messages);

        /// <summary>
        /// Creates a mock IResult instance for a failed outcome.
        /// </summary>
        /// <param name="errors">The list of errors. Must not be null or empty.</param>
        /// <param name="status">Optional status. Defaults to Bad Request (400).</param>
        /// <param name="messages">Optional messages.</param>
        /// <returns>A mocked IResult instance representing failure.</returns>
        /// <exception cref="ArgumentException">Thrown if errors list is null or empty.</exception>
        protected static Mock<IResult> CreateMockFailedResult(
            IEnumerable<ErrorInfo> errors,
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
            => ResultMockHelper.CreateMockFailedResult(errors, status, messages);

        /// <summary>
        /// Creates a mock IResult{T} instance for a successful outcome.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The success value.</param>
        /// <param name="status">Optional status. Defaults to OK (200).</param>
        /// <param name="messages">Optional messages.</param>
        /// <returns>A mocked IResult{T} instance representing success.</returns>
        protected static Mock<IResult<T>> CreateMockSuccessfulResult<T>(
            T value,
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
            where T : notnull
            => ResultMockHelper.CreateMockSuccessfulResult<T>(value, status, messages);

        /// <summary>
        /// Creates a mock IResult{T} instance for a failed outcome.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="errors">The list of errors. Must not be null or empty.</param>
        /// <param name="status">Optional status. Defaults to Bad Request (400).</param>
        /// <param name="messages">Optional messages.</param>
        /// <returns>A mocked IResult{T} instance representing failure.</returns>
        /// <exception cref="ArgumentException">Thrown if errors list is null or empty.</exception>
        protected static Mock<IResult<T>> CreateMockFailedResult<T>(
            IEnumerable<ErrorInfo> errors,
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
            => ResultMockHelper.CreateMockFailedResult<T>(errors, status, messages);

        // --- TransportMetadata Helpers ---

        /// <summary>Creates a TransportMetadata instance with specified tags.</summary>
        /// <param name="tags">The initial tags for the metadata.</param>
        /// <returns>A new TransportMetadata instance.</returns>
        protected static TransportMetadata CreateTransportMetadata(
            IDictionary<string, object?>? tags = null)
            => tags == null ? new TransportMetadata() : TransportMetadata.From(tags);

        /// <summary>
        /// Creates a TransportMetadata instance using the advanced helper for HTTP-specific tags.
        /// </summary>
        /// <param name="httpStatusCodeHint">Optional HTTP status code hint.</param>
        /// <param name="problemDetailsOverride">Optional ProblemDetails object to override default mapping.</param>
        /// <param name="logger">Optional <see cref="ILogger"/> instance to include in metadata.</param>
        /// <param name="headers">Optional custom HTTP headers to include.</param>
        /// <param name="locationUri">Optional URI for the Location header.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance with specified tags.</returns>
        protected static TransportMetadata CreateHttpTransportMetadata(
            int? httpStatusCodeHint = null,
            Microsoft.AspNetCore.Mvc.ProblemDetails? problemDetailsOverride = null,
            ILogger? logger = null,
            ImmutableDictionary<string, string>? headers = null,
            Uri? locationUri = null)
            => TransportMetadataHelper.CreateTransportMetadata(httpStatusCodeHint, problemDetailsOverride, logger, headers, locationUri);

        /// <summary>
        /// Creates a mock ILogger instance.
        /// </summary>
        /// <returns>A mocked ILogger instance.</returns>
        protected static ILogger CreateMockLogger()
            => TransportMetadataHelper.CreateMockLogger();

        /// <summary>
        /// Gets default JsonSerializerOptions configured for Zentient.Results and Endpoints.
        /// </summary>
        /// <returns>A JsonSerializerOptions instance.</returns>
        protected static JsonSerializerOptions Options
            => OptionsHelper.JsonSerializerOptions;

        /// <summary>
        /// Serializes an object to a JSON string using configured options.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        protected static string SerializeToJson<T>(T obj)
            => SerializationHelper.SerializeToJson(obj, Options);

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type using configured options.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected static T? DeserializeFromJson<T>(string json)
            => SerializationHelper.DeserializeFromJson<T>(json, Options);

        // --- EndpointOutcome Mock Helpers ---

        /// <summary>
        /// Creates a mocked <see cref="IEndpointOutcome"/> instance based on a Zentient <see cref="Results.IResult"/>.
        /// </summary>
        /// <param name="baseResult">The underlying <see cref="Results.IResult"/> for the outcome.</param>
        /// <param name="transportMetadata">Optional <see cref="TransportMetadata"/> to associate with the outcome.</param>
        /// <returns>A mocked <see cref="IEndpointOutcome"/> object.</returns>
        protected static IEndpointOutcome CreateEndpointOutcomeMock(
            Results.IResult baseResult,
            TransportMetadata? transportMetadata = null)
            => EndpointOutcomeMockHelper.CreateEndpointOutcomeMock(baseResult, transportMetadata);

        /// <summary>
        /// Creates a mocked generic <see cref="IEndpointOutcome{TValue}"/> instance based on a Zentient <see cref="IResult{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="baseResult">The underlying <see cref="IResult{TValue}"/> for the outcome.</param>
        /// <param name="transportMetadata">Optional <see cref="TransportMetadata"/> to associate with the outcome.</param>
        /// <returns>A mocked generic <see cref="IEndpointOutcome{TValue}"/> object.</returns>
        protected static IEndpointOutcome<TValue> CreateGenericEndpointOutcomeMock<TValue>(
            IResult<TValue> baseResult,
            TransportMetadata? transportMetadata = null)
            where TValue : notnull
            => EndpointOutcomeMockHelper.CreateGenericEndpointOutcomeMock(baseResult, transportMetadata);

        /// <summary>
        /// Creates a generic <see cref="IEndpointOutcome{T}"/> from a Zentient <see cref="IResult{T}"/> and transport metadata.
        /// </summary>
        /// <typeparam name="T">The type of the outcome's value.</typeparam>
        /// <param name="zentientResult">The Zentient result to convert into an endpoint outcome.</param>
        /// <param name="transport">Transport metadata to associate with the outcome.</param>
        protected static IEndpointOutcome<T> CreateGenericEndpointOutcome<T>(IResult<T> zentientResult, TransportMetadata transport)
            where T : class
            => EndpointOutcomeMockHelper.CreateGenericEndpointOutcome(zentientResult, transport);

        // --- HttpContext Helpers ---

        /// <summary>
        /// Executes a ContentHttpResult and deserializes the response body as ProblemDetails.
        /// </summary>
        /// <param name="contentResult">The ContentHttpResult to execute.</param>
        /// <param name="httpContext">The HttpContext with a writable Response.Body stream.</param>
        /// <param name="serializerOptions">The JsonSerializerOptions to use.</param>
        /// <returns>The deserialized ProblemDetails instance.</returns>
        protected static Task<Microsoft.AspNetCore.Mvc.ProblemDetails?> DeserializeProblemDetailsFromContentResultAsync(
            Microsoft.AspNetCore.Http.HttpResults.ContentHttpResult contentResult,
            Microsoft.AspNetCore.Http.HttpContext httpContext,
            JsonSerializerOptions serializerOptions)
            => HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(contentResult, httpContext, serializerOptions);

        /// <summary>
        /// Creates a default <see cref="Microsoft.AspNetCore.Http.DefaultHttpContext"/> instance for testing purposes.
        /// </summary>
        /// <returns>A new <see cref="Microsoft.AspNetCore.Http.DefaultHttpContext"/> instance.</returns>
        protected static Microsoft.AspNetCore.Http.DefaultHttpContext CreateHttpContext()
            => HttpContextHelper.CreateHttpContext();

        /// <summary>
        /// Creates a <see cref="Microsoft.AspNetCore.Http.DefaultHttpContext"/> instance with a mocked <see cref="IEndpointOutcomeToHttpMapper"/>
        /// registered in its service provider.
        /// </summary>
        /// <param name="mapperMock">An optional pre-configured mock for <see cref="IEndpointOutcomeToHttpMapper"/>.</param>
        /// <returns>A new <see cref="Microsoft.AspNetCore.Http.DefaultHttpContext"/> instance with a service provider.</returns>
        protected static Microsoft.AspNetCore.Http.DefaultHttpContext CreateHttpContextWithMapper(
            Mock<IEndpointOutcomeToHttpMapper>? mapperMock = null)
            => HttpContextHelper.CreateHttpContextWithMapper(mapperMock);
    }
}
