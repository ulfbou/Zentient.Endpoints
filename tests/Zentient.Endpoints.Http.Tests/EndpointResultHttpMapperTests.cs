// <copyright file="EndpointOutcomeHttpMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Http;
using Zentient.Results;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public class EndpointOutcomeHttpMapperTests
    {
        private readonly Mock<IProblemDetailsMapper> _problemDetailsMapperMock;
        private readonly EndpointOutcomeHttpMapper _mapper;
        private readonly HttpContext _httpContext;

        public EndpointOutcomeHttpMapperTests()
        {
            _problemDetailsMapperMock = new Mock<IProblemDetailsMapper>();
            _mapper = new EndpointOutcomeHttpMapper(_problemDetailsMapperMock.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpContext.SetupGet(c => c.Request).Returns(mockHttpRequest.Object);
            mockHttpRequest.SetupGet(r => r.Path).Returns(new PathString("/test-path"));
            mockHttpContext.SetupGet(c => c.TraceIdentifier).Returns("test_trace_id");
            _httpContext = mockHttpContext.Object;
        }

        [Fact]
        public async Task Map_Successful_UnitResult_ReturnsNoContentOrStatus()
        {
            // Arrange
            IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value, ResultStatuses.NoContent, new List<string>());
            IEndpointOutcome endpointResult = CreateGenericEndpointOutcome(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<StatusCodeHttpResult>();
            StatusCodeHttpResult statusCodeResult = result.As<StatusCodeHttpResult>();
            statusCodeResult.StatusCode.Should().Be(ResultStatuses.NoContent.Code);
        }

        [Fact]
        public async Task Map_Successful_NonGenericResult_ReturnsNoContent()
        {
            // Arrange
            Zentient.Results.IResult zentientResult = CreateZentientResultMock(isSuccess: true, status: ResultStatuses.NoContent, messages: new List<string>());
            IEndpointOutcome endpointResult = CreateEndpointOutcome(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<StatusCodeHttpResult>();
            StatusCodeHttpResult statusCodeResult = result.As<StatusCodeHttpResult>();
            statusCodeResult.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "CA1506:Avoid excessive class coupling", Justification = "Integration test inherently has high coupling.")]
        public async Task Map_Failed_UsesProblemDetailsMapperIfNoTransportProblemDetails()
        {
            // Arrange
            ErrorInfo testError = new ErrorInfo(ErrorCategory.NotFound, "TEST_CODE", "Test message.");
            Results.IResult<string> zentientResult = CreateZentientFailedResultMock<string>(new List<ErrorInfo> { testError }, ResultStatuses.NotFound, new List<string>());
            IEndpointOutcome endpointResult = CreateGenericEndpointOutcome(zentientResult);

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

            // Assert
            result.Should().BeOfType<JsonHttpResult<Microsoft.AspNetCore.Mvc.ProblemDetails>>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            var jsonResult = result as JsonHttpResult<ProblemDetails>;
            jsonResult.Should().NotBeNull();
            jsonResult!.Value.Should().NotBeNull();
            jsonResult.Value.Status.Should().Be(mappedProblemDetails.Status);
            jsonResult.Value.Title.Should().Be(mappedProblemDetails.Title);
            jsonResult.Value.Detail.Should().Be(mappedProblemDetails.Detail);
            jsonResult.Value.Extensions.Should().ContainKey("code");
            jsonResult.Value.Extensions["code"].Should().Be("TEST_CODE");
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
                Instance = "/transport-instance"
            };

            Results.IResult<string> zentientResult = CreateZentientFailedResultMock<string>(
                new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "TransportFailure", "Generic transport failure") },
                ResultStatuses.Unauthorized,
                new List<string>());

            TransportMetadata transport = CreateTransportMetadata(pd: transportProblemDetails);
            IEndpointOutcome endpointResult = CreateGenericEndpointOutcome(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<Microsoft.AspNetCore.Mvc.ProblemDetails>>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Never());

            var jsonResult = result as JsonHttpResult<ProblemDetails>;
            jsonResult.Should().NotBeNull();
            jsonResult!.Value.Should().NotBeNull();
            jsonResult.Value.Status.Should().Be(transportProblemDetails.Status);
            jsonResult.Value.Title.Should().Be(transportProblemDetails.Title);
            jsonResult.Value.Detail.Should().Be(transportProblemDetails.Detail);
        }

        [Fact]
        public async Task Map_Failed_NoErrors_ReturnsDefaultInternalServerError()
        {
            // Arrange
            Results.IResult zentientResult = CreateZentientResultMock(isSuccess: false, errors: new List<ErrorInfo>(), status: ResultStatuses.Error, messages: new List<string>());
            IEndpointOutcome endpointResult = CreateEndpointOutcome(zentientResult);

            _problemDetailsMapperMock
                .Setup(m => m.Map(It.Is<ErrorInfo>(e => e.Code == "InternalError"), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Extensions = new Dictionary<string, object?> { { "code", "InternalError" } }
                }))
                .Verifiable();

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<Microsoft.AspNetCore.Mvc.ProblemDetails>>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            var jsonResult = result as JsonHttpResult<ProblemDetails>;
            jsonResult.Should().NotBeNull();
            jsonResult!.Value.Should().NotBeNull();
            jsonResult.Value.Status.Should().Be((int)HttpStatusCode.InternalServerError);
            jsonResult.Value.Title.Should().Be("Internal Server Error");
            jsonResult.Value.Detail.Should().Be("An unexpected error occurred.");
            jsonResult.Value.Extensions.Should().ContainKey("code");
            jsonResult.Value.Extensions["code"].Should().Be("InternalError");
        }

        [Fact]
        public void Map_ThrowsOnNullArguments()
        {
            // Arrange
            IEndpointOutcome? nullEndpointOutcome = null;
            HttpContext? nullHttpContext = null;

            // Act & Assert
            Func<Task> act1 = async () => await _mapper.Map(nullEndpointOutcome!, _httpContext).ConfigureAwait(false);
            act1.Should().ThrowAsync<ArgumentNullException>().WithParameterName("endpointResult");

            Func<Task> act2 = async () => await _mapper.Map(Mock.Of<IEndpointOutcome>(), nullHttpContext!).ConfigureAwait(false);
            act2.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task Map_Successful_Unit_WithCustomStatusCode()
        {
            // Arrange
            Results.IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value, ResultStatuses.Accepted, new List<string>());
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.Accepted);
            IEndpointOutcome endpointResult = CreateGenericEndpointOutcome(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<object?>>>();
            JsonHttpResult<SuccessResponse<object?>> jsonResult = result.As<JsonHttpResult<SuccessResponse<object?>>>();
            DefaultHttpContext tempHttpContextForExecution = CreateHttpContextForExecution();

            // Manually serialize the Value to the response stream using System.Text.Json
            tempHttpContextForExecution.Response.StatusCode = jsonResult.StatusCode ?? 200;
            tempHttpContextForExecution.Response.ContentType = jsonResult.ContentType ?? "application/json";
            await System.Text.Json.JsonSerializer.SerializeAsync(
                tempHttpContextForExecution.Response.Body,
                jsonResult.Value,
                jsonResult.Value?.GetType() ?? typeof(object),
                options: null,
                cancellationToken: default
            );
            tempHttpContextForExecution.Response.Body.Seek(0, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(tempHttpContextForExecution.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                SuccessResponse<object>? deserializedResponse = JsonSerializer.Deserialize<SuccessResponse<object>>(responseBody);
                deserializedResponse.Should().NotBeNull();
                deserializedResponse!.Data.Should().BeNull();
                deserializedResponse.StatusCode.Should().Be(ResultStatuses.Accepted.Code);
                deserializedResponse.StatusDescription.Should().Be(ResultStatuses.Accepted.Description);
                deserializedResponse.Messages.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Map_Successful_GenericObjectResult_ReturnsJsonWithStatus()
        {
            // Arrange
            string testData = "Test Data";
            Results.IResult<string> zentientResult = CreateZentientSuccessResultMock(testData, ResultStatuses.Success, new List<string>());
            IEndpointOutcome endpointResult = CreateGenericEndpointOutcome(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = await _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<object?>>>();
            JsonHttpResult<SuccessResponse<object?>> jsonResult = result.As<JsonHttpResult<SuccessResponse<object?>>>();
            DefaultHttpContext tempHttpContextForExecution = CreateHttpContextForExecution();

            // Serialize the Value to the response stream using System.Text.Json
            tempHttpContextForExecution.Response.StatusCode = jsonResult.StatusCode ?? 200;
            tempHttpContextForExecution.Response.ContentType = jsonResult.ContentType ?? "application/json";
            await System.Text.Json.JsonSerializer.SerializeAsync(
                tempHttpContextForExecution.Response.Body,
                jsonResult.Value,
                jsonResult.Value?.GetType() ?? typeof(object),
                options: null,
                cancellationToken: default
            );
            tempHttpContextForExecution.Response.Body.Seek(0, SeekOrigin.Begin);

            using (StreamReader reader = new StreamReader(tempHttpContextForExecution.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                SuccessResponse<string>? deserializedResponse = JsonSerializer.Deserialize<SuccessResponse<string>>(responseBody);

                deserializedResponse.Should().NotBeNull();
                deserializedResponse!.Data.Should().Be(testData);
                deserializedResponse.StatusCode.Should().Be(ResultStatuses.Success.Code);
                deserializedResponse.StatusDescription.Should().Be(ResultStatuses.Success.Description);
                deserializedResponse.Messages.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task Map_Successful_CustomStatusCode_ReturnsCustomStatus()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata(299);
            Zentient.Results.IResult<string> baseResult = CreateZentientSuccessResultMock("bar", ResultStatuses.GetStatus(299, "Custom Status"), new List<string>());
            IEndpointOutcome<string> endpointResult = CreateGenericEndpointOutcome(baseResult, transport);
            EndpointOutcomeHttpMapper mapper = new EndpointOutcomeHttpMapper(Mock.Of<IProblemDetailsMapper>());
            HttpContext context = CreateHttpContextForExecution();

            // Act
            Microsoft.AspNetCore.Http.IResult result = await mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<JsonHttpResult<SuccessResponse<object?>>>();
            JsonHttpResult<SuccessResponse<object?>> jsonResult = result.As<JsonHttpResult<SuccessResponse<object?>>>();
            DefaultHttpContext tempHttpContextForExecution = CreateHttpContextForExecution();

            // Serialize the Value to the response stream using System.Text.Json
            tempHttpContextForExecution.Response.StatusCode = jsonResult.StatusCode ?? 200;
            tempHttpContextForExecution.Response.ContentType = jsonResult.ContentType ?? "application/json";
            await System.Text.Json.JsonSerializer.SerializeAsync(
                tempHttpContextForExecution.Response.Body,
                jsonResult.Value,
                jsonResult.Value?.GetType() ?? typeof(object),
                options: null,
                cancellationToken: default
            );
            tempHttpContextForExecution.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(tempHttpContextForExecution.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                SuccessResponse<string>? deserializedResponse = JsonSerializer.Deserialize<SuccessResponse<string>>(responseBody);

                deserializedResponse.Should().NotBeNull();
                deserializedResponse!.Data.Should().Be("bar");
                deserializedResponse.StatusCode.Should().Be(299);
                deserializedResponse.StatusDescription.Should().Be("Custom Status");
                deserializedResponse.Messages.Should().BeEmpty();
            }

            jsonResult.StatusCode.Should().Be(299);
        }

        private static DefaultHttpContext CreateHttpContextForExecution()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/mocked-path";
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static TransportMetadata CreateTransportMetadata(int? status = null, ProblemDetails? pd = null)
            => TransportMetadata.Default(status, pd);

        private static Results.IResult CreateZentientResultMock(
            bool isSuccess = true,
            List<ErrorInfo>? errors = null,
            IResultStatus? status = null,
            List<string>? messages = null)
        {
            Mock<Results.IResult> mock = new Mock<Results.IResult>();
            mock.SetupGet(r => r.IsSuccess).Returns(isSuccess);
            mock.SetupGet(r => r.Errors).Returns(errors ?? new List<ErrorInfo>());

            mock.SetupGet(r => r.Status).Returns(status ?? (isSuccess ? ResultStatuses.Success : ResultStatuses.Error));
            mock.SetupGet(r => r.Messages).Returns(messages ?? Array.Empty<string>().ToList());

            return mock.Object;
        }

        private static IResult<TValue> CreateZentientSuccessResultMock<TValue>(
            TValue value,
            IResultStatus? status = null,
            List<string>? messages = null)
            where TValue : notnull
        {
            Mock<IResult<TValue>> mock = new Mock<IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(true);
            mock.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>());
            mock.SetupGet(r => r.Value).Returns(value);
            mock.SetupGet(r => r.Status).Returns(status ?? ResultStatuses.Success);
            mock.SetupGet(r => r.Messages).Returns(messages ?? Array.Empty<string>().ToList());

            return mock.Object;
        }

        private static IResult<TValue> CreateZentientFailedResultMock<TValue>(
            List<ErrorInfo> errors,
            IResultStatus? status = null,
            List<string>? messages = null)
        {
            Mock<IResult<TValue>> mock = new Mock<IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(false);
            mock.SetupGet(r => r.Errors).Returns(errors);
            mock.SetupGet(r => r.Status).Returns(status ?? ResultStatuses.Error);
            mock.SetupGet(r => r.Messages).Returns(messages ?? Array.Empty<string>().ToList());

            return mock.Object;
        }

        private static IEndpointOutcome CreateEndpointOutcome(
            Zentient.Results.IResult baseResult,
            TransportMetadata? transport = null)
        {
            Mock<IEndpointOutcome> mock = new Mock<IEndpointOutcome>();
            mock.SetupGet(e => e.IsSuccess).Returns(baseResult.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(baseResult.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(baseResult.Errors);
            mock.SetupGet(e => e.Messages).Returns(baseResult.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(baseResult.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(baseResult.Status);
            mock.SetupGet(e => e.TransportMetadata).Returns(transport ?? new TransportMetadata());
            return mock.Object;
        }

        private static IEndpointOutcome<TValue> CreateGenericEndpointOutcome<TValue>(
            Zentient.Results.IResult<TValue> baseResult,
            TransportMetadata? transport = null)
            where TValue : notnull
        {
            Mock<IEndpointOutcome<TValue>> mock = new Mock<IEndpointOutcome<TValue>>();
            mock.SetupGet(e => e.IsSuccess).Returns(baseResult.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(baseResult.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(baseResult.Errors);
            mock.SetupGet(e => e.Messages).Returns(baseResult.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(baseResult.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(baseResult.Status);
            mock.SetupGet(e => e.TransportMetadata).Returns(transport ?? new TransportMetadata());

            if (baseResult.IsSuccess)
            {
                mock.SetupGet(e => e.Value).Returns(baseResult.Value!);
            }
            else
            {
                mock.SetupGet(e => e.Value).Throws<InvalidOperationException>();
            }

            return mock.Object;
        }
    }
}
#pragma warning restore CS1591
