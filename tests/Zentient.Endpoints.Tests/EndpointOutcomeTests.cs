// <copyright file="EndpointOutcomeTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;
using Zentient.Results;
using Zentient.Results.Constants;
using Zentient.Endpoints.Tests.Common;

#pragma warning disable CS1591

namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeTests : EndpointTestBase
    {
        private static readonly ErrorInfo _testError = CreateErrorInfo(code: "ERR", message: "Test error");
        private static readonly string[] SuccessMsgArray = new[] { "msg" };

        [Fact]
        public void Constructor_Throws_IfResultIsNull()
        {
            // Arrange
            IResult? nullResult = null;

            // Act
            Action act = () => _ = new TestableEndpointOutcome(nullResult!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Constructor_SetsProperties_ForSuccess()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult(CreateMockResultStatus(200, "OK"), SuccessMsgArray).Object;
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });

            // Act
            var outcome = new TestableEndpointOutcome(mockResultObject, metadata);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.IsFailure.Should().BeFalse();
            outcome.Errors.Should().BeEmpty();
            outcome.Messages.Should().Contain("msg");
            outcome.Status.Should().Be(mockResultObject.Status);
            outcome.Metadata.Should().Be(metadata);
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResultObject);
        }

        [Fact]
        public void Constructor_SetsProperties_ForFailure()
        {
            // Arrange
            IResult mockResultObject = CreateMockFailedResult(new[] { _testError }, CreateMockResultStatus(400, "Bad Request")).Object;
            TransportMetadata metadata = CreateTransportMetadata();

            // Act
            var outcome = new TestableEndpointOutcome(mockResultObject, metadata);

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.IsFailure.Should().BeTrue();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Status.Should().Be(mockResultObject.Status);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void Constructor_DefaultsMetadataToEmpty()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult().Object;

            // Act
            var outcome = new TestableEndpointOutcome(mockResultObject);

            // Assert
            outcome.Metadata.Should().NotBeNull();
            outcome.Metadata.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Properties_Proxy_ToUnderlyingResult()
        {
            // Arrange
            ErrorInfo error = CreateErrorInfo(message: "err");
            string[] messages = new[] { "m1", "m2" };
            IResultStatus status = CreateMockResultStatus(201, "Created");

            // Use the common helper for a successful mock, then override specific properties for this test's unique scenario
            // The original test directly mocked IResult to set specific properties that might not align with typical
            // success/failure result conventions (e.g., a "success" result having errors).
            // Keeping the direct Moq setup for this specific "proxying" test, as its purpose is to verify raw property forwarding.
            Mock<IResult> mockResult = new Mock<IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(true);
            mockResult.SetupGet(r => r.IsFailure).Returns(false);
            mockResult.SetupGet(r => r.Errors).Returns(new List<ErrorInfo> { error });
            mockResult.SetupGet(r => r.Messages).Returns(messages);
            mockResult.SetupGet(r => r.ErrorMessage).Returns("err");
            mockResult.SetupGet(r => r.Status).Returns(status);

            // Act
            var outcome = new TestableEndpointOutcome(mockResult.Object);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.IsFailure.Should().BeFalse();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(error);
            outcome.Messages.Should().BeEquivalentTo(messages);
            outcome.ErrorMessage.Should().Be("err");
            outcome.Status.Should().Be(status);
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResult.Object);
        }

        [Fact]
        public void Success_ReturnsSuccessfulOutcome()
        {
            // Arrange & Act
            IEndpointOutcome outcome = EndpointOutcome.Success();

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Errors.Should().BeEmpty();
            outcome.Status.Code.Should().Be(ResultStatuses.Success.Code);
            outcome.Metadata.Tags.Should().BeEmpty();
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().BeOfType<Result>();
        }

        [Fact]
        public void Success_WithStatus_ReturnsSuccessfulOutcome()
        {
            // Arrange
            IResultStatus status = CreateMockResultStatus(201, "Created");

            // Act
            IEndpointOutcome outcome = EndpointOutcome.Success(status);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Status.Should().Be(status);
        }

        [Fact]
        public void Success_WithMetadata_ReturnsSuccessfulOutcome()
        {
            // Arrange
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "foo", "bar" } });

            // Act
            IEndpointOutcome outcome = EndpointOutcome.Success(metadata);

            // Assert
            outcome.IsSuccess.Should().BeTrue();
            outcome.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        }

        [Fact]
        public void From_Throws_IfResultIsNull()
        {
            // Arrange
            IResult? nullResult = null;

            // Act
            Action act = () => EndpointOutcome.From(nullResult!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void From_ReturnsOutcome_WithResultAndMetadata()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult().Object;
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });

            // Act
            IEndpointOutcome outcome = EndpointOutcome.From(mockResultObject, metadata);

            // Assert
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResultObject);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void FromError_Throws_IfErrorIsNull()
        {
            // Arrange
            ErrorInfo? nullError = null;

            // Act
            Action act = () => EndpointOutcome.FromError(nullError!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("error");
        }

        [Fact]
        public void FromError_ReturnsFailedOutcome()
        {
            // Arrange & Act
            IEndpointOutcome outcome = EndpointOutcome.FromError(_testError);

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.BadRequest.Code);
            outcome.Metadata.Should().NotBeNull();
            outcome.Metadata.Tags.Should().BeEmpty();
        }

        [Fact]
        public void FromErrors_Throws_IfNull()
        {
            // Arrange
            IEnumerable<ErrorInfo>? nullErrors = null;

            // Act
            Action act = () => EndpointOutcome.FromErrors(nullErrors!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_Throws_IfEmpty()
        {
            // Arrange
            IEnumerable<ErrorInfo> emptyErrors = Enumerable.Empty<ErrorInfo>();

            // Act
            Action act = () => EndpointOutcome.FromErrors(emptyErrors);

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_ReturnsFailedOutcome()
        {
            // Arrange
            ErrorInfo[] errors = new[] { _testError, CreateErrorInfo(code: "E2", message: "Another") };

            // Act
            IEndpointOutcome outcome = EndpointOutcome.FromErrors(errors);

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void NotFound_ReturnsFailedOutcome()
        {
            // Arrange & Act
            IEndpointOutcome outcome = EndpointOutcome.NotFound("not found", "404");

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.NotFound.Code);
            outcome.Errors[0].Message.Should().Be("not found");
        }

        [Fact]
        public void Unauthorized_ReturnsFailedOutcome()
        {
            // Arrange & Act
            IEndpointOutcome outcome = EndpointOutcome.Unauthorized("unauth", "401");

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.Unauthorized.Code);
            outcome.Errors[0].Message.Should().Be("unauth");
        }

        [Fact]
        public void Forbidden_ReturnsFailedOutcome()
        {
            // Arrange & Act
            IEndpointOutcome outcome = EndpointOutcome.Forbidden("forbid", "403");

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.Forbidden.Code);
            outcome.Errors[0].Message.Should().Be("forbid");
        }

        [Fact]
        public void FromException_Throws_IfNull()
        {
            // Arrange
            Exception? nullEx = null;

            // Act
            Action act = () => EndpointOutcome.FromException(nullEx!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("ex");
        }

        [Fact]
        public void FromException_ReturnsFailedOutcome()
        {
            // Arrange
            Exception ex = new InvalidOperationException("fail!");

            // Act
            IEndpointOutcome outcome = EndpointOutcome.FromException(ex);

            // Assert
            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().NotBeEmpty();
            outcome.Status.Code.Should().Be(ResultStatuses.Error.Code);
        }

        [Fact]
        public void WithMetadataInternal_AppliesTransformAndReturnsNewInstance()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult().Object;
            TransportMetadata originalMetadata = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } });
            var original = new TestableEndpointOutcome(mockResultObject, originalMetadata);

            // Act
            EndpointOutcome newOutcome = original.WithMetadataInternal(m => m.WithTag("b", 2));

            // Assert
            newOutcome.Should().NotBeSameAs(original);
            newOutcome.Metadata.Tags.Should().ContainKey("b").WhoseValue.Should().Be(2);
            original.Metadata.Tags.Should().ContainKey("a");
        }

        [Fact]
        public void GetValueAsObject_AlwaysReturnsNull()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult().Object;
            var outcome = new TestableEndpointOutcome(mockResultObject);

            // Act & Assert
            outcome.GetValueAsObject().Should().BeNull();
        }

        [Fact]
        public void Equals_ReturnsTrue_ForIdenticalOutcomes()
        {
            // Arrange
            IResult result1 = Result.Success();
            TransportMetadata metadata1 = new TransportMetadata();
            var o1 = new TestableEndpointOutcome(result1, metadata1);
            IResult result2 = Result.Success();
            TransportMetadata metadata2 = new TransportMetadata();
            var o2 = new TestableEndpointOutcome(result2, metadata2);

            // Act & Assert
            o1.Equals(o2).Should().BeTrue();
            o1.GetHashCode().Should().Be(o2.GetHashCode());
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentResults()
        {
            // Arrange
            IResult result1 = Result.Success();
            IResult result2 = Result.Failure(_testError);
            TransportMetadata metadata = new TransportMetadata();
            var o1 = new TestableEndpointOutcome(result1, metadata);
            var o2 = new TestableEndpointOutcome(result2, metadata);

            // Act & Assert
            o1.Equals(o2).Should().BeFalse();
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentMetadata()
        {
            // Arrange
            IResult result = Result.Success();
            TransportMetadata m1 = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });
            TransportMetadata m2 = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 2 } });
            var o1 = new TestableEndpointOutcome(result, m1);
            var o2 = new TestableEndpointOutcome(result, m2);

            // Act & Assert
            o1.Equals(o2).Should().BeFalse();
        }

        [Fact]
        [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "<Pending>")]
        public void Equals_ReturnsFalse_ForNull()
        {
            // Arrange
            IResult result = Result.Success();
            var o1 = new TestableEndpointOutcome(result);
            object? nullObj = null;

            // Act & Assert
            object.Equals(o1, nullObj).Should().BeFalse();
        }

        [Fact]
        public void ToString_ReturnsExpectedTypeName()
        {
            // Arrange
            IResult mockResultObject = CreateMockSuccessfulResult().Object;
            var outcome = new TestableEndpointOutcome(mockResultObject);

            // Act & Assert
            outcome.ToString().Should().Be("Zentient.Endpoints.EndpointOutcome");
        }

        private sealed class TestableEndpointOutcome : EndpointOutcome
        {
            public TestableEndpointOutcome(IResult result, TransportMetadata? metadata = null)
                : base(result, metadata)
            { }

            public new object? GetValueAsObject() => base.GetValueAsObject();
        }
    }
}
#pragma warning restore CS1591
