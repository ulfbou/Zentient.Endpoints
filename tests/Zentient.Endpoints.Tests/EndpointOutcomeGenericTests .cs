using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Zentient.Results;
using Zentient.Results.Constants;
using Zentient.Endpoints.Constants;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

using Zentient.Endpoints;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeGenericTests : EndpointTestBase
    {
        private static readonly ErrorInfo _testError = CreateErrorInfo(code: "ERR", message: "Test error");
        private static readonly string[] _successMessages = new[] { "Success!" };

        // --- Constructor Tests ---

        [Fact]
        public void Constructor_Throws_IfResultIsNull()
        {
            // CA1806: The exception is the test's purpose.
            Action act = () => _ = new EndpointOutcome<string>(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Constructor_SetsProperties_ForSuccess()
        {
            // Arrange
            string value = "abc";
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult(value, CreateMockResultStatus(200, "OK"), _successMessages);
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });

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
            Mock<IResult<string>> mockResult = CreateMockFailedResult<string>(new[] { _testError }, CreateMockResultStatus(400, "Bad Request"));
            TransportMetadata metadata = CreateTransportMetadata();

            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object, metadata);

            outcome.IsSuccess.Should().BeFalse();
            outcome.IsFailure.Should().BeTrue();
            outcome.Value.Should().Be(default(string));
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Status.Should().Be(mockResult.Object.Status);
            outcome.Metadata.Should().Be(metadata);
        }

        // --- Value Property ---

        [Fact]
        public void Value_ReturnsValue_OnSuccess()
        {
            string value = "val";
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult(value);
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            outcome.Value.Should().Be(value);
        }

        [Fact]
        public void Value_ReturnsDefault_OnFailure()
        {
            Mock<IResult<int>> mockResult = CreateMockFailedResult<int>(new[] { _testError });
            EndpointOutcome<int> outcome = new EndpointOutcome<int>(mockResult.Object);

            outcome.Value.Should().Be(default(int));
        }

        [Fact]
        public void Value_ReturnsNull_OnSuccessWithNull()
        {
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult<string>(null!);
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            outcome.Value.Should().BeNull();
        }

        // --- Static Factory Methods ---

        [Fact]
        public void Success_ReturnsSuccessfulOutcome_WithValueAndMetadata()
        {
            int value = 42;
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "foo", "bar" } });

            IEndpointOutcome<int> outcome = EndpointOutcome<int>.Success(value, metadata);

            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
            outcome.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        }

        [Fact]
        public void Success_WithStatus_ReturnsSuccessfulOutcome()
        {
            int value = 99;
            IResultStatus status = CreateMockResultStatus(201, "Created");
            TransportMetadata metadata = CreateTransportMetadata();

            IEndpointOutcome<int> outcome = EndpointOutcome<int>.Success(value, status, metadata);

            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
            outcome.Status.Should().Be(status);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void NoContent_ReturnsOutcome_WithDefaultValue()
        {
            IEndpointOutcome<Guid> outcome = EndpointOutcome<Guid>.NoContent();

            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(default(Guid));
            outcome.Status.Code.Should().Be(ResultStatuses.NoContent.Code);
        }

        [Fact]
        public void NoContent_ReturnsOutcome_ForUnit()
        {
            IEndpointOutcome<Unit> outcome = EndpointOutcome<Unit>.NoContent();

            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(Unit.Value);
            outcome.Status.Code.Should().Be(ResultStatuses.NoContent.Code);
        }

        [Fact]
        public void FromError_Throws_IfErrorIsNull()
        {
            Action act = () => EndpointOutcome<string>.FromError(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("error");
        }

        [Fact]
        public void FromError_ReturnsFailedOutcome()
        {
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.FromError(_testError);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Value.Should().Be(default(string));
        }

        [Fact]
        public void FromErrors_Throws_IfNull()
        {
            Action act = () => EndpointOutcome<int>.FromErrors(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_Throws_IfEmpty()
        {
            Action act = () => EndpointOutcome<int>.FromErrors(Enumerable.Empty<ErrorInfo>());
            act.Should().Throw<ArgumentException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_ReturnsFailedOutcome()
        {
            ErrorInfo[] errors = new[] { _testError, CreateErrorInfo(code: "E2", message: "Another") };
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.FromErrors(errors);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void From_Throws_IfResultIsNull()
        {
            Action act = () => EndpointOutcome<string>.From(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void From_ReturnsOutcome_WithResult()
        {
            string value = "abc";
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult(value);
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.From(mockResult.Object);

            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(value);
        }

        [Fact]
        public void NotFound_ReturnsFailedOutcome()
        {
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.NotFound("not found", "404");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.NotFound);
            outcome.Errors[0].Message.Should().Be("not found"); // CA1826: Use indexer
        }

        [Fact]
        public void Unauthorized_ReturnsFailedOutcome()
        {
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.Unauthorized("unauth", "401");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.Unauthorized);
            outcome.Errors[0].Message.Should().Be("unauth"); // CA1826: Use indexer
        }

        [Fact]
        public void Forbidden_ReturnsFailedOutcome()
        {
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.Forbidden("forbid", "403");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatusConstants.Code.Forbidden);
            outcome.Errors[0].Message.Should().Be("forbid"); // CA1826: Use indexer
        }

        [Fact]
        public void FromException_Throws_IfNull()
        {
            Action act = () => EndpointOutcome<string>.FromException(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("ex");
        }

        [Fact]
        public void FromException_ReturnsFailedOutcome()
        {
            Exception ex = new InvalidOperationException("fail!");
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.FromException(ex);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().NotBeEmpty();
            outcome.Status.Code.Should().Be(ResultStatuses.Error.Code);
        }

        // --- WithMetadataInternal ---

        [Fact]
        public void WithMetadataInternal_AppliesTransformAndReturnsNewInstance()
        {
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult("x");
            EndpointOutcome<string> original = new EndpointOutcome<string>(mockResult.Object, CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } }));

            EndpointOutcome<string> newOutcome = (EndpointOutcome<string>)original.WithMetadataInternal(m => m.WithTag("b", 2));

            newOutcome.Should().NotBeSameAs(original);
            newOutcome.Metadata.Tags.Should().ContainKey("b").WhoseValue.Should().Be(2);
            original.Metadata.Tags.Should().ContainKey("a");
        }

        // --- ToString ---

        [Fact]
        public void ToString_ReturnsTypeName()
        {
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult("y");
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            outcome.ToString().Should().Be("Zentient.Endpoints.EndpointOutcome`1[System.String]");
        }

        // --- GetValueAsObject ---

        [Fact]
        public void GetValueAsObject_ReturnsValue()
        {
            string value = "z";
            Mock<IResult<string>> mockResult = CreateMockSuccessfulResult(value);
            EndpointOutcome<string> outcome = new EndpointOutcome<string>(mockResult.Object);

            object? obj = outcome.GetValueAsObject();

            obj.Should().Be(value);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
