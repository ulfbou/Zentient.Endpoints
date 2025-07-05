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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    public class EndpointOutcomeMetadataExtensionsTests : EndpointTestBase
    {
        // --- WithMetadata (Generic) ---
        [Fact]
        public void WithMetadata_Generic_ThrowsOnNulls()
        {
            IEndpointOutcome<string>? outcome = null;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            Action act1 = () => outcome!.WithMetadata(transform);
            Action act2 = () => EndpointOutcome<string>.Success("abc").WithMetadata(null!);
            act1.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
            act2.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
        }

        [Fact]
        public void WithMetadata_Generic_ThrowsOnNonConcreteType()
        {
            var mock = new Mock<IEndpointOutcome<string>>();
            mock.SetupGet(x => x.Value).Returns("abc");
            mock.SetupGet(x => x.Metadata).Returns(new TransportMetadata());
            IEndpointOutcome<string> outcome = mock.Object;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            Action act = () => outcome.WithMetadata(transform);
            act.Should().Throw<InvalidOperationException>().WithMessage("*EndpointOutcome*");
        }

        [Fact]
        public void WithMetadata_Generic_AppliesTransform()
        {
            IEndpointOutcome<string> original = EndpointOutcome<string>.Success("abc");
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("foo", 42);
            var updated = original.WithMetadata(transform);
            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(42);
            original.Metadata.Tags.Should().NotContainKey("foo");
            updated.Value.Should().Be("abc");
        }

        // --- WithMetadata (Non-Generic) ---
        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNulls()
        {
            IEndpointOutcome? outcome = null;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            Action act1 = () => outcome!.WithMetadata(transform);
            Action act2 = () => EndpointOutcome.Success().WithMetadata(null!);
            act1.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
            act2.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
        }

        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNonConcreteType()
        {
            var mock = new Mock<IEndpointOutcome>();
            mock.SetupGet(x => x.Metadata).Returns(new TransportMetadata());
            IEndpointOutcome outcome = mock.Object;
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            Action act = () => outcome.WithMetadata(transform);
            act.Should().Throw<InvalidOperationException>().WithMessage("*EndpointOutcome*");
        }

        [Fact]
        public void WithMetadata_NonGeneric_AppliesTransform()
        {
            IEndpointOutcome original = EndpointOutcome.Success();
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("bar", 99);
            var updated = original.WithMetadata(transform);
            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("bar").WhoseValue.Should().Be(99);
            original.Metadata.Tags.Should().NotContainKey("bar");
        }

        // --- OutcomeEquals ---
        [Fact]
        public void OutcomeEquals_ReferenceEquals_And_Identical()
        {
            var o1 = EndpointOutcome<int>.Success(5);
            o1.OutcomeEquals(o1).Should().BeTrue();

            var o2 = EndpointOutcome<int>.Success(5);
            o1.OutcomeEquals(o2).Should().BeTrue();
        }

        [Theory]
        [InlineData(5, 6)]
        [InlineData(42, 43)]
        public void OutcomeEquals_DifferentValues(int v1, int v2)
        {
            var o1 = EndpointOutcome<int>.Success(v1);
            var o2 = EndpointOutcome<int>.Success(v2);
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentStatus()
        {
            var status1 = CreateMockResultStatus(200, "OK");
            var status2 = CreateMockResultStatus(201, "Created");
            var o1 = EndpointOutcome<int>.Success(5, status1);
            var o2 = EndpointOutcome<int>.Success(5, status2);
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentMetadata()
        {
            var m1 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } });
            var m2 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 2 } });
            var o1 = EndpointOutcome<int>.Success(5, m1);
            var o2 = EndpointOutcome<int>.Success(5, m2);
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_DifferentErrors()
        {
            var e1 = CreateErrorInfo(code: "E1", message: "err1");
            var e2 = CreateErrorInfo(code: "E2", message: "err2");
            var o1 = EndpointOutcome<int>.FromError(e1);
            var o2 = EndpointOutcome<int>.FromError(e2);
            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_NullCases()
        {
            var o1 = EndpointOutcome<int>.Success(5);
            IEndpointOutcome<int>? o2 = null;
            o1.OutcomeEquals(o2!).Should().BeFalse();
        }

        // --- ToHttpResult ---
        [Fact]
        public async Task ToHttpResult_ThrowsOnNulls()
        {
            IEndpointOutcome? outcome = null;
            var ctx = new DefaultHttpContext();
            Func<Task> act1 = () => outcome!.ToHttpResult(ctx);
            Func<Task> act2 = () => EndpointOutcome<string>.Success("abc").ToHttpResult(null!);
            await act1.Should().ThrowAsync<ArgumentNullException>().WithParameterName("endpointResult");
            await act2.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task ToHttpResult_ThrowsIfMapperNotRegistered()
        {
            var ctx = new DefaultHttpContext();
            ctx.RequestServices = new ServiceCollection().BuildServiceProvider();
            var outcome = EndpointOutcome<string>.Success("abc");
            Func<Task> act = () => outcome.ToHttpResult(ctx);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No service for type*IEndpointOutcomeToHttpMapper*");
        }

        [Fact]
        public async Task ToHttpResult_DelegatesToMapper()
        {
            var mapperMock = new Mock<IEndpointOutcomeToHttpMapper>();
            var ctx = new DefaultHttpContext();
            ctx.RequestServices = new ServiceCollection().AddSingleton(mapperMock.Object).BuildServiceProvider();
            var outcome = EndpointOutcome<string>.Success("abc");
            var expectedResult = Microsoft.AspNetCore.Http.Results.Ok("result");
            mapperMock.Setup(m => m.Map(outcome, ctx, It.IsAny<CancellationToken>())).ReturnsAsync(expectedResult);

            var result = await outcome.ToHttpResult(ctx);

            result.Should().BeSameAs(expectedResult);
            mapperMock.Verify(m => m.Map(outcome, ctx, It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- ToMinimalApiResult ---
        [Fact]
        public void ToMinimalApiResult_ReturnsSameInstance_And_ThrowsOnNull()
        {
            var outcome = EndpointOutcome<int>.Success(42);
            outcome.ToMinimalApiResult().Should().BeSameAs(outcome);

            IEndpointOutcome<string>? nullOutcome = null;
            Action act = () => nullOutcome!.ToMinimalApiResult();
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        // --- ToEndpointOutcome (Generic/Non-Generic) ---
        [Fact]
        public void ToEndpointOutcome_Generic_Success_And_Failure()
        {
            var messages = new[] { "msg1", "msg2" };
            var result = Zentient.Results.Result<string>.Success("data", ResultStatuses.Ok, messages);
            var outcome = result.ToEndpointOutcome();
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be("data");
            outcome.Messages.Should().BeEquivalentTo(messages);
            outcome.Status.Should().Be(ResultStatuses.Ok);
            outcome.Errors.Should().BeEmpty();
            outcome.Metadata.Should().NotBeNull();

            var error = new ErrorInfo(ErrorCategory.Validation, "VAL", "Validation failed");
            var failResult = Zentient.Results.Result<string>.Failure(errors: new[] { error }, status: ResultStatuses.BadRequest);
            var failOutcome = failResult.ToEndpointOutcome();
            failOutcome.IsSuccess.Should().BeFalse();
            failOutcome.Value.Should().BeNull();
            failOutcome.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(error);
            failOutcome.Status.Should().Be(ResultStatuses.BadRequest);
            failOutcome.Metadata.Should().NotBeNull();
        }

        [Fact]
        public void ToEndpointOutcome_Generic_Throws_If_Null()
        {
            Zentient.Results.IResult<string>? result = null;
            Action act = () => result!.ToEndpointOutcome();
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ToEndpointOutcome_NonGeneric_Success_And_Failure()
        {
            var result = Zentient.Results.Result.Success();
            var outcome = result.ToEndpointOutcome();
            outcome.IsSuccess.Should().BeTrue();
            outcome.Value.Should().Be(Unit.Value);
            outcome.Errors.Should().BeEmpty();
            outcome.Status.Should().Be(ResultStatuses.Ok);
            outcome.Metadata.Should().NotBeNull();

            var error = new ErrorInfo(ErrorCategory.Conflict, "C", "conflict");
            var failResult = Zentient.Results.Result.Failure(error);
            var failOutcome = failResult.ToEndpointOutcome();
            failOutcome.IsSuccess.Should().BeFalse();
            failOutcome.Value.Should().Be(Unit.Value);
            failOutcome.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(error);
            failOutcome.Status.Should().Be(ResultStatuses.Error);
            failOutcome.Metadata.Should().NotBeNull();

            var mockResult = new Mock<Zentient.Results.IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(false);
            mockResult.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>().AsReadOnly());
            mockResult.SetupGet(r => r.Status).Returns(ResultStatuses.Error);
            mockResult.SetupGet(r => r.ErrorMessage).Returns("fail");
            var defaultOutcome = mockResult.Object.ToEndpointOutcome();
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
            Zentient.Results.IResult? result = null;
            Action act = () => result!.ToEndpointOutcome();
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ToEndpointOutcome_With_TransportMetadata()
        {
            var result = Zentient.Results.Result<string>.Success("abc");
            var meta = Zentient.Endpoints.TransportMetadata.From(new Dictionary<string, object?> { { "k", "v" } });
            var outcome = result.ToEndpointOutcome(meta);
            outcome.Metadata.Tags.Should().ContainKey("k").WhoseValue.Should().Be("v");

            var nonGenericResult = Zentient.Results.Result.Success();
            var meta2 = Zentient.Endpoints.TransportMetadata.From(new Dictionary<string, object?> { { "k", 123 } });
            var outcome2 = nonGenericResult.ToEndpointOutcome(meta2);
            outcome2.Metadata.Tags.Should().ContainKey("k").WhoseValue.Should().Be(123);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
