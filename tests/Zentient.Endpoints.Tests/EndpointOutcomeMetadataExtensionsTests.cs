// <copyright file="EndpointOutcomeMetadataExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Zentient.Endpoints.Http;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Results;
using Zentient.Endpoints.Tests.Common;
using System.Threading.Tasks;

#pragma warning disable CS1591

namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeMetadataExtensionsTests : EndpointTestBase
    {
        [Fact]
        public void WithMetadata_Generic_ThrowsOnNulls()
        {
            // Arrange
            IEndpointOutcome<string>? outcome = null;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);

            // Act
            Action act1 = () => outcome!.WithMetadata(transform);
            Action act2 = () => EndpointOutcome<string>.Success("abc").WithMetadata(null!);

            // Assert
            act1.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
            act2.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
        }

        [Fact]
        public void WithMetadata_Generic_ThrowsOnNonConcreteType()
        {
            // Arrange
            var result = Result<string>.Success("abc");
            var metadata = CreateTransportMetadata();
            IEndpointOutcome<string> mock = EndpointOutcomeMockHelper.CreateGenericEndpointOutcomeMock(result, metadata);
            IEndpointOutcome<string> outcome = mock;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);

            // Act
            Action act = () => outcome.WithMetadata(transform);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*EndpointOutcome*");
        }

        [Fact]
        public void WithMetadata_Generic_AppliesTransform()
        {
            // Arrange
            IEndpointOutcome<string> original = EndpointOutcome<string>.Success("abc");
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("foo", 42);

            // Act
            var updated = original.WithMetadata(transform);

            // Assert
            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(42);
            original.Metadata.Tags.Should().NotContainKey("foo");
            updated.Value.Should().Be("abc");
        }

        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNulls()
        {
            // Arrange
            IEndpointOutcome? outcome = null;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);

            // Act
            Action act1 = () => outcome!.WithMetadata(transform);
            Action act2 = () => EndpointOutcome.Success().WithMetadata(null!);

            // Assert
            act1.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
            act2.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
        }

        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNonConcreteType()
        {
            // Arrange
            var mock = EndpointOutcomeMockHelper.CreateEndpointOutcomeMock(Result.Success());
            IEndpointOutcome outcome = mock;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);

            // Act
            Action act = () => outcome.WithMetadata(transform);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*EndpointOutcome*");
        }

        [Fact]
        public void WithMetadata_NonGeneric_AppliesTransform()
        {
            // Arrange
            IEndpointOutcome original = EndpointOutcome.Success();
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("bar", 99);

            // Act
            var updated = original.WithMetadata(transform);

            // Assert
            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("bar").WhoseValue.Should().Be(99);
            original.Metadata.Tags.Should().NotContainKey("bar");
        }

        [Fact]
        public void OutcomeEquals_ReferenceEquals_And_Identical()
        {
            // Arrange
            var o1 = EndpointOutcome<int>.Success(5);
            var o2 = EndpointOutcome<int>.Success(5);

            // Act & Assert
            o1.OutcomeEquals(o1).Should().BeTrue();
            o1.OutcomeEquals(o2).Should().BeTrue();
        }

        [Theory]
        [InlineData(5, 6)]
        [InlineData(42, 43)]
        public void OutcomeEquals_DifferentValues(int v1, int v2)
        {
            // Arrange
            var o1 = EndpointOutcome<int>.Success(v1);
            var o2 = EndpointOutcome<int>.Success(v2);

            // Act & Assert
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentStatus()
        {
            // Arrange
            var status1 = CreateMockResultStatus(200, "OK");
            var status2 = CreateMockResultStatus(201, "Created");
            var o1 = EndpointOutcome<int>.Success(5, status1);
            var o2 = EndpointOutcome<int>.Success(5, status2);

            // Act & Assert
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentMetadata()
        {
            // Arrange
            var m1 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } });
            var m2 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 2 } });
            var o1 = EndpointOutcome<int>.Success(5, m1);
            var o2 = EndpointOutcome<int>.Success(5, m2);

            // Act & Assert
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentErrors()
        {
            // Arrange
            var e1 = TestErrorInfoFactory.Custom(ErrorCategory.General, "E1", "err1");
            var e2 = TestErrorInfoFactory.Custom(ErrorCategory.General, "E2", "err2");
            var o1 = EndpointOutcome<int>.FromError(e1);
            var o2 = EndpointOutcome<int>.FromError(e2);

            // Act & Assert
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_NullCases()
        {
            // Arrange
            var o1 = EndpointOutcome<int>.Success(5);
            IEndpointOutcome<int>? o2 = null;

            // Act & Assert
            o1.OutcomeEquals(o2!).Should().BeFalse();
        }

        [Fact]
        public async Task ToHttpResult_ThrowsOnNulls()
        {
            // Arrange
            IEndpointOutcome? outcome = null;
            var ctx = CreateHttpContext();

            // Act
            Func<Task> act1 = () => outcome!.ToHttpResult(ctx);
            Func<Task> act2 = () => EndpointOutcome<string>.Success("abc").ToHttpResult(null!);

            // Assert
            await act1.Should().ThrowAsync<ArgumentNullException>().WithParameterName("endpointResult");
            await act2.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task ToHttpResult_ThrowsIfMapperNotRegistered()
        {
            // Arrange
            var ctx = new TestHttpContextBuilder().WithServices(s => { /* no mapper */ }).Build();
            var outcome = EndpointOutcome<string>.Success("abc");

            // Act
            Func<Task> act = () => outcome.ToHttpResult(ctx);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No service for type*IEndpointOutcomeToHttpMapper*");
        }

        [Fact]
        public async Task ToHttpResult_DelegatesToMapper()
        {
            // Arrange
            var mapperMock = new Mock<IEndpointOutcomeToHttpMapper>();
            var ctx = new TestHttpContextBuilder().WithService(mapperMock.Object).Build();
            var outcome = EndpointOutcome<string>.Success("abc");
            var expectedResult = Microsoft.AspNetCore.Http.Results.Ok("result");
            mapperMock.Setup(m => m.Map(outcome, ctx, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            // Act
            var result = await outcome.ToHttpResult(ctx);

            // Assert
            result.Should().BeSameAs(expectedResult);
            mapperMock.Verify(m => m.Map(outcome, ctx, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void ToMinimalApiResult_ReturnsSameInstance_And_ThrowsOnNull()
        {
            // Arrange
            var outcome = EndpointOutcome<int>.Success(42);
            IEndpointOutcome<string>? nullOutcome = null;

            // Act & Assert
            outcome.ToMinimalApiResult().Should().BeSameAs(outcome);
            Action act = () => nullOutcome!.ToMinimalApiResult();
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        [Fact]
        public void ToEndpointOutcome_Generic_Success_And_Failure()
        {
            // Arrange
            var messages = new[] { "msg1", "msg2" };
            var successResult = Result<string>.Success("data", ResultMockHelper.CreateMockResultStatus(200, "OK"), messages);
            var error = TestErrorInfoFactory.Validation();
            var failResult = Result<string>.Failure(errors: new[] { error }, status: ResultMockHelper.CreateMockResultStatus(400, "Bad Request"));

            // Act
            var successOutcome = successResult.ToEndpointOutcome();
            var failOutcome = failResult.ToEndpointOutcome();

            // Assert - Success Outcome
            successOutcome.IsSuccess.Should().BeTrue();
            successOutcome.Value.Should().Be("data");
            successOutcome.Messages.Should().BeEquivalentTo(messages);
            successOutcome.Status.Code.Should().Be(200);
            successOutcome.Errors.Should().BeEmpty();
            successOutcome.Metadata.Should().NotBeNull();

            // Assert - Failure Outcome
            failOutcome.IsSuccess.Should().BeFalse();
            failOutcome.Value.Should().BeNull();
            failOutcome.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(error);
            failOutcome.Status.Code.Should().Be(400);
            failOutcome.Metadata.Should().NotBeNull();
        }

        [Fact]
        public void ToEndpointOutcome_Generic_Throws_If_Null()
        {
            // Arrange
            Zentient.Results.IResult<string>? result = null;

            // Act
            Action act = () => result!.ToEndpointOutcome();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ToEndpointOutcome_NonGeneric_Success_And_Failure()
        {
            // Arrange
            var successResult = Result.Success();
            var error = TestErrorInfoFactory.Conflict();
            var failResult = Result.Failure(error);

            // Act
            var successOutcome = successResult.ToEndpointOutcome();
            var failOutcome = failResult.ToEndpointOutcome();

            // Assert - Success Outcome
            successOutcome.IsSuccess.Should().BeTrue();
            successOutcome.Value.Should().Be(Unit.Value);
            successOutcome.Errors.Should().BeEmpty();
            successOutcome.Status.Should().Be(ResultStatuses.Ok);
            successOutcome.Metadata.Should().NotBeNull();

            // Assert - Failure Outcome
            failOutcome.IsSuccess.Should().BeFalse();
            failOutcome.Value.Should().Be(Unit.Value);
            failOutcome.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(error);
            failOutcome.Status.Should().Be(ResultStatuses.Error);
            failOutcome.Metadata.Should().NotBeNull();

            // Arrange - For default outcome with mock
            var mockResult = ResultMockHelper.CreateMockFailedResult(ImmutableList<ErrorInfo>.Empty); // Use helper, provide empty list, it will throw, catch and re-mock with default behavior
            // The original test mocked an empty error list and then asserted a single default error.
            // ResultMockHelper.CreateMockFailedResult throws if errors is empty.
            // If the intent is to test the default error creation when Result.Errors is empty,
            // we should directly mock IResult with an empty error list.
            var specificMockResult = new Mock<Zentient.Results.IResult>();
            specificMockResult.SetupGet(r => r.IsSuccess).Returns(false);
            specificMockResult.SetupGet(r => r.Errors).Returns(ImmutableList<ErrorInfo>.Empty);
            specificMockResult.SetupGet(r => r.Status).Returns(ResultStatuses.Error);
            specificMockResult.SetupGet(r => r.ErrorMessage).Returns("fail");

            // Act
            var defaultOutcome = specificMockResult.Object.ToEndpointOutcome();

            // Assert
            defaultOutcome.IsSuccess.Should().BeFalse();
            defaultOutcome.Value.Should().Be(Unit.Value);
            defaultOutcome.Errors.Should().ContainSingle();
            defaultOutcome.Errors[0].Category.Should().Be(ErrorCategory.InternalServerError);
            defaultOutcome.Errors[0].Code.Should().Be("UnknownError");
            defaultOutcome.Errors[0].Message.Should().Be("An unknown error occurred.");
            defaultOutcome.Status.Should().Be(ResultStatuses.Error);
            defaultOutcome.Metadata.Should().NotBeNull();
        }

        [Fact]
        public void ToEndpointOutcome_NonGeneric_Throws_If_Null()
        {
            // Arrange
            Zentient.Results.IResult? result = null;

            // Act
            Action act = () => result!.ToEndpointOutcome();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ToEndpointOutcome_With_TransportMetadata()
        {
            // Arrange
            var genericResult = Result<string>.Success("abc");
            var genericMeta = CreateTransportMetadata(new Dictionary<string, object?> { { "k", "v" } });
            var nonGenericResult = Result.Success();
            var nonGenericMeta = CreateTransportMetadata(new Dictionary<string, object?> { { "k", 123 } });

            // Act
            var genericOutcome = genericResult.ToEndpointOutcome(genericMeta);
            var nonGenericOutcome = nonGenericResult.ToEndpointOutcome(nonGenericMeta);

            // Assert
            genericOutcome.Metadata.Tags.Should().ContainKey("k").WhoseValue.Should().Be("v");
            nonGenericOutcome.Metadata.Tags.Should().ContainKey("k").WhoseValue.Should().Be(123);
        }
    }
}
#pragma warning restore CS1591
