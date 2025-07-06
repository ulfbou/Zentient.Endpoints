// <copyright file="TestEndpointOutcomeBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using Zentient.Results;

namespace Zentient.Endpoints.Builders
{
    /// <summary>
    /// A fluent builder for creating <see cref="IEndpointOutcome{TValue}"/> instances
    /// for comprehensive testing scenarios.
    /// </summary>
    /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public sealed class EndpointOutcomeBuilder<TValue>
    {
        private TValue? _value;
        private bool _isSuccess = true; // Default to success
        private List<string> _messages = new List<string>();
        private List<ErrorInfo> _errors = new List<ErrorInfo>();
        private ImmutableDictionary<string, object?> _metadataTags = ImmutableDictionary<string, object?>.Empty;
        private IResultStatus? _status; // Use nullable for internal tracking

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointOutcomeBuilder{TValue}"/> class.
        /// </summary>
        public EndpointOutcomeBuilder()
        {
            _status = ResultStatuses.Ok; // Default success status
        }

        /// <summary>
        /// Sets the outcome to a success state with an optional value.
        /// </summary>
        /// <param name="value">The value for the successful outcome.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> IsSuccess(TValue value)
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
        public EndpointOutcomeBuilder<TValue> IsFailure()
        {
            ResetForFailure();
            return this;
        }

        /// <summary>
        /// Adds a message to the outcome.
        /// </summary>
        /// <param name="message">The message string.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithMessage(string message)
        {
            _messages.Add(message);
            return this;
        }

        /// <summary>
        /// Adds multiple messages to the outcome.
        /// </summary>
        /// <param name="messages">A collection of message strings.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithMessages(IEnumerable<string> messages)
        {
            _messages.AddRange(messages);
            return this;
        }

        /// <summary>
        /// Adds an error to the outcome, implicitly setting it to a failure state.
        /// </summary>
        /// <param name="error">The error information.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithError(ErrorInfo error)
        {
            ArgumentNullException.ThrowIfNull(error, nameof(error));
            ResetForFailure();
            _errors.Add(error);
            _status = EndpointOutcomeBuilder.MapCategoryToStatus(error.Category);
            return this;
        }

        /// <summary>
        /// Adds multiple errors to the outcome, implicitly setting it to a failure state.
        /// </summary>
        /// <param name="errors">A collection of error information.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithErrors(IEnumerable<ErrorInfo> errors)
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            var errorsList = errors.ToList();
            if (errorsList.Count == 0) return this;

            ResetForFailure();
            _errors.AddRange(errorsList);

            var firstSpecificError = errorsList.FirstOrDefault(e =>
                e.Category != ErrorCategory.General && e.Category != ErrorCategory.None && e.Category != ErrorCategory.ProblemDetails);

            _status = firstSpecificError is not null
                ? EndpointOutcomeBuilder.MapCategoryToStatus(firstSpecificError.Category)
                : ResultStatuses.InternalServerError;
            return this;
        }

        /// <summary>
        /// Sets the explicit status for the outcome.
        /// </summary>
        /// <param name="status">The <see cref="IResultStatus"/> object.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithStatus(IResultStatus status)
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
        public EndpointOutcomeBuilder<TValue> WithMetadata(string key, object? value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            _metadataTags = _metadataTags.SetItem(key, value);
            return this;
        }

        /// <summary>
        /// Adds a custom metadata tag to the outcome's transport metadata.
        /// </summary>
        /// <param name="metadata">The <see cref="TransportMetadata"/> instance whose tags will be used.</param>
        /// <returns>The current builder instance for chaining.</returns>
        public EndpointOutcomeBuilder<TValue> WithMetadata(TransportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));
            _metadataTags = metadata.Tags;
            return this;
        }

        /// <summary>
        /// Builds the <see cref="IEndpointOutcome{TValue}"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IEndpointOutcome{TValue}"/> instance.</returns>
        public IEndpointOutcome<TValue> Build()
        {
            IResult<TValue> result = null!;

            if (_isSuccess)
            {
                var successStatus = _status ?? ResultStatuses.Ok;
                result = Result<TValue>.Success(_value!, successStatus, _messages.AsReadOnly());
                return EndpointOutcome<TValue>.From(result, new TransportMetadata(_metadataTags));
            }

            var failureStatus = _status ?? ResultStatuses.InternalServerError;
            result = Result<TValue>.Failure(
                errors: _errors.AsReadOnly(),
                status: failureStatus);

            return EndpointOutcome<TValue>.From(result, new TransportMetadata(_metadataTags));
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
