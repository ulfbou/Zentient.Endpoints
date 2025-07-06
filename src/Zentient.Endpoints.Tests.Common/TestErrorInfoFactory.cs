// <copyright file="TestErrorInfoFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A factory for creating <see cref="ErrorInfo"/> instances for testing purposes.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public static class TestErrorInfoFactory
    {
        private static ErrorInfo Create(
            ErrorCategory category,
            string code,
            string message,
            string? detail = null,
            IImmutableDictionary<string, object?>? metadata = null,
            IImmutableList<ErrorInfo>? innerErrors = null)
            => new ErrorInfo(category, code, message, detail, metadata, innerErrors);

        /// <summary>
        /// Creates a validation error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "VALIDATION_ERROR".</param>
        /// <param name="message">The error message. Defaults to "One or more validation errors occurred."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for validation errors.</returns>
        public static ErrorInfo Validation(
            string code = "VALIDATION_ERROR",
            string message = "One or more validation errors occurred.",
            string? detail = null)
            => Create(ErrorCategory.Validation, code, message, detail);

        /// <summary>
        /// Creates a not found error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "RESOURCE_NOT_FOUND".</param>
        /// <param name="message">The error message. Defaults to "The requested resource was not found."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for not found errors.</returns>
        public static ErrorInfo NotFound(
            string code = "RESOURCE_NOT_FOUND",
            string message = "The requested resource was not found.",
            string? detail = null)
            => Create(ErrorCategory.NotFound, code, message, detail);

        /// <summary>
        /// Creates an authentication error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "UNAUTHORIZED_ACCESS".</param>
        /// <param name="message">The error message. Defaults to "Authentication is required or has failed."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for authentication errors.</returns>
        public static ErrorInfo Authentication(
            string code = "UNAUTHORIZED_ACCESS",
            string message = "Authentication is required or has failed.",
            string? detail = null)
            => Create(ErrorCategory.Authentication, code, message, detail);

        /// <summary>
        /// Creates an authorization error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "FORBIDDEN_ACCESS".</param>
        /// <param name="message">The error message. Defaults to "You do not have permission to access this resource."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for authorization errors.</returns>
        public static ErrorInfo Authorization(
            string code = "FORBIDDEN_ACCESS",
            string message = "You do not have permission to access this resource.",
            string? detail = null)
            => Create(ErrorCategory.Authorization, code, message, detail);

        /// <summary>
        /// Creates a conflict error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "RESOURCE_CONFLICT".</param>
        /// <param name="message">The error message. Defaults to "The request could not be completed due to a conflict with the current state of the resource."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for conflict errors.</returns>
        public static ErrorInfo Conflict(
            string code = "RESOURCE_CONFLICT",
            string message = "The request could not be completed due to a conflict with the current state of the resource.",
            string? detail = null)
            => Create(ErrorCategory.Conflict, code, message, detail);

        /// <summary>
        /// Creates an internal server error <see cref="ErrorInfo"/>.
        /// </summary>
        /// <param name="code">The error code. Defaults to "INTERNAL_SERVER_ERROR".</param>
        /// <param name="message">The error message. Defaults to "An unexpected internal server error occurred."</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for internal server errors.</returns>
        public static ErrorInfo InternalServerError(
            string code = "INTERNAL_SERVER_ERROR",
            string message = "An unexpected internal server error occurred.",
            string? detail = null)
            => Create(ErrorCategory.InternalServerError, code, message, detail);

        /// <summary>
        /// Creates a custom <see cref="ErrorInfo"/> with the specified category, code, message, and optional details/metadata/inner errors.
        /// </summary>
        /// <param name="category">The error category.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="detail">Optional detailed information.</param>
        /// <param name="metadata">Optional custom metadata.</param>
        /// <param name="innerErrors">Optional inner errors.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for custom errors.</returns>
        public static ErrorInfo Custom(
            ErrorCategory category,
            string code,
            string message,
            string? detail = null,
            IImmutableDictionary<string, object?>? metadata = null,
            IImmutableList<ErrorInfo>? innerErrors = null)
            => Create(category, code, message, detail, metadata, innerErrors);

        /// <summary>
        /// Creates a new <see cref="ErrorInfo"/> by adding or updating a metadata key/value on an existing error.
        /// </summary>
        /// <param name="baseError">The base <see cref="ErrorInfo"/> to extend.</param>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>A new <see cref="ErrorInfo"/> instance with the added metadata.</returns>
        public static ErrorInfo WithMetadata(ErrorInfo baseError, string key, object? value)
        {
            var newMetadata = baseError?.Metadata?.SetItem(key, value) ?? ImmutableDictionary<string, object?>.Empty.SetItem(key, value);
            return new ErrorInfo(
                baseError?.Category ?? ErrorCategory.General,
                baseError?.Code ?? "UNKNOWN_ERROR",
                baseError?.Message ?? "An error occurred.",
                baseError?.Detail ?? string.Empty,
                newMetadata,
                baseError?.InnerErrors ?? ImmutableList<ErrorInfo>.Empty);
        }

        /// <summary>
        /// Creates a new <see cref="ErrorInfo"/> by adding inner errors to an existing error.
        /// </summary>
        /// <param name="baseError">The base <see cref="ErrorInfo"/> to extend.</param>
        /// <param name="innerErrors">The inner errors to add.</param>
        /// <returns>A new <see cref="ErrorInfo"/> instance with the added inner errors.</returns>
        public static ErrorInfo WithInnerErrors(ErrorInfo baseError, params ErrorInfo[] innerErrors)
        {
            var existingInnerErrors = baseError?.InnerErrors ?? ImmutableList<ErrorInfo>.Empty;
            var combinedInnerErrors = existingInnerErrors.AddRange(innerErrors);
            return new ErrorInfo(
                baseError?.Category ?? ErrorCategory.General,
                baseError?.Code ?? "UNKNOWN_ERROR",
                baseError?.Message ?? "An error occurred.",
                baseError?.Detail ?? string.Empty,
                baseError?.Metadata,
                combinedInnerErrors);
        }

        /// <summary>
        /// Creates an internal server error <see cref="ErrorInfo"/> from an exception.
        /// </summary>
        /// <param name="exception">The exception to derive the error from.</param>
        /// <param name="code">The error code. If null, uses the exception type name.</param>
        /// <param name="message">The error message. If null, uses the exception message.</param>
        /// <returns>An <see cref="ErrorInfo"/> instance for the exception.</returns>
        public static ErrorInfo InternalErrorFromException(
            Exception exception,
            string? code = null,
            string? message = null)
            => ErrorInfo.FromException(exception, message, code);
    }
}
