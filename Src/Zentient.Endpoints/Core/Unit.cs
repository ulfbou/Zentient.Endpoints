// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Unit.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System; // For IEquatable, IComparable

namespace Zentient.Endpoints.Core
{
    /// <summary>
    /// Represents a type with a single, unique value.
    /// This is used to indicate the absence of a meaningful return value
    /// in generic functional programming contexts, similar to void but
    /// allowing for type consistency in generic results (e.g., EndpointResult&lt;Unit&gt;).
    /// </summary>
    /// <remarks>
    /// The Unit type is a common construct in functional programming to represent
    /// operations that produce no observable result, effectively acting as a
    /// type-safe alternative to void in generic contexts.
    /// </remarks>
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
    {
        /// <summary>
        /// Gets the single instance of the <see cref="Unit"/> type.
        /// </summary>
        public static readonly Unit Value = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Unit"/> struct.
        /// </summary>
        public Unit()
        {
        }

        /// <summary>
        /// Implements the equality operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="true"/>.</returns>
        public static bool operator ==(Unit left, Unit right)
            => true;

        /// <summary>
        /// Implements the inequality operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="false"/>.</returns>
        public static bool operator !=(Unit left, Unit right)
            => false;

        /// <summary>
        /// Implements the less than operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="false"/>.</returns>
        public static bool operator <(Unit left, Unit right)
            => false;

        /// <summary>
        /// Implements the less than or equal operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="true"/>.</returns>
        public static bool operator <=(Unit left, Unit right)
            => true;

        /// <summary>
        /// Implements the greater than operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="false"/>.</returns>
        public static bool operator >(Unit left, Unit right)
            => false;

        /// <summary>
        /// Implements the greater than or equal operator for <see cref="Unit"/> instances.
        /// </summary>
        /// <param name="left">The left <see cref="Unit"/> instance.</param>
        /// <param name="right">The right <see cref="Unit"/> instance.</param>
        /// <returns><see langword="true"/>.</returns>
        public static bool operator >=(Unit left, Unit right)
            => true;

        /// <summary>
        /// Returns a string representation of the <see cref="Unit"/> value.
        /// </summary>
        /// <returns>A string representing the <see cref="Unit"/> value (always "()").</returns>
        public override string ToString()
            => "()";

        /// <summary>
        /// Determines whether the current <see cref="Unit"/> instance is equal to another <see cref="Unit"/> instance.
        /// As there is only one possible value for <see cref="Unit"/>, all instances are considered equal.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the other object is a <see cref="Unit"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Unit other)
            => true;

        /// <summary>
        /// Determines whether the current <see cref="Unit"/> instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> if the specified object is a <see cref="Unit"/>; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj)
            => obj is Unit;

        /// <summary>
        /// Returns the hash code for this <see cref="Unit"/> instance.
        /// As there is only one possible value for <see cref="Unit"/>, all instances have the same hash code.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
            => 0;

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.
        /// As there is only one possible value for <see cref="Unit"/>, this method always returns 0.</returns>
        public int CompareTo(Unit other)
            => 0;
    }
}
