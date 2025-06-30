// <copyright file="ErrorInfoHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Zentient.Results;
using Zentient.Results.Constants;

namespace Zentient.Endpoints.Tests.Shared
{
    /// <summary>
    /// Provides helper methods for creating <see cref="ErrorInfo"/> instances for testing purposes.
    /// </summary>
    public static class ErrorInfoHelper
    {
        /// <summary>
        /// Creates an <see cref="ErrorInfo"/> instance with common properties.
        /// </summary>
        /// <param name="category">The error category.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="detail">Optional detail message.</param>
        /// <param name="metadata">Optional metadata dictionary.</param>
        /// <param name="innerErrors">Optional list of inner errors.</param>
        /// <returns>
        /// An <see cref="ErrorInfo"/> instance initialized with the specified parameters.
        /// </returns>
        public static ErrorInfo CreateErrorInfo(
            ErrorCategory category = ErrorCategory.General,
            string code = ErrorCodes.General,
            string message = "Test Error",
            string? detail = null,
            IImmutableDictionary<string, object?>? metadata = null,
            IImmutableList<ErrorInfo>? innerErrors = null)
        {
            return new ErrorInfo(category, code, message, detail, metadata, innerErrors);
        }
    }
}
