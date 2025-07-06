// <copyright file="TransportMetadataTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace Zentient.Endpoints.Tests
{
    public class TransportMetadataTests
    {
        [Fact]
        public void Empty_ShouldBeSingletonWithEmptyTags()
        {
            // Arrange & Act
            TransportMetadata empty1 = TransportMetadata.Empty;
            TransportMetadata empty2 = TransportMetadata.Empty;

            // Assert
            empty1.Should().NotBeNull();
            empty1.Tags.Should().BeEmpty();
            empty1.Should().BeSameAs(empty2);
        }

        [Fact]
        public void DefaultConstructor_ShouldYieldEmptyTags()
        {
            // Arrange & Act
            TransportMetadata metadata = new TransportMetadata();

            // Assert
            metadata.Tags.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void WithTag_AddsAndUpdatesTags()
        {
            // Arrange
            TransportMetadata meta = new TransportMetadata();

            // Act
            TransportMetadata meta2 = meta.WithTag("foo", 123);
            TransportMetadata meta3 = meta2.WithTag("foo", 456);

            // Assert
            meta2.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(123);
            meta3.Tags.Should().ContainKey("foo").WhoseValue.Should().Be(456);
            meta.Tags.Should().NotContainKey("foo");
            meta2.Should().NotBeSameAs(meta);
            meta3.Should().NotBeSameAs(meta2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithTag_ThrowsOnNullOrWhitespaceKey(string? key)
        {
            // Arrange
            TransportMetadata meta = new TransportMetadata();

            // Act
            Action act = () => meta.WithTag(key!, 1);

            // Assert
            act.Should().Throw<ArgumentException>().WithParameterName(nameof(key));
        }

        [Fact]
        public void WithLogger_AddsLoggerTag()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            TransportMetadata meta = new TransportMetadata();

            // Act
            TransportMetadata meta2 = meta.WithLogger(logger);

            // Assert
            meta2.Tags.Should().ContainKey(Zentient.Endpoints.Constants.MetadataKeys.Logger).WhoseValue.Should().Be(logger);
            meta2.Should().NotBeSameAs(meta);
        }

        [Fact]
        public void WithLogger_ThrowsOnNull()
        {
            // Arrange
            TransportMetadata meta = new TransportMetadata();
            ILogger? nullLogger = null;

            // Act
            Action act = () => meta.WithLogger(nullLogger!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void From_CreatesFromDictionaryAndLogger()
        {
            // Arrange
            Dictionary<string, object?> dict = new Dictionary<string, object?> { { "a", 1 }, { "b", "x" } };
            ILogger logger = new Mock<ILogger>().Object;

            // Act
            TransportMetadata meta = TransportMetadata.From(dict, logger);

            // Assert
            meta.Tags.Should().ContainKey("a").WhoseValue.Should().Be(1);
            meta.Tags.Should().ContainKey("b").WhoseValue.Should().Be("x");
            meta.Tags.Should().ContainKey(Zentient.Endpoints.Constants.MetadataKeys.Logger).WhoseValue.Should().Be(logger);
        }

        [Fact]
        public void From_ThrowsOnNullDictionary()
        {
            // Arrange
            IDictionary<string, object?>? nullDict = null;

            // Act
            Action act = () => TransportMetadata.From(nullDict!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("initialTags");
        }

        [Fact]
        public void TryGetTag_ReturnsTrueAndValue_IfPresentAndTyped()
        {
            // Arrange
            TransportMetadata meta = new TransportMetadata().WithTag("x", 42);

            // Act
            bool result = meta.TryGetTag<int>("x", out var val);

            // Assert
            result.Should().BeTrue();
            val.Should().Be(42);
        }

        [Fact]
        public void TryGetTag_ReturnsFalse_IfMissingOrWrongType()
        {
            // Arrange
            TransportMetadata meta = new TransportMetadata().WithTag("x", 42);

            // Act & Assert for wrong type
            meta.TryGetTag<string>("x", out var str).Should().BeFalse();
            str.Should().BeNull();

            // Act & Assert for missing key
            meta.TryGetTag<int>("y", out var missing).Should().BeFalse();
            missing.Should().Be(0);
        }

        [Fact]
        public void GetLogger_ReturnsLoggerIfPresent_ElseNull()
        {
            // Arrange
            ILogger logger = new Mock<ILogger>().Object;
            TransportMetadata metaWithLogger = new TransportMetadata().WithLogger(logger);
            TransportMetadata metaWithoutLogger = new TransportMetadata();

            // Act & Assert for logger present
            metaWithLogger.GetLogger().Should().Be(logger);

            // Act & Assert for logger absent
            metaWithoutLogger.GetLogger().Should().BeNull();
        }

        [Fact]
        public void ToString_ReturnsExpected_ForEmptyAndNonEmpty()
        {
            // Arrange
            TransportMetadata empty = new TransportMetadata();
            TransportMetadata meta = new TransportMetadata().WithTag("foo", 123);

            // Act & Assert for empty
            empty.ToString().Should().Be("TransportMetadata { Tags: {} }");

            // Act & Assert for non-empty
            string nonEmptyToString = meta.ToString();
            nonEmptyToString.Should().StartWith("TransportMetadata { Tags: {")
                            .And.Contain("foo: 123")
                            .And.EndWith("} }");
        }

        [Fact]
        public void ToString_HandlesProblemDetails_ILogger_Headers_LongString_Null()
        {
            // Arrange
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

            // Act
            string str = meta.ToString();

            // Assert
            str.Should().Contain("pd: ProblemDetails (Type: type, Title: title)");
            str.Should().Contain("Logger: ILogger instance");
            str.Should().Contain("headers: Headers (1 items)");
            str.Should().Contain("long: " + new string('a', 97) + "...");
            str.Should().Contain("null: null");
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1707 // Identifiers should not contain underscores
