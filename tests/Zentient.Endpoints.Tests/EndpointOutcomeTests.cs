using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using Moq;

using Xunit;

using Zentient.Results;
using Zentient.Results.Constants;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeTests : EndpointTestBase
    {
        private static readonly ErrorInfo _testError = CreateErrorInfo(code: "ERR", message: "Test error");
        // CA1861: Use static readonly for constant array argument
        private static readonly string[] SuccessMsgArray = new[] { "msg" };

        // --- Constructor Tests ---

        [Fact]
        public void Constructor_Throws_IfResultIsNull()
        {
            // CA1806: Assign to discard to avoid warning
            Action act = () => _ = new TestableEndpointOutcome(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void Constructor_SetsProperties_ForSuccess()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult(CreateMockResultStatus(200, "OK"), SuccessMsgArray);
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });

            var outcome = new TestableEndpointOutcome(mockResult.Object, metadata);

            outcome.IsSuccess.Should().BeTrue();
            outcome.IsFailure.Should().BeFalse();
            outcome.Errors.Should().BeEmpty();
            outcome.Messages.Should().Contain("msg");
            outcome.Status.Should().Be(mockResult.Object.Status);
            outcome.Metadata.Should().Be(metadata);
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResult.Object);
        }

        [Fact]
        public void Constructor_SetsProperties_ForFailure()
        {
            Mock<IResult> mockResult = CreateMockFailedResult(new[] { _testError }, CreateMockResultStatus(400, "Bad Request"));
            TransportMetadata metadata = CreateTransportMetadata();

            var outcome = new TestableEndpointOutcome(mockResult.Object, metadata);

            outcome.IsSuccess.Should().BeFalse();
            outcome.IsFailure.Should().BeTrue();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(_testError);
            outcome.Status.Should().Be(mockResult.Object.Status);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void Constructor_DefaultsMetadataToEmpty()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult();
            var outcome = new TestableEndpointOutcome(mockResult.Object);

            outcome.Metadata.Should().NotBeNull();
            outcome.Metadata.Tags.Should().BeEmpty();
        }

        // --- Property Proxying ---

        [Fact]
        public void Properties_Proxy_ToUnderlyingResult()
        {
            ErrorInfo error = CreateErrorInfo(message: "err");
            string[] messages = new[] { "m1", "m2" };
            IResultStatus status = CreateMockResultStatus(201, "Created");
            Mock<IResult> mockResult = new Mock<IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(true);
            mockResult.SetupGet(r => r.IsFailure).Returns(false);
            mockResult.SetupGet(r => r.Errors).Returns(new List<ErrorInfo> { error });
            mockResult.SetupGet(r => r.Messages).Returns(messages);
            mockResult.SetupGet(r => r.ErrorMessage).Returns("err");
            mockResult.SetupGet(r => r.Status).Returns(status);

            var outcome = new TestableEndpointOutcome(mockResult.Object);

            outcome.IsSuccess.Should().BeTrue();
            outcome.IsFailure.Should().BeFalse();
            outcome.Errors.Should().ContainSingle().Which.Should().Be(error);
            outcome.Messages.Should().BeEquivalentTo(messages);
            outcome.ErrorMessage.Should().Be("err");
            outcome.Status.Should().Be(status);
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResult.Object);
        }

        // --- Static Factory Methods ---

        [Fact]
        public void Success_ReturnsSuccessfulOutcome()
        {
            IEndpointOutcome outcome = EndpointOutcome.Success();

            outcome.IsSuccess.Should().BeTrue();
            outcome.Errors.Should().BeEmpty();
            outcome.Status.Code.Should().Be(ResultStatuses.Success.Code);
            outcome.Metadata.Tags.Should().BeEmpty();
            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().BeOfType<Result>();
        }

        [Fact]
        public void Success_WithStatus_ReturnsSuccessfulOutcome()
        {
            IResultStatus status = CreateMockResultStatus(201, "Created");
            IEndpointOutcome outcome = EndpointOutcome.Success(status);

            outcome.IsSuccess.Should().BeTrue();
            outcome.Status.Should().Be(status);
        }

        [Fact]
        public void Success_WithMetadata_ReturnsSuccessfulOutcome()
        {
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "foo", "bar" } });
            IEndpointOutcome outcome = EndpointOutcome.Success(metadata);

            outcome.IsSuccess.Should().BeTrue();
            outcome.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        }

        [Fact]
        public void From_Throws_IfResultIsNull()
        {
            Action act = () => EndpointOutcome.From(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void From_ReturnsOutcome_WithResultAndMetadata()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult();
            TransportMetadata metadata = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });

            IEndpointOutcome outcome = EndpointOutcome.From(mockResult.Object, metadata);

            ((IEndpointOutcomeInternal)outcome).GetUnderlyingResult().Should().Be(mockResult.Object);
            outcome.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void FromError_Throws_IfErrorIsNull()
        {
            Action act = () => EndpointOutcome.FromError(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("error");
        }

        [Fact]
        public void FromError_ReturnsFailedOutcome()
        {
            IEndpointOutcome outcome = EndpointOutcome.FromError(_testError);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.BadRequest.Code);
            outcome.Metadata.Should().NotBeNull();
            outcome.Metadata.Tags.Should().BeEmpty();
        }

        [Fact]
        public void FromErrors_Throws_IfNull()
        {
            Action act = () => EndpointOutcome.FromErrors(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_Throws_IfEmpty()
        {
            Action act = () => EndpointOutcome.FromErrors(Enumerable.Empty<ErrorInfo>());
            act.Should().Throw<ArgumentException>().WithParameterName("errors");
        }

        [Fact]
        public void FromErrors_ReturnsFailedOutcome()
        {
            ErrorInfo[] errors = new[] { _testError, CreateErrorInfo(code: "E2", message: "Another") };
            IEndpointOutcome outcome = EndpointOutcome.FromErrors(errors);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void NotFound_ReturnsFailedOutcome()
        {
            IEndpointOutcome outcome = EndpointOutcome.NotFound("not found", "404");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.NotFound.Code);
            outcome.Errors[0].Message.Should().Be("not found");
        }

        [Fact]
        public void Unauthorized_ReturnsFailedOutcome()
        {
            IEndpointOutcome outcome = EndpointOutcome.Unauthorized("unauth", "401");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.Unauthorized.Code);
            outcome.Errors[0].Message.Should().Be("unauth");
        }

        [Fact]
        public void Forbidden_ReturnsFailedOutcome()
        {
            IEndpointOutcome outcome = EndpointOutcome.Forbidden("forbid", "403");

            outcome.IsSuccess.Should().BeFalse();
            outcome.Status.Code.Should().Be(ResultStatuses.Forbidden.Code);
            outcome.Errors[0].Message.Should().Be("forbid");
        }

        [Fact]
        public void FromException_Throws_IfNull()
        {
            Action act = () => EndpointOutcome.FromException(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("ex");
        }

        [Fact]
        public void FromException_ReturnsFailedOutcome()
        {
            Exception ex = new InvalidOperationException("fail!");
            IEndpointOutcome outcome = EndpointOutcome.FromException(ex);

            outcome.IsSuccess.Should().BeFalse();
            outcome.Errors.Should().NotBeEmpty();
            outcome.Status.Code.Should().Be(ResultStatuses.Error.Code);
        }

        // --- WithMetadataInternal ---

        [Fact]
        public void WithMetadataInternal_AppliesTransformAndReturnsNewInstance()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult();
            TransportMetadata originalMetadata = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } });
            var original = new TestableEndpointOutcome(mockResult.Object, originalMetadata);

            EndpointOutcome newOutcome = original.WithMetadataInternal(m => m.WithTag("b", 2));

            newOutcome.Should().NotBeSameAs(original);
            newOutcome.Metadata.Tags.Should().ContainKey("b").WhoseValue.Should().Be(2);
            original.Metadata.Tags.Should().ContainKey("a");
        }

        // --- GetValueAsObject ---

        [Fact]
        public void GetValueAsObject_AlwaysReturnsNull()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult();
            var outcome = new TestableEndpointOutcome(mockResult.Object);

            outcome.GetValueAsObject().Should().BeNull();
        }

        // --- Equals/GetHashCode/ToString ---

        [Fact]
        public void Equals_ReturnsTrue_ForIdenticalOutcomes()
        {
            IResult result1 = Result.Success();
            IResult result2 = Result.Success();
            TransportMetadata metadata = new TransportMetadata();

            var o1 = new TestableEndpointOutcome(result1, metadata);
            var o2 = new TestableEndpointOutcome(result2, metadata);

            o1.Equals(o2).Should().BeTrue();
            o1.GetHashCode().Should().Be(o2.GetHashCode());
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentResults()
        {
            IResult result1 = Result.Success();
            IResult result2 = Result.Failure(_testError);
            TransportMetadata metadata = new TransportMetadata();

            var o1 = new TestableEndpointOutcome(result1, metadata);
            var o2 = new TestableEndpointOutcome(result2, metadata);

            o1.Equals(o2).Should().BeFalse();
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentMetadata()
        {
            IResult result = Result.Success();
            TransportMetadata m1 = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 1 } });
            TransportMetadata m2 = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 2 } });

            var o1 = new TestableEndpointOutcome(result, m1);
            var o2 = new TestableEndpointOutcome(result, m2);

            o1.Equals(o2).Should().BeFalse();
        }

        [Fact]
        [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "<Pending>")]
        public void Equals_ReturnsFalse_ForNull()
        {
            // Arrange
            IResult result = Result.Success();
            var o1 = new TestableEndpointOutcome(result);

            // Act & Assert
            object? nullObj = null;
            object.Equals(o1, nullObj).Should().BeFalse();
        }

        [Fact]
        public void ToString_ReturnsExpectedTypeName()
        {
            Mock<IResult> mockResult = CreateMockSuccessfulResult();
            var outcome = new TestableEndpointOutcome(mockResult.Object);

            outcome.ToString().Should().Be("Zentient.Endpoints.EndpointOutcome");
        }

        // Helper to allow testing protected constructor
        private sealed class TestableEndpointOutcome : EndpointOutcome
        {
            public TestableEndpointOutcome(IResult result, TransportMetadata? metadata = null)
                : base(result, metadata)
            { }

            public new object? GetValueAsObject() => base.GetValueAsObject();
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
