// <copyright file="ResultMockHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Moq; // For mocking interfaces

using Zentient.Results;
using Zentient.Results.Constants;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests.Common
{
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This is a test helper class, documentation is not required for tests.")]
    public static class ResultMockHelper
    {
        /// <summary>
        /// Creates a mock <see cref="IResultStatus"/> with the specified code and description.
        /// </summary>
        /// <param name="code">The status code to use for the mock.</param>
        /// <param name="description">The description to use for the mock.</param>
        /// <returns>A mock <see cref="IResultStatus"/> instance.</returns>
        public static IResultStatus CreateMockResultStatus(int code, string description)
        {
            var mockStatus = new Mock<IResultStatus>();
            mockStatus.SetupGet(s => s.Code).Returns(code);
            mockStatus.SetupGet(s => s.Description).Returns(description);
            mockStatus.Setup(s => s.ToString()).Returns($"({code}) {description}");
            return mockStatus.Object;
        }

        /// <summary>
        /// Creates an <see cref="ErrorInfo"/> instance with common properties.
        /// </summary>
        /// <param name="category">The error category.</param>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="detail">The error detail (optional).</param>
        /// <param name="metadata">The error metadata (optional).</param>
        /// <param name="innerErrors">The inner errors (optional).</param>
        /// <returns>An <see cref="ErrorInfo"/> instance.</returns>
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

        /// <summary>
        /// Creates a mock <see cref="IResult"/> instance for a successful outcome.
        /// </summary>
        /// <param name="status">The result status to use (optional).</param>
        /// <param name="messages">The messages to include (optional).</param>
        /// <returns>A mock <see cref="IResult"/> representing a successful result.</returns>
        public static Mock<IResult> CreateMockSuccessfulResult(
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
        {
            var mockResult = new Mock<IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(true);
            mockResult.SetupGet(r => r.IsFailure).Returns(false);
            mockResult.SetupGet(r => r.Status).Returns(status ?? CreateMockResultStatus(200, "OK"));
            mockResult.SetupGet(r => r.Errors).Returns(ImmutableList<ErrorInfo>.Empty);
            mockResult.SetupGet(r => r.Messages).Returns(messages?.ToImmutableList() ?? ImmutableList<string>.Empty);
            mockResult.SetupGet(r => r.ErrorMessage).Returns((string?)null);
            return mockResult;
        }

        /// <summary>
        /// Creates a mock <see cref="IResult"/> instance for a failed outcome.
        /// </summary>
        /// <param name="errors">The errors to include in the result.</param>
        /// <param name="status">The result status to use (optional).</param>
        /// <param name="messages">The messages to include (optional).</param>
        /// <returns>A mock <see cref="IResult"/> representing a failed result.</returns>
        public static Mock<IResult> CreateMockFailedResult(
        IEnumerable<ErrorInfo>? errors = null,
        IResultStatus? status = null,
        IEnumerable<string>? messages = null)
        {
            ImmutableList<ErrorInfo> errorList = errors?.ToImmutableList() ?? ImmutableList<ErrorInfo>.Empty;
            var mockResult = new Mock<IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(false);
            mockResult.SetupGet(r => r.IsFailure).Returns(true);
            mockResult.SetupGet(r => r.Status).Returns(status ?? CreateMockResultStatus(400, "Bad Request"));
            mockResult.SetupGet(r => r.Errors).Returns(errorList);
            mockResult.SetupGet(r => r.Messages).Returns(messages?.ToImmutableList() ?? ImmutableList<string>.Empty);
            mockResult.SetupGet(r => r.ErrorMessage).Returns(errorList.FirstOrDefault()?.Message);
            return mockResult;
        }

        /// <summary>
        /// Creates a mock <see cref="IResult{T}"/> instance for a successful outcome.
        /// </summary>
        /// <typeparam name="T">The type of the value in the result.</typeparam>
        /// <param name="value">The value to return for the result.</param>
        /// <param name="status">The result status to use (optional).</param>
        /// <param name="messages">The messages to include (optional).</param>
        /// <returns>A mock <see cref="IResult{T}"/> representing a successful result.</returns>
        public static Mock<IResult<T>> CreateMockSuccessfulResult<T>(
            T value,
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
        {
            var mockResult = new Mock<IResult<T>>();
            mockResult.ApplyResultProperties(CreateMockSuccessfulResult(status, messages).Object);
            mockResult.SetupGet(r => r.Value).Returns(value);
            mockResult.Setup(r => r.GetValueOrThrow()).Returns(value!);
            mockResult.Setup(r => r.GetValueOrThrow(It.IsAny<string>())).Returns(value!);
            mockResult.Setup(r => r.GetValueOrThrow(It.IsAny<Func<Exception>>())).Returns(value!);
            mockResult.Setup(r => r.GetValueOrDefault(It.IsAny<T>())).Returns(value!);
            mockResult.Setup(r => r.Tap(It.IsAny<Action<T>>()))
                .Callback((Action<T> action) => action(value!))
                .Returns(mockResult.Object);
            mockResult.Setup(r => r.OnSuccess(It.IsAny<Action<T>>()))
                .Callback((Action<T> action) => action(value!))
                .Returns(mockResult.Object);
            mockResult.Setup(r => r.OnFailure(It.IsAny<Action<IReadOnlyList<ErrorInfo>>>())).Returns(mockResult.Object);
            return mockResult;
        }

        /// <summary>
        /// Creates a mock <see cref="IResult{T}"/> instance for a failed outcome.
        /// </summary>
        /// <typeparam name="T">The type of the value in the result.</typeparam>
        /// <param name="errors">The errors to include in the result.</param>
        /// <param name="status">The result status to use (optional).</param>
        /// <param name="messages">The messages to include (optional).</param>
        /// <returns>A mock <see cref="IResult{T}"/> representing a failed result.</returns>
        public static Mock<IResult<T>> CreateMockFailedResult<T>(
            IEnumerable<ErrorInfo>? errors = null,
            IResultStatus? status = null,
            IEnumerable<string>? messages = null)
        {
            ImmutableList<ErrorInfo> errorsList = errors?.ToImmutableList() ?? ImmutableList<ErrorInfo>.Empty;
            var mockResult = new Mock<IResult<T>>();
            mockResult.ApplyResultProperties(CreateMockFailedResult(errorsList, status, messages).Object);
            mockResult.SetupGet(r => r.Value).Returns(default(T));
            mockResult.Setup(r => r.GetValueOrThrow()).Throws(new InvalidOperationException());
            mockResult.Setup(r => r.GetValueOrThrow(It.IsAny<string>())).Throws((string msg) => new InvalidOperationException(msg));
            mockResult.Setup(r => r.GetValueOrThrow(It.IsAny<Func<Exception>>())).Throws((Func<Exception> factory) => factory());
            mockResult.Setup(r => r.GetValueOrDefault(It.IsAny<T>())).Returns((T fallback) => fallback);
            mockResult.Setup(r => r.Tap(It.IsAny<Action<T>>())).Returns(mockResult.Object);
            mockResult.Setup(r => r.OnSuccess(It.IsAny<Action<T>>())).Returns(mockResult.Object);
            mockResult.Setup(r => r.OnFailure(It.IsAny<Action<IReadOnlyList<ErrorInfo>>>()))
                .Callback((Action<IReadOnlyList<ErrorInfo>> action) => action(errorsList))
                .Returns(mockResult.Object);
            return mockResult;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
