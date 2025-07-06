// <copyright file="UnitAndUnitJsonConverterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Buffers;

using FluentAssertions;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ReSharper disable EqualExpressionComparison // Suppress redundant equality comparisons in tests where intentional
namespace Zentient.Endpoints.Tests
{
    public sealed class UnitAndUnitJsonConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public UnitAndUnitJsonConverterTests()
        {
            _options = new JsonSerializerOptions();
        }

        [Fact]
        public void Unit_Value_IsSingleton()
        {
            // Arrange & Act
            Unit value1 = Unit.Value;
            Unit value2 = Unit.Value;

            // Assert
            value1.Should().Be(value2);
            (value1 == value2).Should().BeTrue();
            (value1 != value2).Should().BeFalse();
        }

        [Fact]
        public void Unit_Equality_Operators_And_Equals()
        {
            // Arrange
            Unit a = Unit.Value;
            Unit b = Unit.Value;
            Unit? c = null;

            // Act & Assert
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Equals(b).Should().BeTrue();
            a.Equals((object)b).Should().BeTrue();

            // Suppress CA1508 here if it's explicitly testing that Equals(null) is false, which is a valid contract test.
#pragma warning disable CA1508 // Avoid dead conditional code
            a.Equals(c).Should().BeFalse(); // Test Unit.Equals(Unit? other) with null
            a.Equals((object?)c).Should().BeFalse(); // Test Unit.Equals(object? obj) with null
#pragma warning restore CA1508 // Avoid dead conditional code
        }

        [Fact]
        public void Unit_CompareTo_Object()
        {
            // Arrange
            Unit a = Unit.Value;
            object? nullObject = null;
            string notAUnit = "not a unit";

            // Act & Assert
            a.CompareTo(Unit.Value).Should().Be(0);
            a.CompareTo(nullObject).Should().Be(1); // Unit is greater than null

            Action act = () => a.CompareTo(notAUnit);

            act.Should().Throw<ArgumentException>()
               .WithMessage("Object must be of type Unit. (Parameter 'other')"); // Verify specific message
        }

        [Fact]
        public void Unit_GetHashCode_AlwaysZero()
        {
            // Arrange & Act & Assert
            Unit.Value.GetHashCode().Should().Be(0);
        }

        [Fact]
        public void Unit_ToString_ReturnsUnit()
        {
            // Arrange & Act & Assert
            Unit.Value.ToString().Should().Be("Unit");
        }

        [Fact]
        public void Unit_Implements_IEquatable()
        {
            // Arrange
            Unit a = Unit.Value;

            // Act & Assert
            // This test is somewhat redundant given Unit_Equality_Operators_And_Equals,
            // but explicitly checks IEquatable<Unit>.Equals
            a.Equals(Unit.Value).Should().BeTrue();
        }

        [Fact]
        public void Unit_Struct_IsDefaultable()
        {
            // Arrange & Act
            Unit def = default;

            // Assert
            def.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_SerializesToEmptyObject()
        {
            // Arrange
            _options.Converters.Add(new UnitJsonConverter());

            // Act
            string json = JsonSerializer.Serialize(Unit.Value, _options);

            // Assert
            json.Should().Be("{}");
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromEmptyObject()
        {
            // Arrange
            _options.Converters.Add(new UnitJsonConverter());
            string json = "{}";

            // Act
            Unit unit = JsonSerializer.Deserialize<Unit>(json, _options);

            // Assert
            unit.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromNull()
        {
            // Arrange
            _options.Converters.Add(new UnitJsonConverter());
            string json = "null";

            // Act
            Unit unit = JsonSerializer.Deserialize<Unit>(json, _options);

            // Assert
            unit.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnInvalidJson()
        {
            // Arrange
            _options.Converters.Add(new UnitJsonConverter());

            // Test case 1: Primitive value (not object or null)
            string json1 = "42";
            Action act1 = () => JsonSerializer.Deserialize<Unit>(json1, _options);
            act1.Should().Throw<JsonException>()
                // This message is common when the first token is wrong
                .WithMessage("Expected object or null for Unit.");

            // Test case 2: Object with unexpected content or malformed structure
            string json2 = "{\"unexpected\":1}";
            Action act2 = () => JsonSerializer.Deserialize<Unit>(json2, _options);
            act2.Should().Throw<JsonException>()
                // This message suggests it started reading an object, but then failed
                // to find the expected end (likely because it expects an empty object).
                .WithMessage("Expected end of object for Unit.");

            // Test case 3: Array (not object or null)
            string json3 = "[]";
            Action act3 = () => JsonSerializer.Deserialize<Unit>(json3, _options);
            act3.Should().Throw<JsonException>()
                // Similar to json1, the first token is wrong
                .WithMessage("Expected object or null for Unit.");
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnNullWriterOrOptionsForWrite()
        {
            // Arrange
            var converter = new UnitJsonConverter();
            // ArrayBufferWriter is not IDisposable, so remove 'using'
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                // Act & Assert for null writer
                Action writeNullWriter = () => converter.Write(null!, Unit.Value, new JsonSerializerOptions());
                writeNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");

                // Act & Assert for null options
                Action writeNullOptions = () => converter.Write(writer, Unit.Value, null!);
                writeNullOptions.Should().Throw<ArgumentNullException>().WithParameterName("options");
            }
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnNullOptionsForRead()
        {
            // Arrange
            var converter = new UnitJsonConverter();
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes("{}");
            var reader = new Utf8JsonReader(jsonBytes); // Reader is a struct, create directly

            // Act & Assert
            // Directly call the method within a try-catch block to assert the exception
            try
            {
                // Must pass a ref local to the method, so copy the reader
                var tempReader = reader; // Create a mutable copy for the 'ref' parameter
                converter.Read(ref tempReader, typeof(Unit), null!);
                // If no exception is thrown, the test should fail
                Assert.Fail("Expected ArgumentNullException to be thrown.");
            }
            catch (ArgumentNullException ex)
            {
                ex.ParamName.Should().Be("options");
            }
        }

        // It is not possible to test passing a 'null' Utf8JsonReader to a 'ref' parameter
        // as Utf8JsonReader is a struct and cannot be null. Testing default(Utf8JsonReader)
        // would require a separate test for deserialization failures due to an invalid reader state.
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
// ReSharper restore EqualExpressionComparison // Restore warning if not needed globally
