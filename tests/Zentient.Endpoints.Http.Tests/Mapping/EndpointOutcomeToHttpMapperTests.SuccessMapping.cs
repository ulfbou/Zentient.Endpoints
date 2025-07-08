// <copyright file="EndpointOutcomeToHttpMapperTests.SuccessMapping.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;
using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults; // Added for JsonHttpResult and ContentHttpResult

using Zentient.Endpoints;
using Zentient.Endpoints.Constants;
using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;

using Zentient.Endpoints.Tests.Common;
using Zentient.Endpoints.Http.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Http.Tests.Mapping
{
    public partial class EndpointOutcomeToHttpMapperTests
    {
        [Fact]
        public async Task Map_Success_ReturnsNoContentStatusCodeResult_WhenOutcomeIsUnitAndStatusCodeIs204()
        {
            // Arrange
            var outcome = EndpointOutcome<Unit>.Success(
                Unit.Value,
                transportMetadata: new TransportMetadata().WithHttpStatusCodeHint(StatusCodes.Status204NoContent));
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext(_mapper.JsonSerializerOptions);

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            (result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult)!.StatusCode.Should().Be(StatusCodes.Status204NoContent);

            _mockSuccessResponseFactory.Verify(
                f => f.CreateSuccessResponse(
                    It.IsAny<IEndpointOutcome<object>>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<object>()),
                Times.Never(),
                "Generic success response factory should not be called for 204 Unit outcomes.");

            _mockSuccessResponseFactory.Verify(
                f => f.CreateSuccessResponse(
                    It.IsAny<IEndpointOutcome>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()),
                Times.Never(),
                "Non-generic success response factory should not be called for 204 Unit outcomes.");
        }

        [Fact]
        public async Task Map_Success_ReturnsNoContentStatusCodeResult_WhenOutcomeHasNoValueAndStatusCodeIs204()
        {
            // Arrange
            var outcome = EndpointOutcome.Success();
            var httpContext = new DefaultHttpContext();
            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status204NoContent;

            var expectedStatusCode = StatusCodes.Status204NoContent;

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            (result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult)!.StatusCode.Should().Be(expectedStatusCode);

            _mockSuccessResponseFactory.Verify(
                f => f.CreateSuccessResponse(
                    It.IsAny<IEndpointOutcome>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyList<string>>()),
                Times.Never(),
                "Factory should not be called for 204 non-generic outcomes with no messages");
        }

        [Fact]
        public async Task Map_Success_ReturnsNoContentResult_WhenOutcomeHasValueAndStatusCodeIs204()
        {
            // Arrange
            var data = new { Name = "Test" };
            var outcome = EndpointOutcome<object>.Success(data);
            var httpContext = new DefaultHttpContext();

            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status204NoContent;
            var expectedStatusCode = StatusCodes.Status204NoContent;

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            var statusCodeResult = result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult!.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public async Task Map_Success_ReturnsJsonResult_WhenOutcomeHasMessagesAndStatusCodeIs200OK()
        {
            // Arrange
            var messages = ImmutableList.Create("Operation successful");
            var resultWithMessages = Zentient.Results.Result.Success(messages: messages);
            var outcome = EndpointOutcome.From(resultWithMessages);
            var httpContext = new DefaultHttpContext();

            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status200OK;
            var expectedStatusCode = StatusCodes.Status200OK;
            var expectedFactoryPayload = new { Messages = messages.ToList() };
            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(),
                expectedStatusCode,
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>()))
                .Returns(expectedFactoryPayload);

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            var description = ResultStatuses.GetStatus(expectedStatusCode).Description;
            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                outcome,
                expectedStatusCode,
                description,
                messages),
                Times.Once());

            result.Should().BeOfType<JsonHttpResult<object>>();
            var jsonResult = result as JsonHttpResult<object>;
            jsonResult.Should().NotBeNull();

            // Assert the status code
            jsonResult!.StatusCode.Should().Be(expectedStatusCode);

            // Assert the body content directly from jsonResult.Value
            jsonResult.Value.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(expectedFactoryPayload);
        }

        [Fact]
        public async Task Map_Success_ReturnsNoContentResult_WhenStatusCodeIs204_RegardlessOfMessages()
        {
            // Arrange
            var messages = ImmutableList.Create("Operation successful, but no content will be sent in body for 204.");
            var resultWithMessages = Zentient.Results.Result.Success(messages: messages);
            var outcome = EndpointOutcome.From(resultWithMessages);

            var httpContext = new DefaultHttpContext();

            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status204NoContent;
            var expectedStatusCode = StatusCodes.Status204NoContent;

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert



            result.Should().BeOfType<StatusCodeHttpResult>();
            var statusCodeResult = result as StatusCodeHttpResult;
            statusCodeResult.Should().NotBeNull();

            // Assert the status code
            statusCodeResult!.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public async Task Map_Success_UsesDefaultOkStatusCode_WhenNoHintAndNotNoContent()
        {
            // Arrange
            var outcome = EndpointOutcome.Success();
            var httpContext = new DefaultHttpContext();
            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status201Created;
            _defaultOptions.SuccessResponse.DefaultNoContentStatusCode = StatusCodes.Status205ResetContent;

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), StatusCodes.Status201Created, It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Status = "Created" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<object>>();
            var jsonResult = result as JsonHttpResult<object>;
            jsonResult!.StatusCode.Should().Be(StatusCodes.Status201Created);
            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                outcome, StatusCodes.Status201Created, It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()), Times.Once());
        }

        [Fact]
        public async Task Map_Success_UsesHintStatusCode_OverridesDefault()
        {
            // Arrange
            var metadata = new TransportMetadata().WithTag(HttpMetadataKeys.HttpStatusCodeHint, StatusCodes.Status202Accepted);
            var outcome = EndpointOutcome.Success(transportMetadata: metadata);
            var httpContext = new DefaultHttpContext();
            _defaultOptions.SuccessResponse.DefaultOkStatusCode = StatusCodes.Status200OK;

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), StatusCodes.Status202Accepted, It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Status = "Accepted" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<object>>();
            var jsonResult = result as JsonHttpResult<object>;
            jsonResult!.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                outcome, StatusCodes.Status202Accepted, It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()), Times.Once());
        }

        [Fact]
        public async Task Map_Success_CallsGenericCreateSuccessResponse_ForGenericOutcomeWithConcreteValue()
        {
            // Arrange
            var data = "SomeData";
            var expectedFactoryPayload = new { Value = data, Status = "OK" };
            var outcome = EndpointOutcome<string>.Success(data);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext(_mapper.JsonSerializerOptions);

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                (IEndpointOutcome<string>)outcome,
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                data))
                .Returns(expectedFactoryPayload);

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                (IEndpointOutcome<string>)outcome,
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                data), Times.Once());

            result.Should().BeOfType<JsonHttpResult<object>>();
            var jsonResult = result as JsonHttpResult<object>;
            jsonResult!.Value.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(expectedFactoryPayload);
        }

        [Fact]
        public async Task Map_Success_CallsNonGenericCreateSuccessResponse_ForGenericOutcomeWithUnitValue()
        {
            // Arrange
            var outcome = EndpointOutcome<Unit>.Success(Unit.Value);
            var httpContext = new DefaultHttpContext();
            _defaultOptions.SuccessResponse.DefaultNoContentStatusCode = StatusCodes.Status200OK;

            var expectedFactoryPayload = new { Status = "OK" };

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>()))
                .Returns(expectedFactoryPayload);

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome<Unit>>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<Unit>()), Times.Never());

            _mockSuccessResponseFactory.Verify(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>()), Times.Once());

            result.Should().BeOfType<JsonHttpResult<object>>();
            var jsonResult = result as JsonHttpResult<object>;
            jsonResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
            jsonResult.Value.Should().NotBeNull();
            jsonResult.Value.Should().BeEquivalentTo(expectedFactoryPayload);
        }

        [Fact]
        public async Task Map_Success_ReturnsNoContentStatusCodeResult_WhenStatusCodeIs204_RegardlessOfFactoryPayload()
        {
            // Arrange
            var outcome = EndpointOutcome.Success();

            var httpContext = new DefaultHttpContext();

            var expectedStatusCode = StatusCodes.Status204NoContent;

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>();
            var statusCodeResult = result as Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult;
            statusCodeResult.Should().NotBeNull();
            statusCodeResult!.StatusCode.Should().Be(expectedStatusCode);
        }
    }
}
#pragma warning restore CS1591
