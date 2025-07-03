// <copyright file="Unit.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Zentient.Endpoints.Serialization;

namespace Zentient.Endpoints
{
    /// <summary>
    /// Represents a void type or the absence of a value.
    /// Useful for representing results of operations that do not return any specific data,
    /// similar to the 'void' keyword but allows for generic type parameters.
    /// </summary>
    /// <remarks>This is a singleton struct to avoid unnecessary allocations.</remarks>
    [DataContract]
    public readonly struct Unit : IEquatable<Unit>, IComparable, IComparable<Unit>
    {
        /// <summary>Gets the singleton instance of <see cref="Unit"/>.</summary>
        /// <value>A singleton instance of <see cref="Unit"/>.</value>
        public static Unit Value { get; }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is less than the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is less than <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <(Unit left, Unit right)
        {
            _ = left;
            _ = right;
            return false;
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is greater than the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is greater than <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >(Unit left, Unit right)
        {
            _ = left;
            _ = right;
            return false;
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is less than or equal to the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is less than or equal to <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator <=(Unit left, Unit right)
        {
            _ = left;
            _ = right;
            return true;
        }

        /// <summary>
        /// Compares two <see cref="Unit"/> instances to determine if the first is greater than or equal to the second.
        /// </summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >=(Unit left, Unit right)
        {
            _ = left;
            _ = right;
            return true;
        }

        /// <summary>Compares two <see cref="Unit"/> instances for equality.</summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is equal to <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(Unit left, Unit right)
            => left.Equals(right);

        /// <summary>Compares two <see cref="Unit"/> instances for inequality.</summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="left"/> is not equal to <paramref name="right"/>;
        /// otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(Unit left, Unit right)
            => !(left == right);

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is Unit;

        /// <inheritdoc />
        public bool Equals(Unit other)
            => true;

        /// <inheritdoc />
        public override int GetHashCode()
            => 0;

        /// <inheritdoc />
        public override string ToString()
            => "Unit";

        /// <inheritdoc />
        public int CompareTo(object? other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other is Unit unit)
            {
                return this.CompareTo(unit);
            }

            throw new ArgumentException($"Object must be of type {nameof(Unit)}.", nameof(other));
        }

        /// <inheritdoc />
        public int CompareTo(Unit other) => 0;
    }
}
