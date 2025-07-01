// <copyright file="EndpointOutcomeHttpMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Models;
using Zentient.Endpoints.Http.Options;
using Zentient.Endpoints.Tests.Common;
using Zentient.Results;
using Zentient.Results.Constants;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public class EndpointOutcomeHttpMapperTests : IDisposable
    {
        private readonly Mock<IProblemDetailsMapper> _problemDetailsMapperMock;
        private readonly Mock<IProblemTypeUriGenerator> _problemTypeUriGeneratorMock;
        private readonly Mock<ISuccessResponseFactory> _successResponseFactoryMock;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;
        private readonly IOptions<EndpointsHttpOptions> _options;
        private readonly EndpointOutcomeToHttpMapper _mapper;
        private readonly DefaultHttpContext _httpContext;
        private readonly ILoggerFactory _loggerFactory;
        private bool disposedValue;

        public EndpointOutcomeHttpMapperTests()
        {
            _problemDetailsMapperMock = new Mock<IProblemDetailsMapper>();
            _problemTypeUriGeneratorMock = new Mock<IProblemTypeUriGenerator>();
            _successResponseFactoryMock = new Mock<ISuccessResponseFactory>();
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

            _options = Microsoft.Extensions.Options.Options.Create(new EndpointsHttpOptions());
            _webHostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Production");

            _mapper = new EndpointOutcomeToHttpMapper(
            _problemDetailsMapperMock.Object,
            _problemTypeUriGeneratorMock.Object,
            _successResponseFactoryMock.Object,
            _options,
            _webHostEnvironmentMock.Object);

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Path = new PathString("/test-path");
            _httpContext.TraceIdentifier = "test_trace_id";
            _httpContext.Response.Body = new MemoryStream();

            var services = new ServiceCollection();
            _loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ILoggerFactory>(_loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            _httpContext.RequestServices = services.BuildServiceProvider();

            _problemTypeUriGeneratorMock
                .Setup(g => g.Generate(It.IsAny<string>(), It.IsAny<HttpContext>()))
                .Returns((string? code, HttpContext ctx) =>
                    ValueTask.FromResult($"https://example.com/problems/{code?.ToUpperInvariant().Replace(' ', '-') ?? "GENERIC"}"));
        }

        [Fact]
        public async Task Map_Successful_UnitResult_ReturnsNoContentOrStatus()
        {
            // Arrange
            IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value, ResultStatuses.NoContent, new List<string>());
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.NoContent);
            IEndpointOutcome<Unit> endpointResult = CreateGenericEndpointOutcome(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<HeaderWrappedResult>();
            HeaderWrappedResult headerWrappedResult = result.As<HeaderWrappedResult>();
            headerWrappedResult.Result.Should().BeOfType<StatusCodeHttpResult>();
            (headerWrappedResult.Result as StatusCodeHttpResult)?.StatusCode.Should().Be(ResultStatuses.NoContent.Code);

            await result.ExecuteAsync(_httpContext);

            _httpContext.Response.StatusCode.Should().Be(ResultStatuses.NoContent.Code, "because the HTTP response status code should be set to 204 No Content.");
            _httpContext.Response.Body.Length.Should().Be(0, "because a 204 No Content response should have no body.");
        }

        // In tests/Zentient.Endpoints.Http.Tests/EndpointOutcomeHttpMapperTests.cs

        [Fact]
        public async Task Map_Successful_NonGenericResult_ReturnsNoContent()
        {
            // Arrange
            Zentient.Results.IResult zentientResult = CreateZentientResultMock(
                isSuccess: true,
                status: ResultStatuses.NoContent,
                messages: new List<string>());
            // Explicitly add HTTP 204 status code hint to metadata
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.NoContent);
            IEndpointOutcome endpointResult = CreateEndpointOutcome(zentientResult, transport);

            _successResponseFactoryMock
                .Setup(f => f.CreateSuccessResponse(
                    It.IsAny<IEndpointOutcome>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()))
                .Returns((object)null!);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert: Execute the result and check the HTTP response
            await result.ExecuteAsync(_httpContext);

            _httpContext.Response.StatusCode.Should().Be(ResultStatuses.NoContent.Code, "should be 204 No Content");
            _httpContext.Response.Body.Length.Should().Be(0, "204 No Content responses must not have a body");
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "CA1506:Avoid excessive class coupling", Justification = "Integration test inherently has high coupling.")]
        public async Task Map_Failed_UsesProblemDetailsMapperIfNoTransportProblemDetails()
        {
            // Arrange
            ErrorInfo testError = new ErrorInfo(ErrorCategory.NotFound, "TEST_CODE", "Test message.");
            IResult<string> zentientResult = CreateZentientFailedResultMock<string>(new List<ErrorInfo> { testError }, ResultStatuses.NotFound, new List<string>());
            IEndpointOutcome<string> endpointResult = CreateGenericEndpointOutcome(zentientResult);

            ProblemDetails mappedProblemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Test Title",
                Detail = "Test Detail",
                Instance = "/test-instance",
                Extensions = new Dictionary<string, object?> { { "code", "TEST_CODE" } }
            };

            _problemDetailsMapperMock
                .Setup(m => m.Map(It.Is<ErrorInfo>(e => e.Code == "TEST_CODE"), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(mappedProblemDetails))
                .Verifiable();

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Unwrap HeaderWrappedResult if present
            if (result is HeaderWrappedResult headerWrapped)
            {
                result = headerWrapped.Result;
            }

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            ContentHttpResult contentResult = result.As<ContentHttpResult>();
            contentResult.Should().NotBeNull();
            contentResult.ContentType.Should().Be("application/problem+json");
            contentResult.StatusCode.Should().Be(mappedProblemDetails.Status);

            ProblemDetails? deserializedProblemDetails = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(
                contentResult,
                _httpContext,
                _options.Value.JsonSerializerOptions);
            deserializedProblemDetails.Should().NotBeNull();
            deserializedProblemDetails!.Status.Should().Be(mappedProblemDetails.Status);
            deserializedProblemDetails.Title.Should().Be(mappedProblemDetails.Title);
            deserializedProblemDetails.Detail.Should().Be(mappedProblemDetails.Detail);
            deserializedProblemDetails.Extensions.Should().ContainKey("code");
            deserializedProblemDetails.Extensions["code"]!.ToString().Should().Be("TEST_CODE");
        }

        [Fact]
        public async Task Map_Failed_UsesProblemDetailsFromTransportIfPresent()
        {
            // Arrange
            ProblemDetails transportProblemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Transport Error",
                Detail = "Transport-level problem.",
                Instance = "/transport-instance",
                Extensions = new Dictionary<string, object?> { { "customKey", "customValue" } }
            };

            Results.IResult<string> zentientResult = CreateZentientFailedResultMock<string>(
            new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "TransportFailure", "Generic transport failure") },
            ResultStatuses.Unauthorized,
            new List<string>());

            TransportMetadata transport = CreateTransportMetadata(pd: transportProblemDetails);
            IEndpointOutcome<string> endpointResult = CreateGenericEndpointOutcome(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Never());

            ContentHttpResult contentResult = result.As<ContentHttpResult>();
            contentResult.Should().NotBeNull();
            contentResult.ContentType.Should().Be("application/problem+json");
            contentResult.StatusCode.Should().Be(transportProblemDetails.Status);

            ProblemDetails? deserializedProblemDetails = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(
                contentResult,
                _httpContext,
                _options.Value.JsonSerializerOptions);
            deserializedProblemDetails.Should().NotBeNull();
            deserializedProblemDetails!.Status.Should().Be(transportProblemDetails.Status);
            deserializedProblemDetails.Title.Should().Be(transportProblemDetails.Title);
            deserializedProblemDetails.Detail.Should().Be(transportProblemDetails.Detail);
            deserializedProblemDetails.Instance.Should().Be(transportProblemDetails.Instance);
            deserializedProblemDetails.Extensions.Should().ContainKey("customKey");
            deserializedProblemDetails.Extensions["customKey"]!.ToString().Should().Be("customValue");
        }

        [Fact]
        public async Task Map_Failed_NoErrors_ReturnsDefaultInternalServerError()
        {
            // Arrange
            Zentient.Results.IResult zentientResult = CreateZentientResultMock(isSuccess: false, errors: new List<ErrorInfo>(), status: ResultStatuses.Error, messages: new List<string>());
            IEndpointOutcome endpointResult = CreateEndpointOutcome(zentientResult);

            _problemTypeUriGeneratorMock
            .Setup(g => g.Generate(ErrorCodes.InternalServerError.ToString(), It.IsAny<HttpContext>()))
            .Returns((string? code, HttpContext ctx) => new ValueTask<string>("https://example.com/problems/INTERNAL-SERVER-ERROR"));

            _problemDetailsMapperMock
            .Setup(m => m.Map(It.Is<ErrorInfo>(e => e.Code == ErrorCodes.InternalServerError.ToString()), It.IsAny<HttpContext>()))
            .Returns(Task.FromResult(new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Extensions = new Dictionary<string, object?> { { "code", ErrorCodes.InternalServerError.ToString() } }
            }))
            .Verifiable();

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            ContentHttpResult contentResult = result.As<ContentHttpResult>();
            contentResult.Should().NotBeNull();
            contentResult.ContentType.Should().Be("application/problem+json");
            contentResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            ProblemDetails? deserializedProblemDetails = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(
            contentResult,
            _httpContext,
            _options.Value.JsonSerializerOptions);
            deserializedProblemDetails.Should().NotBeNull();
            deserializedProblemDetails!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
            deserializedProblemDetails.Title.Should().Be("Internal Server Error");
            deserializedProblemDetails.Detail.Should().Be("An unexpected error occurred.");
            deserializedProblemDetails.Extensions.Should().ContainKey("code");
            deserializedProblemDetails.Extensions["code"]!.ToString().Should().Be(ErrorCodes.InternalServerError.ToString());
        }

        [Fact]
        public void Map_ThrowsOnNullArguments()
        {
            // Arrange
            IEndpointOutcome? nullEndpointOutcome = null;
            HttpContext? nullHttpContext = null;

            // Act & Assert
            Func<Task> act1 = async () => await _mapper.Map(nullEndpointOutcome!, _httpContext).ConfigureAwait(false);
            act1.Should().ThrowAsync<ArgumentNullException>().WithParameterName("outcome");

            Func<Task> act2 = async () => await _mapper.Map(Mock.Of<IEndpointOutcome>(), nullHttpContext!).ConfigureAwait(false);
            act2.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task Map_Successful_Unit_WithCustomStatusCode()
        {
            // Arrange
            Zentient.Results.IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value, ResultStatuses.Accepted, new List<string>());
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.Accepted);
            IEndpointOutcome<Unit> endpointResult = CreateGenericEndpointOutcome(zentientResult, transport);

            _successResponseFactoryMock
            .Setup(f => f.CreateSuccessResponse<Unit>(
            It.IsAny<IEndpointOutcome<Unit>>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<Unit>()))
            .Returns(new SuccessResponse<Unit>(
            Unit.Value,
            null,
            (int)HttpStatusCode.Accepted,
            "Accepted",
            new List<string>()));

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<Unit>>>();
            JsonHttpResult<SuccessResponse<Unit>> jsonResult = result.As<JsonHttpResult<SuccessResponse<Unit>>>();

            jsonResult.StatusCode.Should().Be(ResultStatuses.Accepted.Code);
            jsonResult.Value.Should().NotBeNull();
            jsonResult.Value!.Data.Should().Be(Unit.Value);
            jsonResult.Value.StatusCode.Should().Be(ResultStatuses.Accepted.Code);
            jsonResult.Value.StatusDescription.Should().Be(ResultStatuses.Accepted.Description);
            jsonResult.Value.Messages.Should().BeEmpty();
            jsonResult.Value.Message.Should().BeNull();
        }

        [Fact]
        public async Task Map_Successful_GenericObjectResult_ReturnsJsonWithStatus()
        {
            // Arrange
            string testData = "Test Data";
            Results.IResult<string> zentientResult = CreateZentientSuccessResultMock(testData, ResultStatuses.Ok, new List<string>());
            IEndpointOutcome<string> endpointResult = CreateGenericEndpointOutcome(zentientResult);

            _successResponseFactoryMock
            .Setup(f => f.CreateSuccessResponse(
            It.IsAny<IEndpointOutcome<string>>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<string?>()))
            .Returns(new SuccessResponse<string>(
            testData,
            null,
            (int)HttpStatusCode.OK,
            "OK",
            new List<string>()));

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<string>>>();
            JsonHttpResult<SuccessResponse<string>> jsonResult = result.As<JsonHttpResult<SuccessResponse<string>>>();

            jsonResult.StatusCode.Should().Be(ResultStatuses.Ok.Code);
            jsonResult.Value.Should().NotBeNull();
            jsonResult.Value!.Data.Should().Be(testData);
            jsonResult.Value.StatusCode.Should().Be(ResultStatuses.Ok.Code);
            jsonResult.Value.StatusDescription.Should().Be(ResultStatuses.Ok.Description);
            jsonResult.Value.Messages.Should().BeEmpty();
            jsonResult.Value.Message.Should().BeNull();
        }

        [Fact]
        public async Task Map_Successful_CustomStatusCode_ReturnsCustomStatus()
        {
            // Arrange
            int customStatusCode = 299;
            string customStatusDescription = "Custom Status";
            string testData = "bar";

            TransportMetadata transport = CreateTransportMetadata(customStatusCode);
            Zentient.Results.IResult<string> baseResult = CreateZentientSuccessResultMock(testData, ResultStatuses.GetStatus(customStatusCode, customStatusDescription), new List<string>());
            IEndpointOutcome<string> endpointResult = CreateGenericEndpointOutcome(baseResult, transport);

            _successResponseFactoryMock
            .Setup(f => f.CreateSuccessResponse(
            It.IsAny<IEndpointOutcome<string>>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<string?>()))
            .Returns(new SuccessResponse<string>(
            testData,
            null,
            customStatusCode,
            customStatusDescription,
            new List<string>()));

            HttpContext context = CreateHttpContextForExecution();

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<string>>>();
            JsonHttpResult<SuccessResponse<string>> jsonResult = result.As<JsonHttpResult<SuccessResponse<string>>>();

            jsonResult.StatusCode.Should().Be(customStatusCode);
            jsonResult.Value.Should().NotBeNull();
            jsonResult.Value!.Data.Should().Be(testData);
            jsonResult.Value.StatusCode.Should().Be(customStatusCode);
            jsonResult.Value.StatusDescription.Should().Be(customStatusDescription);
            jsonResult.Value.Messages.Should().BeEmpty();
        }

        #region Test Helpers

        public static IResult<TValue> CreateZentientSuccessResultMock<TValue>(TValue value, IResultStatus? status = null, IReadOnlyList<string>? messages = null)
        where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            var mockResult = new Mock<IResult<TValue>>();
            mockResult.Setup(r => r.IsSuccess).Returns(true);
            mockResult.Setup(r => r.IsFailure).Returns(false);
            mockResult.Setup(r => r.Value).Returns(value);
            mockResult.Setup(r => r.Status).Returns(status ?? ResultStatuses.Ok);
            mockResult.Setup(r => r.Messages).Returns(messages?.ToImmutableList() ?? ImmutableList<string>.Empty);
            mockResult.Setup(r => r.Errors).Returns(ImmutableList<ErrorInfo>.Empty);
            return mockResult.Object;
        }

        public static Zentient.Results.IResult CreateZentientResultMock(bool isSuccess, IResultStatus? status = null, IReadOnlyList<ErrorInfo>? errors = null, IReadOnlyList<string>? messages = null)
        {
            var mockResult = new Mock<Zentient.Results.IResult>();
            mockResult.Setup(r => r.IsSuccess).Returns(isSuccess);
            mockResult.Setup(r => r.IsFailure).Returns(!isSuccess);
            mockResult.Setup(r => r.Status).Returns(status ?? (isSuccess ? ResultStatuses.Ok : ResultStatuses.Error));
            mockResult.Setup(r => r.Errors).Returns(errors?.ToImmutableList() ?? ImmutableList<ErrorInfo>.Empty);
            mockResult.Setup(r => r.Messages).Returns(messages?.ToImmutableList() ?? ImmutableList<string>.Empty);
            return mockResult.Object;
        }

        public static IResult<TValue> CreateZentientFailedResultMock<TValue>(IReadOnlyList<ErrorInfo> errors, IResultStatus? status = null, IReadOnlyList<string>? messages = null)
        where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));

            var mockResult = new Mock<IResult<TValue>>();
            mockResult.Setup(r => r.IsSuccess).Returns(false);
            mockResult.Setup(r => r.IsFailure).Returns(true);
            mockResult.Setup(r => r.Errors).Returns(errors.ToImmutableList());
            mockResult.Setup(r => r.Status).Returns(status ?? ResultStatuses.Error);
            mockResult.Setup(r => r.Messages).Returns(messages?.ToImmutableList() ?? ImmutableList<string>.Empty);
            mockResult.Setup(r => r.Value).Returns(default(TValue)!);
            return mockResult.Object;
        }

        public static IEndpointOutcome<TValue> CreateGenericEndpointOutcome<TValue>(IResult<TValue> result, TransportMetadata? metadata = null)
        where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            var mockOutcome = new Mock<IEndpointOutcome<TValue>>();
            mockOutcome.Setup(o => o.IsSuccess).Returns(result.IsSuccess);
            mockOutcome.Setup(o => o.IsFailure).Returns(result.IsFailure);
            mockOutcome.Setup(o => o.Errors).Returns(result.Errors);
            mockOutcome.Setup(o => o.Messages).Returns(result.Messages);
            mockOutcome.Setup(o => o.Status).Returns(result.Status);
            mockOutcome.Setup(o => o.Value).Returns(result.Value);
            mockOutcome.Setup(o => o.Metadata).Returns(metadata ?? TransportMetadata.From(ImmutableDictionary<string, object?>.Empty));
            return mockOutcome.Object;
        }

        public static IEndpointOutcome CreateEndpointOutcome(Zentient.Results.IResult result, TransportMetadata? metadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            var mockOutcome = new Mock<IEndpointOutcome>();
            mockOutcome.Setup(o => o.IsSuccess).Returns(result.IsSuccess);
            mockOutcome.Setup(o => o.IsFailure).Returns(result.IsFailure);
            mockOutcome.Setup(o => o.Errors).Returns(result.Errors);
            mockOutcome.Setup(o => o.Messages).Returns(result.Messages);
            mockOutcome.Setup(o => o.Status).Returns(result.Status);
            mockOutcome.Setup(o => o.Metadata).Returns(metadata ?? TransportMetadata.From(ImmutableDictionary<string, object?>.Empty));
            return mockOutcome.Object;
        }

        public static TransportMetadata CreateTransportMetadata(int? statusCode = null, ProblemDetails? pd = null, Uri? location = null, IReadOnlyDictionary<string, string>? headers = null)
        {
            ImmutableDictionary<string, object?> tags = ImmutableDictionary<string, object?>.Empty;
            if (statusCode.HasValue)
            {
                tags = tags.Add(Zentient.Endpoints.Http.Constants.HttpMetadataKeys.HttpStatusCodeHint, statusCode.Value);
            }
            if (pd != null)
            {
                tags = tags.Add(Zentient.Endpoints.Http.Constants.HttpMetadataKeys.ProblemDetailsOverride, pd);
            }
            if (location != null)
            {
                tags = tags.Add(Zentient.Endpoints.Http.Constants.HttpMetadataKeys.LocationUri, location);
            }
            if (headers != null)
            {
                tags = tags.Add(Zentient.Endpoints.Http.Constants.HttpMetadataKeys.ResponseHeaders, headers);
            }
            return TransportMetadata.From(tags);
        }

        public static HttpContext CreateHttpContextForExecution()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            return httpContext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _loggerFactory?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EndpointOutcomeHttpMapperTests()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
#pragma warning restore CS1591
