using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Immutable;

using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    public class TransportMetadataTests
    {
        [Fact]
        public void Empty_ShouldBeSingletonWithEmptyTags()
        {
            TransportMetadata empty1 = TransportMetadata.Empty;
            TransportMetadata empty2 = TransportMetadata.Empty;

            empty1.Should().NotBeNull();
            empty1.Tags.Should().BeEmpty();
            empty1.Should().BeSameAs(empty2);
        }

        [Fact]
        public void DefaultConstructor_ShouldYieldEmptyTags()
        {
            TransportMetadata metadata = new TransportMetadata();
            metadata.Tags.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void WithTag_AddsAndUpdatesTags()
        {
            TransportMetadata meta = new TransportMetadata();
            TransportMetadata meta2 = meta.WithTag("foo", 123);
            TransportMetadata meta3 = meta2.WithTag("foo", 456);

            meta2.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(123);
            meta3.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(456);
            meta.Tags.Should().NotContainKey("foo");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithTag_ThrowsOnNullOrWhitespaceKey(string? key)
        {
            TransportMetadata meta = new TransportMetadata();
            Action act = () => meta.WithTag(key!, 1);
            act.Should().Throw<ArgumentException>().WithParameterName(nameof(key));
        }

        [Fact]
        public void WithLogger_AddsLoggerTag()
        {
            ILogger logger = new Mock<ILogger>().Object;
            TransportMetadata meta = new TransportMetadata();
            TransportMetadata meta2 = meta.WithLogger(logger);

            meta2.Tags.Should().ContainKey(Zentient.Endpoints.Constants.MetadataKeys.Logger).WhoseValue.Should().Be(logger);
        }

        [Fact]
        public void WithLogger_ThrowsOnNull()
        {
            TransportMetadata meta = new TransportMetadata();
            Action act = () => meta.WithLogger(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void From_CreatesFromDictionaryAndLogger()
        {
            Dictionary<string, object?> dict = new Dictionary<string, object?> { { "a", 1 }, { "b", "x" } };
            ILogger logger = new Mock<ILogger>().Object;
            TransportMetadata meta = TransportMetadata.From(dict, logger);

            meta.Tags.Should().ContainKey("a").WhoseValue.Should().Be(1);
            meta.Tags.Should().ContainKey("b").WhoseValue.Should().Be("x");
            meta.Tags.Should().ContainKey(Zentient.Endpoints.Constants.MetadataKeys.Logger).WhoseValue.Should().Be(logger);
        }

        [Fact]
        public void From_ThrowsOnNullDictionary()
        {
            Action act = () => TransportMetadata.From(null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("initialTags");
        }

        [Fact]
        public void TryGetTag_ReturnsTrueAndValue_IfPresentAndTyped()
        {
            TransportMetadata meta = new TransportMetadata().WithTag("x", 42);
            meta.TryGetTag<int>("x", out var val).Should().BeTrue();
            val.Should().Be(42);
        }

        [Fact]
        public void TryGetTag_ReturnsFalse_IfMissingOrWrongType()
        {
            TransportMetadata meta = new TransportMetadata().WithTag("x", 42);
            meta.TryGetTag<string>("x", out var str).Should().BeFalse();
            str.Should().BeNull();
            meta.TryGetTag<int>("y", out var missing).Should().BeFalse();
        }

        [Fact]
        public void GetLogger_ReturnsLoggerIfPresent_ElseNull()
        {
            ILogger logger = new Mock<ILogger>().Object;
            TransportMetadata meta = new TransportMetadata().WithLogger(logger);
            meta.GetLogger().Should().Be(logger);

            TransportMetadata meta2 = new TransportMetadata();
            meta2.GetLogger().Should().BeNull();
        }

        [Fact]
        public void ToString_ReturnsExpected_ForEmptyAndNonEmpty()
        {
            TransportMetadata empty = new TransportMetadata();
            empty.ToString().Should().Be("TransportMetadata { Tags: {} }");

            TransportMetadata meta = new TransportMetadata().WithTag("foo", 123);
            meta.ToString().Should().Contain("foo: 123");
        }

        [Fact]
        public void ToString_HandlesProblemDetails_ILogger_Headers_LongString_Null()
        {
            ProblemDetails pd = new ProblemDetails { Type = "type", Title = "title" };
            ILogger logger = new Mock<ILogger>().Object;
            ImmutableDictionary<string, string> headers = ImmutableDictionary<string, string>.Empty.Add("h", "v");
            string longStr = new string('a', 120);

            TransportMetadata meta = new TransportMetadata()
                .WithTag("pd", pd)
                .WithLogger(logger)
                .WithTag("headers", headers)
                .WithTag("long", longStr)
                .WithTag("null", null);

            string str = meta.ToString();
            str.Should().Contain("pd: ProblemDetails (Type: type, Title: title)");
            str.Should().Contain("Logger: ILogger instance");
            str.Should().Contain("headers: Headers (1 items)");
            str.Should().Contain("long: " + new string('a', 97) + "...");
            str.Should().Contain("null: null");
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
