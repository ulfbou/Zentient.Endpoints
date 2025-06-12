// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Unit.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Zentient.Endpoints.Core.Serialization;

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Represents a type with a single value, used to indicate the absence of a specific value.
    /// This is equivalent to <c>void</c> in methods that return a value.
    /// It is primarily used in functional programming patterns (e.g., in <c>Result</c> or <c>Option</c> types)
    /// where a value is always expected, but sometimes that value is merely "nothingness" or "success without data".
    /// </summary>
    /// <remarks>
    /// The <see cref="Unit"/> struct is a singleton. Its only value is <see cref="Value"/>.
    /// </remarks>
    [DataContract]
    [JsonConverter(typeof(UnitJsonConverter))]
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        /// <summary>Gets the single instance of the <see cref="Unit"/> struct.</summary>
        /// <value>The singleton instance of <see cref="Unit"/>.</value>
        public static Unit Value { get; }

        /// <summary>
        /// Determines whether two specified <see cref="Unit"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return true; // All Unit instances are equal to each other
        }

        /// <summary>
        /// Determines whether two specified <see cref="Unit"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return false; // All Unit instances are equal, so they are never not equal
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is less than the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return false; // All Unit instances are equal, so none is less than another
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is greater than the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return false; // All Unit instances are equal, so none is greater than another
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is less than or equal to the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <=(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return true; // All Unit instances are equal, so any is less than or equal to another
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is greater than or equal to the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >=(Unit left, Unit right)
        {
            // Discard assignments used to silence IDE0060 for unused parameters in operator overloads.
            _ = left;
            _ = right;
            return true;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Unit;

        /// <inheritdoc />
        public bool Equals(Unit other) => true;

        /// <inheritdoc />
        public override int GetHashCode() => 0;

        /// <inheritdoc />
        public override string ToString() => "()";

        /// <inheritdoc />
        public int CompareTo(Unit other) => 0;

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is Unit)
            {
                return 0;
            }

            throw new ArgumentException($"Object must be of type {nameof(Unit)}.", nameof(obj));
        }
    }
}
