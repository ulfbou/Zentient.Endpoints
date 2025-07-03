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
            value1.Should().Be(value2);
            (value1 == value2).Should().BeTrue();
            (value1 != value2).Should().BeFalse();
        }

        [Fact]
        public void Unit_Equality_Operators_And_Equals()
        {
            Unit a = Unit.Value;
            Unit b = Unit.Value;
            (a == b).Should().BeTrue();
            (a != b).Should().BeFalse();
            a.Equals(b).Should().BeTrue();
            a.Equals((object)b).Should().BeTrue();
            a.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void Unit_CompareTo_Object()
        {
            Unit a = Unit.Value;
            a.CompareTo(Unit.Value).Should().Be(0);
            a.CompareTo(null).Should().Be(1);
            Action act = () => a.CompareTo("not a unit");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Unit_GetHashCode_AlwaysZero()
        {
            Unit.Value.GetHashCode().Should().Be(0);
        }

        [Fact]
        public void Unit_ToString_ReturnsUnit()
        {
            Unit.Value.ToString().Should().Be("Unit");
        }

        [Fact]
        public void Unit_Implements_IEquatable()
        {
            Unit a = Unit.Value;
            a.Equals(Unit.Value).Should().BeTrue();
        }

        [Fact]
        public void Unit_Struct_IsDefaultable()
        {
            Unit def = default;
            def.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_SerializesToEmptyObject()
        {
            _options.Converters.Add(new UnitJsonConverter());
            string json = JsonSerializer.Serialize(Unit.Value, _options);
            json.Should().Be("{}");
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromEmptyObject()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Unit unit = JsonSerializer.Deserialize<Unit>("{}", _options);
            unit.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromNull()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Unit unit = JsonSerializer.Deserialize<Unit>("null", _options);
            unit.Should().Be(Unit.Value);
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnInvalidJson()
        {
            _options.Converters.Add(new UnitJsonConverter());
            Action act1 = () => JsonSerializer.Deserialize<Unit>("42", _options);
            Action act2 = () => JsonSerializer.Deserialize<Unit>("{\"unexpected\":1}", _options);
            act1.Should().Throw<JsonException>();
            act2.Should().Throw<JsonException>();
        }

        [Fact]
        public void UnitJsonConverter_ThrowsOnNullOptionsOrWriter()
        {
            var converter = new UnitJsonConverter();
            using (var writer = new Utf8JsonWriter(new System.Buffers.ArrayBufferWriter<byte>()))
            {
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
                false.Should().BeTrue("Expected ArgumentNullException to be thrown for null options");
            }
            catch (ArgumentNullException ex)
            {
                ex.ParamName.Should().Be("options");
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
