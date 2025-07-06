// <copyright file="TestEndpointOutcomeBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A fluent builder for creating <see cref="IEndpointOutcome{TValue}"/> instances
    /// for comprehensive testing scenarios.
    /// </summary>
    /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public sealed class TestEndpointOutcomeBuilder<TValue>
    {
        private TValue? _value;
        private bool _isSuccess = true; // Default to success
        private List<string> _messages = new List<string>();
        private List<ErrorInfo> _errors = new List<ErrorInfo>();
        private ImmutableDictionary<string, object?> _metadataTags = ImmutableDictionary<string, object?>.Empty;
        private IResultStatus? _status; // Use nullable for internal tracking

        /// <summary>
        /// Initializes a new instance of the <see cref="TestEndpointOutcomeBuilder{TValue}"/> class.
        /// </summary>
        public TestEndpointOutcomeBuilder()
        {
            _status = ResultStatuses.Ok; // Default success status
        }

        /// <summary>
        /// Sets the outcome to a success state with an optional value.
        /// </summary>
        /// <param name="value">The value for the successful outcome.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> IsSuccess(TValue value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            _isSuccess = true;
            _value = value;
            _errors.Clear();
            _messages.Clear();
            _status = ResultStatuses.Ok;
            return this;
        }

        /// <summary>
        /// Sets the outcome to a failure state.
        /// </summary>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> IsFailure()
        {
            ResetForFailure();
            return this;
        }

        /// <summary>
        /// Adds a message to the outcome.
        /// </summary>
        /// <param name="message">The message string.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithMessage(string message)
        {
            _messages.Add(message);
            return this;
        }

        /// <summary>
        /// Adds multiple messages to the outcome.
        /// </summary>
        /// <param name="messages">A collection of message strings.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithMessages(IEnumerable<string> messages)
        {
            _messages.AddRange(messages);
            return this;
        }

        /// <summary>
        /// Adds an error to the outcome, implicitly setting it to a failure state.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithError(ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            ResetForFailure();
            _errors.Add(error);
            _status = TestEndpointOutcomeBuilder.MapCategoryToStatus(error.Category);
            return this;
        }

        /// <summary>
        /// Adds multiple errors to the outcome, implicitly setting it to a failure state.
        /// </summary>
        /// <param name="errors">A collection of error information.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithErrors(IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorsList = errors.ToList();
            if (errorsList.Count == 0) return this;

            ResetForFailure();
            _errors.AddRange(errorsList);

            var firstSpecificError = errorsList.FirstOrDefault(e =>
                e.Category != ErrorCategory.General && e.Category != ErrorCategory.None && e.Category != ErrorCategory.ProblemDetails);

            _status = firstSpecificError is not null
                ? TestEndpointOutcomeBuilder.MapCategoryToStatus(firstSpecificError.Category)
                : ResultStatuses.InternalServerError;
            return this;
        }

        /// <summary>
        /// Sets the explicit status for the outcome.
        /// </summary>
        /// <param name="status">The <see cref="IResultStatus"/> object.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithStatus(IResultStatus status)
        {
            ArgumentNullException.ThrowIfNull(status, nameof(status));
            _status = status;
            return this;
        }

        /// <summary>
        /// Adds a custom metadata tag to the outcome's transport metadata.
        /// </summary>
        /// <param name="key">The key for the metadata tag.</param>
        /// <param name="value">The value for the metadata tag.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public TestEndpointOutcomeBuilder<TValue> WithMetadataTag(string key, object? value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            _metadataTags = _metadataTags.SetItem(key, value);
            return this;
        }

        /// <summary>
        /// Builds the <see cref="IEndpointOutcome{TValue}"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IEndpointOutcome{TValue}"/> instance.</returns>
        public IEndpointOutcome<TValue> Build()
        {
            if (_isSuccess)
            {
                // Ensure _status is not null for Success path, default to Ok if somehow not set.
                // Cast _value to TValue directly. If TValue is a non-nullable value type, and _value is null,
                // this will result in a runtime default value (e.g., 0 for int, false for bool).
                // If TValue is Unit, it's always Unit.Value.
                var successStatus = _status ?? ResultStatuses.Ok;
                var result = Results.Result<TValue>.Success(_value!, successStatus, _messages.AsReadOnly());
                return EndpointOutcome<TValue>.From(result, new TransportMetadata(_metadataTags));
            }
            else
            {
                // Ensure _status is not null for Failure path, default to InternalServerError if somehow not set.
                var failureStatus = _status ?? ResultStatuses.InternalServerError;

                // Use the correct overload for Failure
                // Assuming Results.Result<TValue>.Failure has an overload like:
                // Failure(IEnumerable<ErrorInfo> errors, IResultStatus? status = null, IEnumerable<string>? messages = null)
                var result = Results.Result<TValue>.Failure(
                    errors: _errors.AsReadOnly(),
                    status: failureStatus);

                return EndpointOutcome<TValue>.From(result, new TransportMetadata(_metadataTags));
            }
        }

        private void ResetForFailure()
        {
            _isSuccess = false;
            _value = default;
            _messages.Clear();
            _errors.Clear();
            _status = ResultStatuses.InternalServerError;
        }
    }
}
