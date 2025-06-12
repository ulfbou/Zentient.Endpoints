// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointResultHttpMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Newtonsoft.Json;

using Xunit;

using Zentient.Endpoints.Core;
using Zentient.Endpoints.Http;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>Unit tests for <see cref="EndpointResultHttpMapper"/>.</summary>
    public class EndpointResultHttpMapperTests
    {
        private readonly Mock<IProblemDetailsMapper> _problemDetailsMapperMock;
        private readonly EndpointResultHttpMapper _mapper;
        private readonly DefaultHttpContext _httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointResultHttpMapperTests"/> class.
        /// Sets up the required mocks and test context for unit testing <see cref="EndpointResultHttpMapper"/>.
        /// </summary>
        public EndpointResultHttpMapperTests()
        {
            _problemDetailsMapperMock = new Mock<IProblemDetailsMapper>();
            _mapper = new EndpointResultHttpMapper(_problemDetailsMapperMock.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.TraceIdentifier = "test_trace_id";
        }

        /// <summary>
        /// Tests that mapping a successful <see cref="EndpointResult{T}"/> with <see cref="Unit"/> value
        /// and default status returns an appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/> for "No Content".
        /// </summary>
        [Fact]
        public Task Map_Successful_UnitResult_ReturnsNoContentOrStatus()
        {
            // Arrange
            IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value);
            IEndpointResult endpointResult = CreateGenericEndpointResult(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<EmptyResultWithStatusCode>()
                  .Which.StatusCode.Should().Be(ResultStatuses.NoContent.Code);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Tests that mapping a successful <see cref="EndpointResult{T}"/> with <see cref="Unit"/> value
        /// and a custom HTTP status code returns an appropriate <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        [Fact]
        public Task Map_Successful_Unit_WithCustomStatusCode()
        {
            // Arrange
            Results.IResult<Unit> zentientResult = CreateZentientSuccessResultMock(Unit.Value);
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.Accepted);
            IEndpointResult endpointResult = CreateGenericEndpointResult(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<EmptyResultWithStatusCode>()
                  .Which.StatusCode.Should().Be(ResultStatuses.Accepted.Code);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Tests that mapping a successful <see cref="EndpointResult{TResult}"/> with a generic object value
        /// returns a <see cref="NewtonsoftJsonResult"/> with the correct JSON content and status.
        /// </summary>
        [Fact]
        public async Task Map_Successful_GenericObjectResult_ReturnsJsonWithStatus()
        {
            // Arrange
            string testData = "Test Data";
            Results.IResult<string> zentientResult = CreateZentientSuccessResultMock(testData);
            IEndpointResult endpointResult = CreateGenericEndpointResult(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<NewtonsoftJsonResult>();

            NewtonsoftJsonResult newtonsoftResult = (NewtonsoftJsonResult)result;
            DefaultHttpContext tempHttpContext = CreateHttpContext();
            tempHttpContext.Response.Body = new MemoryStream();

            await newtonsoftResult.ExecuteAsync(tempHttpContext);

            tempHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(tempHttpContext.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                responseBody.Should().Be("\"Test Data\"");
            }
            newtonsoftResult.StatusCode.Should().Be(ResultStatuses.Success.Code);
        }

        /// <summary>
        /// Tests that mapping a non-generic successful <see cref="EndpointResult{TResult}"/> returns an appropriate
        /// <see cref="Microsoft.AspNetCore.Http.IResult"/> for "No Content".
        /// </summary>
        [Fact]
        public Task Map_Successful_NonGenericResult_ReturnsNoContent()
        {
            // Arrange
            Zentient.Results.IResult zentientResult = CreateZentientResultMock(isSuccess: true);
            IEndpointResult endpointResult = CreateEndpointResult(zentientResult);

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<EmptyResultWithStatusCode>()
                  .Which.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Tests that mapping a failed <see cref="EndpointResult{TResult}"/> without transport-level problem details
        /// uses the <see cref="IProblemDetailsMapper"/> to generate a <see cref="ProblemDetails"/> response.
        /// </summary>
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "CA1506:Avoid excessive class coupling", Justification = "Integration test inherently has high coupling.")]
        public async Task Map_Failed_UsesProblemDetailsMapperIfNoTransportProblemDetails()
        {
            // Arrange
            ErrorInfo testError = new ErrorInfo(ErrorCategory.NotFound, "TEST_CODE", "Test message.");
            Results.IResult<string> zentientResult = CreateZentientFailedResultMock<string>(new List<ErrorInfo> { testError });
            IEndpointResult endpointResult = CreateGenericEndpointResult(zentientResult);

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
                .Returns(mappedProblemDetails)
                .Verifiable();

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<NewtonsoftJsonResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            NewtonsoftJsonResult newtonsoftResult = (NewtonsoftJsonResult)result;
            DefaultHttpContext tempHttpContext = CreateHttpContext();
            tempHttpContext.Response.Body = new MemoryStream();
            await newtonsoftResult.ExecuteAsync(tempHttpContext);
            tempHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(tempHttpContext.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                ProblemDetails? deserializedProblem = JsonConvert.DeserializeObject<ProblemDetails>(responseBody);
                deserializedProblem.Should().NotBeNull();
                deserializedProblem!.Status.Should().Be(mappedProblemDetails.Status);
                deserializedProblem.Title.Should().Be(mappedProblemDetails.Title);
                deserializedProblem.Detail.Should().Be(mappedProblemDetails.Detail);
                deserializedProblem.Extensions.Should().ContainKey("code");
                deserializedProblem.Extensions["code"].Should().Be("TEST_CODE");
            }
        }

        /// <summary>
        /// Tests that mapping a failed <see cref="EndpointResult{TResult}"/> with existing transport-level problem details
        /// uses those provided problem details directly, without invoking the <see cref="IProblemDetailsMapper"/>.
        /// </summary>
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

            Results.IResult<string> zentientResult = CreateZentientFailedResultMock<string>(new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "TransportFailure", "Generic transport failure") });
            TransportMetadata transport = CreateTransportMetadata(pd: transportProblemDetails);
            IEndpointResult endpointResult = CreateGenericEndpointResult(zentientResult, transport);

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<NewtonsoftJsonResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Never());

            NewtonsoftJsonResult newtonsoftResult = (NewtonsoftJsonResult)result;
            DefaultHttpContext tempHttpContext = CreateHttpContext();
            tempHttpContext.Response.Body = new MemoryStream();
            await newtonsoftResult.ExecuteAsync(tempHttpContext);
            tempHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(tempHttpContext.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                ProblemDetails? deserializedProblem = JsonConvert.DeserializeObject<ProblemDetails>(responseBody);
                deserializedProblem.Should().NotBeNull();
                deserializedProblem!.Status.Should().Be(transportProblemDetails.Status);
                deserializedProblem.Title.Should().Be(transportProblemDetails.Title);
                deserializedProblem.Detail.Should().Be(transportProblemDetails.Detail);
            }
        }

        /// <summary>
        /// Tests that mapping a failed <see cref="Zentient.Endpoints.Core.IEndpointResult"/> with no explicit errors in its base result
        /// falls back to a default internal server error and uses the <see cref="IProblemDetailsMapper"/>.
        /// </summary>
        [Fact]
        public async Task Map_Failed_NoErrors_ReturnsDefaultInternalServerError()
        {
            // Arrange
            Results.IResult zentientResult = CreateZentientResultMock(isSuccess: false, errors: new List<ErrorInfo>());
            IEndpointResult endpointResult = CreateEndpointResult(zentientResult);

            _problemDetailsMapperMock
                .Setup(m => m.Map(It.Is<ErrorInfo>(e => e.Code == "InternalError"), It.IsAny<HttpContext>()))
                .Returns(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Extensions = new Dictionary<string, object?> { { "code", "InternalError" } }
                })
                .Verifiable();

            // Act
            Microsoft.AspNetCore.Http.IResult result = _mapper.Map(endpointResult, _httpContext);

            // Assert
            result.Should().BeOfType<NewtonsoftJsonResult>();
            _problemDetailsMapperMock.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Once());

            NewtonsoftJsonResult newtonsoftResult = (NewtonsoftJsonResult)result;
            DefaultHttpContext tempHttpContext = CreateHttpContext();
            tempHttpContext.Response.Body = new MemoryStream();
            await newtonsoftResult.ExecuteAsync(tempHttpContext);
            tempHttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(tempHttpContext.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                ProblemDetails? deserializedProblem = JsonConvert.DeserializeObject<ProblemDetails>(responseBody);
                deserializedProblem.Should().NotBeNull();
                deserializedProblem!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
                deserializedProblem.Title.Should().Be("Internal Server Error");
                deserializedProblem.Detail.Should().Be("An unexpected error occurred.");
                deserializedProblem.Extensions.Should().ContainKey("code");
                deserializedProblem.Extensions["code"].Should().Be("InternalError");
            }
        }

        /// <summary>
        /// Tests that the <see cref="EndpointResultHttpMapper.Map"/> method throws <see cref="ArgumentNullException"/>
        /// when provided with null arguments.
        /// </summary>
        [Fact]
        public void Map_ThrowsOnNullArguments()
        {
            // Arrange
            IEndpointResult? nullEndpointResult = null;
            HttpContext? nullHttpContext = null;

            // Act & Assert
            Action act1 = () => _mapper.Map(nullEndpointResult!, _httpContext);
            act1.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");

            Action act2 = () => _mapper.Map(Mock.Of<IEndpointResult>(), nullHttpContext!);
            act2.Should().Throw<ArgumentNullException>().WithParameterName("httpContext");
        }

        /// <summary>
        /// Verifies that a successful result with a custom status code returns the correct status.
        /// </summary>
        [Fact]
        public async Task Map_Successful_CustomStatusCode_ReturnsCustomStatus()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata(299);
            Zentient.Results.IResult<string> baseResult = CreateZentientSuccessResultMock("bar");
            IEndpointResult<string> endpointResult = CreateGenericEndpointResult(baseResult, transport);
            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<NewtonsoftJsonResult>();
            NewtonsoftJsonResult newtonsoftResult = (NewtonsoftJsonResult)result;
            DefaultHttpContext tempHttpContext = CreateHttpContext();
            tempHttpContext.Response.Body = new System.IO.MemoryStream();

            await newtonsoftResult.ExecuteAsync(tempHttpContext);
            tempHttpContext.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);

            using (System.IO.StreamReader reader = new System.IO.StreamReader(tempHttpContext.Response.Body))
            {
                string responseBody = await reader.ReadToEndAsync();
                responseBody.Should().Be("\"bar\"");
            }

            newtonsoftResult.StatusCode.Should().Be(299);
        }

        // --------------------------------------------------------------------------------
        // Helper Methods
        // --------------------------------------------------------------------------------
        /// <summary>Creates a default HTTP context.</summary>
        /// <returns>A new <see cref="DefaultHttpContext"/> instance.</returns>
        private static DefaultHttpContext CreateHttpContext() => new();

        /// <summary>Creates transport metadata with optional HTTP status code and problem details.</summary>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="pd">The problem details.</param>
        /// <returns>A new <see cref="TransportMetadata"/> instance.</returns>
        private static TransportMetadata CreateTransportMetadata(int? status = null, ProblemDetails? pd = null)
            => TransportMetadata.Default(status, pd);

        /// <summary>Creates a mock for a non-generic <see cref="Results.IResult"/>.</summary>
        /// <param name="isSuccess">Indicates if the result is successful.</param>
        /// <param name="errors">A list of errors, if any.</param>
        /// <returns>A mocked <see cref="Results.IResult"/> instance.</returns>
        private static Results.IResult CreateZentientResultMock(bool isSuccess = true, List<ErrorInfo>? errors = null)
        {
            Mock<Results.IResult> mock = new Mock<Results.IResult>();
            mock.SetupGet(r => r.IsSuccess).Returns(isSuccess);
            mock.SetupGet(r => r.Errors).Returns(errors ?? new List<ErrorInfo>());
            return mock.Object;
        }

        /// <summary>Creates a mock for a generic successful <see cref="IResult{TValue}"/>.</summary>
        /// <typeparam name="TValue">The type of the result's value.</typeparam>
        /// <param name="value">The value to be returned by the successful result.</param>
        /// <returns>A mocked <see cref="IResult{TValue}"/> instance.</returns>
        private static IResult<TValue> CreateZentientSuccessResultMock<TValue>(TValue value)
            where TValue : notnull
        {
            Mock<IResult<TValue>> mock = new Mock<IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(true);
            mock.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>());
            mock.SetupGet(r => r.Value).Returns(value);
            return mock.Object;
        }

        /// <summary>Creates a mock for a generic failed <see cref="IResult{TValue}"/>.</summary>
        /// <typeparam name="TValue">The type of the result's value.</typeparam>
        /// <param name="errors">A list of errors.</param>
        /// <returns>A mocked <see cref="IResult{TValue}"/> instance.</returns>
        private static IResult<TValue> CreateZentientFailedResultMock<TValue>(List<ErrorInfo> errors)
        {
            Mock<IResult<TValue>> mock = new Mock<IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(false);
            mock.SetupGet(r => r.Errors).Returns(errors);
            return mock.Object;
        }

        /// <summary>Creates a mock for a non-generic <see cref="IEndpointResult"/>.</summary>
        /// <param name="baseResult">The underlying <see cref="Results.IResult"/>.</param>
        /// <param name="transport">The transport metadata.</param>
        /// <returns>A mocked <see cref="IEndpointResult"/> instance.</returns>
        private static IEndpointResult CreateEndpointResult(
            Results.IResult baseResult,
            TransportMetadata? transport = null)
        {
            Mock<IEndpointResult> mock = new Mock<IEndpointResult>();
            mock.SetupGet(e => e.BaseResult).Returns(baseResult);
            mock.SetupGet(e => e.BaseTransport).Returns(transport ?? TransportMetadata.Default());
            return mock.Object;
        }

        /// <summary>Creates a mock for a generic <see cref="IEndpointResult{TValue}"/>.</summary>
        /// <typeparam name="TValue">The type of the endpoint result's value.</typeparam>
        /// <param name="baseResult">The underlying <see cref="Zentient.Results.IResult{TValue}"/> that contains the value.</param>
        /// <param name="transport">The transport metadata.</param>
        /// <returns>A mocked <see cref="IEndpointResult{TValue}"/> instance.</returns>
        private static IEndpointResult<TValue> CreateGenericEndpointResult<TValue>(
            Zentient.Results.IResult<TValue> baseResult,
            TransportMetadata? transport = null)
            where TValue : notnull
        {
            Mock<IEndpointResult<TValue>> mock = new Mock<IEndpointResult<TValue>>();
            mock.SetupGet(e => e.BaseResult).Returns(baseResult);
            mock.SetupGet(e => e.BaseTransport).Returns(transport ?? TransportMetadata.Default());

            if (baseResult.IsSuccess)
            {
                mock.SetupGet(e => e.Result).Returns(baseResult.Value!);
            }
            else
            {
                mock.SetupGet(e => e.Result).Throws<InvalidOperationException>();
            }

            return mock.Object;
        }
    }
}
