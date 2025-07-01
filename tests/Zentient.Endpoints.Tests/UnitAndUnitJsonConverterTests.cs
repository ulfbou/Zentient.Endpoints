// <copyright file="UnitAndUnitJsonConverterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

using FluentAssertions;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Serialization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests
{
    public sealed class UnitAndUnitJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions();

        [Fact]
        public void Unit_Value_IsSingleton()
        {
            Unit value1 = Unit.Value;
            Unit value2 = Unit.Value;
            Assert.Equal(value1, value2);
            Assert.True(value1 == value2);
            Assert.False(value1 != value2);
        }

        [Fact]
        public void Unit_Equality_Operators_And_Equals()
        {
            Unit a = Unit.Value;
            Unit b = Unit.Value;
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.False(a.Equals(null));
        }

        [Fact]
        public void Unit_CompareTo_Object()
        {
            Unit a = Unit.Value;
            Assert.Equal(0, a.CompareTo(Unit.Value));
            Assert.Equal(1, a.CompareTo(null));
            Assert.Throws<ArgumentException>(() => a.CompareTo("not a unit"));
        }

        [Fact]
        public void Unit_GetHashCode_AlwaysZero()
        {
            Assert.Equal(0, Unit.Value.GetHashCode());
        }

        [Fact]
        public void Unit_ToString_ReturnsUnit()
        {
            Assert.Equal("Unit", Unit.Value.ToString());
        }

        [Fact]
        public void Unit_Implements_IEquatable()
        {
            Unit a = Unit.Value;
            a.Equals(Unit.Value).ShouldBeTrue();
        }

        [Fact]
        public void Unit_Struct_IsDefaultable()
        {
            Unit def = default;
            Assert.Equal(Unit.Value, def);
        }

        [Fact]
        public void UnitJsonConverter_SerializesToEmptyObject()
        {
            _options.Converters.Add(new UnitJsonConverter());
            string json = JsonSerializer.Serialize(Unit.Value, _options);
            Assert.Equal("{}", json);
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromEmptyObject()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Unit unit = JsonSerializer.Deserialize<Unit>("{}", _options);
            Assert.Equal(Unit.Value, unit);
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromNull()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Unit unit = JsonSerializer.Deserialize<Unit>("null", _options);
            Assert.Equal(Unit.Value, unit);
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnInvalidJson()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Unit>("42", _options));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Unit>("{\"unexpected\":1}", _options));
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnNullOptionsOrWriter()
        {
            var converter = new UnitJsonConverter();
            using (var writer = new Utf8JsonWriter(new System.Buffers.ArrayBufferWriter<byte>()))
            {
                // FluentAssertions for exception checks
                Action writeNullWriter = () => converter.Write(null!, Unit.Value, new JsonSerializerOptions());
                writeNullWriter.Should().Throw<ArgumentNullException>().WithParameterName("writer");

                Action writeNullOptions = () => converter.Write(writer, Unit.Value, null!);
                writeNullOptions.Should().Throw<ArgumentNullException>().WithParameterName("options");
            }

            // CA1825: Use Array.Empty<byte>() instead of new byte[] { }
            var reader = new Utf8JsonReader(Array.Empty<byte>());

            // CS8175: Avoid ref in lambda/anonymous method: call directly and assert
            try
            {
                converter.Read(ref reader, typeof(Unit), null!);
                Assert.Fail("Expected ArgumentNullException was not thrown.");
            }
            catch (ArgumentNullException ex)
            {
                ex.ParamName.Should().Be("options");
            }
        }
    }

    internal static class UnitTestExtensions
    {
        public static void ShouldBeTrue(this bool value)
        {
            Assert.True(value);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
