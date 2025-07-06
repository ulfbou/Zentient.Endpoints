// <copyright file="EndpointOutcomeGenericTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Builders;
using Zentient.Endpoints.Constants;
using Zentient.Endpoints.Tests.Common;

using Zentient.Results;
using Zentient.Results.Constants;

#pragma warning disable CS1591 // Disable XML comment warnings for test class

namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeGenericTests : EndpointTestBase
    {
        private const string ErrorCode = "ERR";
        private const string ErrorMessage = "Test error";
        private const string SuccessMessage = "Success!";
        private static readonly ErrorInfo _testError = TestErrorInfoFactory.Custom(
            ErrorCategory.General, ErrorCode, ErrorMessage);

        private static readonly string[] _successMessages = new[] { SuccessMessage };

        [Fact]
        public void Constructor_Throws_IfResultIsNull()
        {
            // Arrange & Act
            Action act = () => _ = new EndpointOutcome<string>(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Constructor_SetsProperties_ForSuccess()
        {
            const string OkValue = "OK";

            // Arrange
            string value = "abc";

            Mock<IResult<string>> mockResult = ResultMockHelper.CreateMockSuccessfulResult(
                value,
                ResultMockHelper.CreateMockResultStatus(200, OkValue),
                _successMessages);

            TransportMetadata metadata = new TransportMetadataBuilder()
                .WithTag("k", "1")
                .Build();

            // Act
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object, metadata);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.IsFailure.Should().BeFalse();
            outcome.Value.Should().Be(value);
            outcome.Errors.Should().BeEmpty();
            outcome.Messages.Should().Contain(_successMessages[0]);
            outcome.Status.Should().Be(mockResult.Object.Status);
            outcome.Metadata.Should().Be(metadata);
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Constructor_SetsProperties_ForFailure()
        {
            // Arrange
            Mock<IResult<string>> mockResult = ResultMockHelper.CreateMockFailedResult<string>(
                new[] { _testError },
                ResultMockHelper.CreateMockResultStatus(ResultStatuses.BadRequest.Code, ResultStatuses.BadRequest.Description));
            TransportMetadata metadata = TransportMetadata.From(
                new Dictionary<string, object?>
                {
                    { "key1", "value1" },
                    { "key2", 42 }
                });

            // Act
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object, metadata);

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.IsFailure.Should().BeTrue();
            outcome.Value.Should().Be(default(string));
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Status.Should().Be(mockResult.Object.Status);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void Value_ReturnsValue_OnSuccess()
        {
            // Arrange
            string value = "val";
            Mock<IResult<string>> mockResult = ResultMockHelper.CreateMockSuccessfulResult(value);

            // Act
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            // Assert
            outcome.Value.Should().Be(value);
        }

        [Fact]
        public void Value_ReturnsDefault_OnFailure()
        {
            // Arrange
            Mock<IResult<int>> mockResult = ResultMockHelper.CreateMockFailedResult<int>(new[] { _testError });

            // Act
            EndpointOutcome<int> outcome = new EndpointOutcome<int>(mockResult.Object);

            // Assert
            outcome.Value.Should().Be(default(int));
        }

        [Fact]
        public void Value_ReturnsNull_OnSuccessWithNull()
        {
            // Arrange
            Mock<IResult<string>> mockResult = ResultMockHelper.CreateMockSuccessfulResult<string>(null!);

            // Act
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            // Assert
            outcome.Value.Should().BeNull();
        }

        [Fact]
        public void Success_ReturnsSuccessfulOutcome_WithValueAndMetadata()
        {
            // Arrange
            int value = 42;
            TransportMetadata metadata = new TransportMetadataBuilder()
                .WithTag("key", "value")
                .Build();

            // Act
            IEndpointOutcome<int> outcome = EndpointOutcomeBuilder.IsSuccess(value)
                .WithMetadata("foo", "bar")
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
            outcome.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        }

        [Fact]
        public void Success_WithStatus_ReturnsSuccessfulOutcome()
        {
            // Arrange
            int value = 99;
            IResultStatus status = ResultMockHelper.CreateMockResultStatus(201, "Created");

            // Act
            IEndpointOutcome<int> outcome = EndpointOutcomeBuilder.IsSuccess(value)
                .WithStatus(status)
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
            outcome.Status.Should().Be(status);
        }

        [Fact]
        public void NoContent_ReturnsOutcome_WithDefaultValue()
        {
            // Arrange & Act
            IEndpointOutcome<Guid> outcome = EndpointOutcomeBuilder.IsSuccess(default(Guid))
                .WithStatus(ResultStatuses.NoContent)
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(default(Guid));
            outcome.Status.Code.Should().Be(ResultStatuses.NoContent.Code);
        }

        [Fact]
        public void NoContent_ReturnsOutcome_ForUnit()
        {
            // Arrange & Act
            IEndpointOutcome<Unit> outcome = EndpointOutcomeBuilder.IsSuccess(Unit.Value)
                .WithStatus(ResultStatuses.NoContent)
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(Unit.Value);
            outcome.Status.Code.Should().Be(ResultStatuses.NoContent.Code);
        }

        [Fact]
        public void FromError_Throws_IfErrorIsNull()
        {
            // Arrange & Act
            Action act = () => EndpointOutcome<string>.FromError(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("error");
        }

        [Fact]
        public void FromError_ReturnsFailedOutcome()
        {
            // Arrange
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithError(_testError)
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Value.Should().Be(default(string));
        }

        [Fact]
        public void FromErrors_Throws_IfNull()
        {
            // Arrange & Act
            Action act = () => EndpointOutcome<int>.FromErrors(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_Throws_IfEmpty()
        {
            // Arrange & Act
            Action act = () => EndpointOutcome<int>.FromErrors(Enumerable.Empty<ErrorInfo>());

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_ReturnsFailedOutcome()
        {
            // Arrange
            ErrorInfo[] errors = new[] { _testError, TestErrorInfoFactory.Custom(ErrorCategory.General, "E2", "Another") };
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithErrors(errors)
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void From_Throws_IfResultIsNull()
        {
            // Arrange & Act
            Action act = () => EndpointOutcome<string>.From(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void From_ReturnsOutcome_WithResult()
        {
            // Arrange
            string value = "abc";
            Mock<IResult<string>> mockResult = ResultMockHelper.CreateMockSuccessfulResult(value);

            // Act
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.From(mockResult.Object);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
        }

        [Fact]
        public void NotFound_ReturnsFailedOutcome()
        {
            // Arrange
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithError(TestErrorInfoFactory.NotFound(detail: "not found", code: "404"))
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.NotFound);
            outcome.Errors[0].Detail.Should().Be("not found");
            outcome.Errors[0].Code.Should().Be("404");
        }

        [Fact]
        public void Unauthorized_ReturnsFailedOutcome()
        {
            const string Detail = "unauth";
            const string Code = "401";

            // Arrange
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithError(TestErrorInfoFactory.Authentication(detail: Detail, code: Code))
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.Unauthorized);
            outcome.Errors[0].Detail.Should().Be(Detail);
            outcome.Errors[0].Code.Should().Be(Code);
        }

        [Fact]
        public void Forbidden_ReturnsFailedOutcome()
        {
            const string Detail = "forbid";
            const string Code = "403";

            // Arrange
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithError(TestErrorInfoFactory.Authorization(detail: Detail, code: Code))
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.Forbidden);
            outcome.Errors[0].Detail.Should().Be(Detail);
            outcome.Errors[0].Code.Should().Be(Code);
        }

        [Fact]
        public void FromException_Throws_IfNull()
        {
            // Arrange & Act
            Action act = () => EndpointOutcome<string>.FromException(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("ex");
        }

        [Fact]
        public void FromException_ReturnsFailedOutcome()
        {
            // Arrange
            Exception ex = new InvalidOperationException("fail!");
            EndpointOutcomeBuilder<string> builder = new EndpointOutcomeBuilder<string>();

            // Act
            IEndpointOutcome<string> outcome = builder
                .WithError(TestErrorInfoFactory.InternalErrorFromException(ex))
                .Build();

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().NotBeEmpty();
            outcome.Errors[0].Metadata.Should().ContainKey("exceptionType").WhoseValue.Should().Be(ex.GetType().FullName);
            outcome.Errors[0].Metadata.Should().ContainKey("exceptionMessage").WhoseValue.Should().Be(ex.Message);

            outcome.Status.Code.Should().Be(ResultStatuses.InternalServerError.Code);
            outcome.Errors[0].Message.Should().Be("fail!");
        }

        [Fact]
        public void WithMetadataInternal_AppliesTransformAndReturnsNewInstance()
        {
            // Arrange
            IEndpointOutcome<string> original = EndpointOutcomeBuilder.IsSuccess("x")
                .WithMetadata("a", 1)
                .Build();

            // Act
            EndpointOutcome<string> newOutcome = (EndpointOutcome<string>)((EndpointOutcome<string>)original).WithMetadataInternal(m => m.WithTag("b", 2));

            // Assert
            newOutcome.Should().NotBeSameAs(original);
            newOutcome.Metadata.Tags.Should().ContainKey("b").WhoseValue.Should().Be(2);
            original.Metadata.Tags.Should().ContainKey("a").WhoseValue.Should().Be(1);
            newOutcome.Metadata.Tags.Should().ContainKey("a").WhoseValue.Should().Be(1);
        }

        [Fact]
        public void ToString_ReturnsTypeName()
        {
            // Arrange
            IEndpointOutcome<string> outcome = EndpointOutcomeBuilder.IsSuccess("y").Build();

            // Act
            string outcomeString = outcome.ToString()!;

            // Assert
            outcomeString.Should().Be("Zentient.Endpoints.EndpointOutcome`1[System.String]");
        }

        [Fact]
        public void GetValueAsObject_ReturnsValue()
        {
            // Arrange
            string value = "z";
            EndpointOutcome<string> outcome = (EndpointOutcome<string>)EndpointOutcomeBuilder.IsSuccess(value).Build();

            // Act
            object? obj = outcome.GetValueAsObject();

            // Assert
            obj.Should().Be(value);
        }
    }
}
#pragma warning restore CS1591
