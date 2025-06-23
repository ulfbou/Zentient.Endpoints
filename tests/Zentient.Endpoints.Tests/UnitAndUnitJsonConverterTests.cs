// <copyright file="UnitAndUnitJsonConverterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Xunit;
using Zentient.Endpoints;
using Zentient.Endpoints.Serialization;

namespace Zentient.Endpoints.Http.Tests
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public sealed class UnitAndUnitJsonConverterTests
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions();

        [Fact]
        public void Unit_Value_IsSingleton()
        {
            Unit value1 = Unit.Value;
            Unit value2 = Unit.Value;
            value1.ShouldBeEqualTo(value2);
        }

        [Fact]
        public void Unit_Equality_Operators_AlwaysTrue()
        {
            Unit a = Unit.Value;
            Unit b = Unit.Value;
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
        }

        [Fact]
        public void Unit_Comparison_Operators()
        {
            Unit a = Unit.Value;
            Unit b = Unit.Value;
            Assert.False(a < b);
            Assert.False(a > b);
            Assert.True(a <= b);
            Assert.True(a >= b);
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
        public void Unit_CompareTo_Object()
        {
            Unit a = Unit.Value;
            Assert.Equal(0, a.CompareTo(Unit.Value));
            Assert.Equal(1, a.CompareTo(null));
            Assert.Throws<ArgumentException>(() => a.CompareTo("not a unit"));
        }

        [Fact]
        public void UnitJsonConverter_SerializesToEmptyObject()
        {

            _options.Converters.Add(new UnitJsonConverter());

            var json = JsonSerializer.Serialize(Unit.Value, _options);
            Assert.Equal("{}", json);
        }

        [Fact]
        public void UnitJsonConverter_DeserializesFromEmptyObject()
        {
            _options.Converters.Add(new UnitJsonConverter());

            Unit unit = JsonSerializer.Deserialize<Unit>("{}", this._options);
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
    }

    // Helper for equality assertion (since Unit is a struct, can use Assert.Equal, but for clarity)
    internal static class UnitTestExtensions
    {
        public static void ShouldBeEqualTo(this Unit actual, Unit expected)
        {
            Assert.Equal(expected, actual);
            Assert.True(actual == expected);
            Assert.False(actual != expected);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
