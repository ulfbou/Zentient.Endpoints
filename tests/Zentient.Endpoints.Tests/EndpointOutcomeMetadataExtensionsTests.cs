using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Moq;

using Xunit;

using Zentient.Results;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    internal class EndpointOutcomeMetadataExtensionsTests : EndpointTestBase
    {
        [Fact]
        public void WithMetadata_Generic_ThrowsOnNullOutcome()
        {
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            IEndpointOutcome<string> outcome = null!;
            Action act = () => outcome.WithMetadata(transform);
            act.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
        }

        [Fact]
        public void WithMetadata_Generic_ThrowsOnNullTransform()
        {
            IEndpointOutcome<string> outcome = EndpointOutcome<string>.Success("abc");
            Action act = () => outcome.WithMetadata(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
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
        public void WithMetadata_Generic_AppliesTransformAndReturnsNewInstance()
        {
            IEndpointOutcome<string> original = EndpointOutcome<string>.Success("abc");
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("foo", 42);

            IEndpointOutcome<string> updated = original.WithMetadata(transform);

            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(42);
            original.Metadata.Tags.Should().NotContainKey("foo");
            updated.Value.Should().Be("abc");
        }

        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNullOutcome()
        {
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("x", 1);
            IEndpointOutcome outcome = null!;
            Action act = () => outcome.WithMetadata(transform);
            act.Should().Throw<ArgumentNullException>().WithParameterName("outcome");
        }

        [Fact]
        public void WithMetadata_NonGeneric_ThrowsOnNullTransform()
        {
            IEndpointOutcome outcome = EndpointOutcome.Success();
            Action act = () => outcome.WithMetadata(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("metadataTransform");
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
        public void WithMetadata_NonGeneric_AppliesTransformAndReturnsNewInstance()
        {
            IEndpointOutcome original = EndpointOutcome.Success();
            Func<TransportMetadata, TransportMetadata> transform = m => m.WithTag("bar", 99);

            IEndpointOutcome updated = original.WithMetadata(transform);

            updated.Should().NotBeSameAs(original);
            updated.Metadata.Tags.Should().ContainKey("bar").WhoseValue.Should().Be(99);
            original.Metadata.Tags.Should().NotContainKey("bar");
        }

        [Fact]
        public void OutcomeEquals_ReturnsTrue_ForIdenticalOutcomes()
        {
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5);
            IEndpointOutcome<int> o2 = EndpointOutcome<int>.Success(5);

            o1.OutcomeEquals(o2).Should().BeTrue();
        }

        [Fact]
        public void OutcomeEquals_ReturnsFalse_ForDifferentValues()
        {
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5);
            IEndpointOutcome<int> o2 = EndpointOutcome<int>.Success(6);

            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_ReturnsFalse_ForDifferentStatus()
        {
            IResultStatus status1 = CreateMockResultStatus(200, "OK");
            IResultStatus status2 = CreateMockResultStatus(201, "Created");
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5, status1);
            IEndpointOutcome<int> o2 = EndpointOutcome<int>.Success(5, status2);

            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_ReturnsFalse_ForDifferentMetadata()
        {
            TransportMetadata m1 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 1 } });
            TransportMetadata m2 = CreateTransportMetadata(new Dictionary<string, object?> { { "a", 2 } });
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5, m1);
            IEndpointOutcome<int> o2 = EndpointOutcome<int>.Success(5, m2);

            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_ReturnsFalse_ForDifferentErrors()
        {
            ErrorInfo e1 = CreateErrorInfo(code: "E1", message: "err1");
            ErrorInfo e2 = CreateErrorInfo(code: "E2", message: "err2");
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.FromError(e1);
            IEndpointOutcome<int> o2 = EndpointOutcome<int>.FromError(e2);

            o1.OutcomeEquals(o2).Should().BeFalse();
        }

        [Fact]
        public void OutcomeEquals_ReturnsTrue_ForReferenceEquals()
        {
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5);
            o1.OutcomeEquals(o1).Should().BeTrue();
        }

        [Fact]
        public void OutcomeEquals_ReturnsFalse_IfEitherIsNull()
        {
            IEndpointOutcome<int> o1 = EndpointOutcome<int>.Success(5);
            IEndpointOutcome<int> o2 = null!;
            o1.OutcomeEquals(o2).Should().BeFalse();
            o2.OutcomeEquals(o1).Should().BeFalse();
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
