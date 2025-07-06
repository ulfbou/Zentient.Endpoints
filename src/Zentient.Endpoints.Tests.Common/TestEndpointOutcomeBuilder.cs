// <copyright file="TestEndpointOutcomeBuilder{TValue}.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Zentient.Endpoints;
using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Convenience builder for <see cref="Unit"/> outcomes and a general entry point for any TValue.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public static class TestEndpointOutcomeBuilder
    {
        /// <summary>
        /// Convenience method for building an outcome with Unit as the value type.
        /// </summary>
        /// <returns>A <see cref="TestEndpointOutcomeBuilder{Unit}"/> instance.</returns>
        public static TestEndpointOutcomeBuilder<Unit> ForUnit() => new TestEndpointOutcomeBuilder<Unit>();

        /// <summary>
        /// Creates a new builder for a success outcome with no specific value.
        /// Useful when the outcome represents a command execution result.
        /// </summary>
        /// <returns>A <see cref="TestEndpointOutcomeBuilder{Unit}"/> instance in a success state.</returns>
        public static TestEndpointOutcomeBuilder<Unit> IsSuccess() => ForUnit().IsSuccess(Unit.Value);

        /// <summary>
        /// Creates a new builder for a success outcome with a specific value.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="value">The value for the successful outcome.</param>
        /// <returns>A <see cref="TestEndpointOutcomeBuilder{TValue}"/> instance in a success state with the provided value.</returns>
        public static TestEndpointOutcomeBuilder<TValue> IsSuccess<TValue>(TValue value) => new TestEndpointOutcomeBuilder<TValue>().IsSuccess(value);

        /// <summary>
        /// Creates a new builder for a failure outcome.
        /// </summary>
        /// <returns>A <see cref="TestEndpointOutcomeBuilder{Unit}"/> instance in a failure state.</returns>
        public static TestEndpointOutcomeBuilder<Unit> IsFailure() => ForUnit().IsFailure();

        /// <summary>
        /// Creates a new builder for a failure outcome with an error.
        /// </summary>
        /// <param name="errorInfo">The error information.</param>
        /// <returns>A <see cref="TestEndpointOutcomeBuilder{Unit}"/> instance in a failure state with the provided error.</returns>
        public static TestEndpointOutcomeBuilder<Unit> WithError(ErrorInfo errorInfo) => ForUnit().WithError(errorInfo);

        internal static IResultStatus? MapCategoryToStatus(ErrorCategory category) =>
            category switch
            {
                ErrorCategory.Validation => ResultStatuses.BadRequest,
                ErrorCategory.NotFound => ResultStatuses.NotFound,
                ErrorCategory.Authentication => ResultStatuses.Unauthorized,
                ErrorCategory.Authorization => ResultStatuses.Forbidden,
                ErrorCategory.Conflict => ResultStatuses.Conflict,
                _ => ResultStatuses.InternalServerError,
            };
    }
}
