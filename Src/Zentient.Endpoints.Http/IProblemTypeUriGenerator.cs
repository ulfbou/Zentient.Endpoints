// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProblemTypeUriGenerator.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System; // Added for System.Uri

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Defines a contract for generating the 'type' URI for Problem Details (RFC 7807).
    /// This allows applications to define their own URL patterns for problem types.
    /// </summary>
    public interface IProblemTypeUriGenerator
    {
        /// <summary>
        /// Generates the 'type' URI for a given problem code.
        /// </summary>
        /// <param name="errorCode">The specific error code (e.g., "VALIDATION_FAILED", "ITEM_NOT_FOUND").</param>
        /// <returns>A <see cref="Uri"/> representing the full URI for the problem type, or <c>null</c> if no URI can be generated.</returns>
        Uri? GenerateProblemTypeUri(string? errorCode);
    }
}
