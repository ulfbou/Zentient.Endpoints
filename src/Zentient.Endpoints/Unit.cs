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
    /// Represents a void type or the absence of a value.
    /// Useful for representing results of operations that do not return any specific data,
    /// similar to the 'void' keyword but allows for generic type parameters.
    /// </summary>
    /// <remarks>
    /// This is a singleton struct to avoid unnecessary allocations.
    /// </remarks>
    [DataContract]
    public readonly struct Unit : IEquatable<Unit>
    {
        /// <summary>Gets the singleton instance of <see cref="Unit"/>.</summary>
        /// <value>A singleton instance of <see cref="Unit"/>.</value>
        public static Unit Value { get; }

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
            return true;
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

        /// <summary>Compares two <see cref="Unit"/> instances for equality.</summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is equal to <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(Unit left, Unit right)
            => left.Equals(right);

        /// <summary>Compares two <see cref="Unit"/> instances for inequality.</summary>
        /// <param name="left">The first <see cref="Unit"/> to compare.</param>
        /// <param name="right">The second <see cref="Unit"/> to compare.</param>
        /// <returns><see langword="true" /> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <see langword="false" />.</returns>
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
            => "()";

        /// <summary>Compares this instance with another object of the same type.</summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>1 if this instance is greater than the specified object; 0 if they are equal; -1 if this instance is less than the specified object.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified object is not of type <see cref="Unit"/>.</exception>
        /// <remarks>This method is primarily used for compatibility with interfaces that require comparison, such as <see cref="IComparable"/>.</remarks>
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
