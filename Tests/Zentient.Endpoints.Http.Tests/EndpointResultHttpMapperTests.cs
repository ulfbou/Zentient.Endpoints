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

using Xunit;

using Zentient.Endpoints.Core;
using Zentient.Endpoints.Http;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>Unit tests for <see cref="EndpointResultHttpMapper"/>.</summary>
    public class EndpointResultHttpMapperTests
    {
        /// <summary>Verifies that Map throws ArgumentNullException when passed null arguments.</summary>
        [Fact]
        public void Map_ThrowsOnNullArguments()
        {
            // Arrange
            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Use the non-generic mock helper as no specific value is needed here
            IEndpointResult endpointResult = CreateEndpointResult(CreateZentientResultMock());

            // Act
            Action act1 = () => mapper.Map(null!, context);
            Action act2 = () => mapper.Map(endpointResult, null!);

            // Assert
            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        /// <summary>Verifies that a successful generic object result returns a JsonResult with the correct status code.</summary>
        [Fact]
        public void Map_Successful_GenericObjectResult_ReturnsJsonWithStatus()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.Created);

            // Use the generic success mock helper
            Results.IResult<string> baseResult = CreateZentientSuccessResultMock("foo");

            // CreateGenericEndpointResult now only needs the baseResult
            IEndpointResult<string> endpointResult = CreateGenericEndpointResult(baseResult, transport);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>>();
            Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>? json = result as Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>;
            json!.Value.Should().Be("foo");
            json.StatusCode.Should().Be((int)HttpStatusCode.Created);
        }

        /// <summary>Verifies that a successful unit result returns NoContent or the specified status code.</summary>
        [Fact]
        public void Map_Successful_UnitResult_ReturnsNoContentOrStatus()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata();

            // Use the generic success mock helper for Unit
            Results.IResult<Unit> baseResult = CreateZentientSuccessResultMock(Unit.Value);

            // CreateGenericEndpointResult now only needs the baseResult
            IEndpointResult<Unit> endpointResult = CreateGenericEndpointResult(baseResult, transport);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult? status = result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult;
            status!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        /// <summary>Verifies that a successful non-generic result returns NoContent.</summary>
        [Fact]
        public void Map_Successful_NonGenericResult_ReturnsNoContent()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata();

            // Use the non-generic mock helper for a successful result without errors
            Results.IResult baseResult = CreateZentientResultMock(true);
            IEndpointResult endpointResult = CreateEndpointResult(baseResult, transport);

            var mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            var status = result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult;
            status!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        /// <summary>Verifies that a successful result with a custom status code returns the correct status.</summary>
        [Fact]
        public void Map_Successful_CustomStatusCode_ReturnsCustomStatus()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata(299);

            // Use the generic success mock helper
            Results.IResult<string> baseResult = CreateZentientSuccessResultMock("bar");

            // CreateGenericEndpointResult now only needs the baseResult
            IEndpointResult<string> endpointResult = CreateGenericEndpointResult(baseResult, transport);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>>();
            Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>? json = result as Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<string>;
            json!.StatusCode.Should().Be(299);
        }

        /// <summary>Verifies that a failed result uses ProblemDetails from transport if present.</summary>
        [Fact]
        public void Map_Failed_UsesProblemDetailsFromTransportIfPresent()
        {
            // Arrange
            ProblemDetails pd = new ProblemDetails { Title = "FromTransport", Status = 418 };
            TransportMetadata transport = CreateTransportMetadata(418, pd);

            // Use the non-generic mock helper for failure
            Results.IResult baseResult = CreateZentientResultMock(false, new List<ErrorInfo> { new ErrorInfo(ErrorCategory.InternalServerError, "E", "fail") });
            IEndpointResult endpointResult = CreateEndpointResult(baseResult, transport);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
            Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult? problem = result as Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult;
            problem!.ProblemDetails.Title.Should().Be("FromTransport");
            problem.ProblemDetails.Status.Should().Be(418);
        }

        /// <summary>Verifies that a failed result uses the ProblemDetailsMapper if no transport ProblemDetails is present.</summary>
        [Fact]
        public void Map_Failed_UsesProblemDetailsMapperIfNoTransportProblemDetails()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.NotFound, "NF", "not found");

            Results.IResult baseResult = CreateZentientResultMock(false, new List<ErrorInfo> { error });
            IEndpointResult endpointResult = CreateEndpointResult(baseResult);

            ProblemDetails expectedProblem = new ProblemDetails { Title = "Mapped", Status = 404 };
            var pdMapper = new Mock<IProblemDetailsMapper>();
            pdMapper.Setup(m => m.Map(error, It.IsAny<HttpContext>())).Returns(expectedProblem);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(pdMapper.Object);
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
            Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult? problem = result as Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult;
            problem!.ProblemDetails.Title.Should().Be("Mapped");
            problem.ProblemDetails.Status.Should().Be(404);
        }

        /// <summary>Verifies that a failed result with no errors returns a default internal server error.</summary>
        [Fact]
        public void Map_Failed_NoErrors_ReturnsDefaultInternalServerError()
        {
            // Arrange
            Results.IResult baseResult = CreateZentientResultMock(false, new List<ErrorInfo>());
            IEndpointResult endpointResult = CreateEndpointResult(baseResult);

            ProblemDetails expectedProblem = new ProblemDetails { Title = "Default", Status = 500 };
            var pdMapper = new Mock<IProblemDetailsMapper>();
            pdMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>())).Returns(expectedProblem);

            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(pdMapper.Object);
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>();
            Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult? problem = result as Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult;
            problem!.ProblemDetails.Title.Should().Be("Default");
            problem.ProblemDetails.Status.Should().Be(500);
        }

        /// <summary>Verifies that a successful Unit result with a custom status code returns the correct status.</summary>
        [Fact]
        public void Map_Successful_Unit_WithCustomStatusCode()
        {
            // Arrange
            TransportMetadata transport = CreateTransportMetadata((int)HttpStatusCode.Accepted);
            Results.IResult<Unit> baseResult = CreateZentientSuccessResultMock(Unit.Value);
            IEndpointResult<Unit> endpointResult = CreateGenericEndpointResult(baseResult, transport);
            EndpointResultHttpMapper mapper = new EndpointResultHttpMapper(Mock.Of<IProblemDetailsMapper>());
            DefaultHttpContext context = CreateHttpContext();

            // Act
            Microsoft.AspNetCore.Http.IResult result = mapper.Map(endpointResult, context);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult? status = result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult;
            status!.StatusCode.Should().Be((int)HttpStatusCode.Accepted);
        }

        // --------------------------------------------------------------------------------
        // Helper Methods
        // --------------------------------------------------------------------------------
        private static DefaultHttpContext CreateHttpContext() => new();

        private static TransportMetadata CreateTransportMetadata(int? status = null, ProblemDetails? pd = null)
            => TransportMetadata.Default(status, pd);

        /// <summary>Creates a mock for a non-generic <see cref="Zentient.Results.IResult"/>.</summary>
        /// <param name="isSuccess">Indicates if the result is successful.</param>
        /// <param name="errors">A list of errors, if any.</param>
        /// <returns>A mocked <see cref="Zentient.Results.IResult"/> instance.</returns>
        private static Results.IResult CreateZentientResultMock(bool isSuccess = true, List<ErrorInfo>? errors = null)
        {
            var mock = new Mock<Results.IResult>();
            mock.SetupGet(r => r.IsSuccess).Returns(isSuccess);
            mock.SetupGet(r => r.Errors).Returns(errors ?? new List<ErrorInfo>());
            return mock.Object;
        }

        /// <summary>Creates a mock for a generic successful <see cref="Zentient.Results.IResult{TValue}"/>.</summary>
        /// <typeparam name="TValue">The type of the result's value.</typeparam>
        /// <param name="value">The value to be returned by the successful result.</param>
        /// <returns>A mocked <see cref="Zentient.Results.IResult{TValue}"/> instance.</returns>
        private static Results.IResult<TValue> CreateZentientSuccessResultMock<TValue>(TValue value)
            where TValue : notnull // Enforce the constraint for IResult<TValue>
        {
            var mock = new Mock<Results.IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(true);
            mock.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>()); // Success result has no errors
            mock.SetupGet(r => r.Value).Returns(value);
            return mock.Object;
        }

        private static Results.IResult<TValue> CreateZentientFailedResultMock<TValue>(List<ErrorInfo> errors)
        {
            var mock = new Mock<Results.IResult<TValue>>();
            mock.SetupGet(r => r.IsSuccess).Returns(false);
            mock.SetupGet(r => r.Errors).Returns(errors);
            return mock.Object;
        }

        /// <summary>Creates a mock for a non-generic <see cref="IEndpointResult"/>.</summary>
        /// <param name="baseResult">The underlying Zentient.Results.IResult.</param>
        /// <param name="transport">The transport metadata.</param>
        /// <returns>A mocked <see cref="IEndpointResult"/> instance.</returns>
        private static IEndpointResult CreateEndpointResult(
            Results.IResult baseResult,
            TransportMetadata? transport = null)
        {
            var mock = new Mock<IEndpointResult>();
            mock.SetupGet(e => e.BaseResult).Returns(baseResult);
            mock.SetupGet(e => e.BaseTransport).Returns(transport ?? TransportMetadata.Default());
            return mock.Object;
        }

        /// <summary>Creates a mock for a generic <see cref="IEndpointResult{TValue}"/>.</summary>
        /// <typeparam name="TValue">The type of the endpoint result's value.</typeparam>
        /// <param name="baseResult">The underlying Zentient.Results.IResult{TValue} that contains the value.</param>
        /// <param name="transport">The transport metadata.</param>
        /// <returns>A mocked <see cref="IEndpointResult{TValue}"/> instance.</returns>
        private static IEndpointResult<TValue> CreateGenericEndpointResult<TValue>(
            Results.IResult<TValue> baseResult,
            TransportMetadata? transport = null)
            where TValue : notnull
        {
            var mock = new Mock<IEndpointResult<TValue>>();
            mock.SetupGet(e => e.BaseResult).Returns(baseResult);
            mock.SetupGet(e => e.BaseTransport).Returns(transport ?? TransportMetadata.Default());
            mock.SetupGet(e => e.Result).Returns(baseResult.Value is not null ? baseResult.Value : throw new InvalidOperationException("Result.Value is null."));
            return mock.Object;
        }
    }
}
