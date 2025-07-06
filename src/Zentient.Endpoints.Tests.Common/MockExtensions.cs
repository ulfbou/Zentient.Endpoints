// <copyright file="MockExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;

using Zentient.Results;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides extension methods for Moq to apply properties of an <see cref="IResult"/> to a mock instance.
    /// /// This is useful for unit testing scenarios where you want to simulate the behavior of an <see cref="IResult"/>
    /// /// without having to manually set each property on the mock.
    /// /// </summary>
    /// /// <remarks>
    /// /// This class contains a single extension method that sets up the mock to return the properties of the provided <see cref="IResult"/>.
    /// The method sets up the following properties:
    /// /// - <see cref="IResult.IsSuccess"/>
    /// /// - <see cref="IResult.IsFailure"/>
    /// /// - <see cref="IResult.Errors"/>
    /// /// - <see cref="IResult.Messages"/>
    /// /// - <see cref="IResult.ErrorMessage"/>
    /// /// - <see cref="IResult.Status"/>
    /// </remarks>
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    public static class MockExtensions
    {
        /// <summary>
        /// Applies the properties of a <see cref="IResult"/> to a mock of type <typeparamref name="TMock"/>.
        /// This method sets up the mock to return the properties of the provided <paramref name="result"/>.
        /// </summary>
        /// <typeparam name="TMock">The type of the mock, which must implement <see cref="IResult"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result, which must implement <see cref="IResult"/>.</typeparam>
        /// <param name="mock">The mock instance to apply the properties to.</param>
        /// <param name="result">The result instance whose properties will be applied to the mock.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="mock"/> or <paramref name="result"/> is null.</exception>
        /// <remarks>
        /// This extension method is useful for setting up mocks in unit tests where you want to simulate
        /// the behavior of an <see cref="IResult"/> without having to manually set each property.
        /// It ensures that the mock behaves as expected when the properties of the result are accessed.
        /// The method sets up the following properties:
        /// - <see cref="IResult.IsSuccess"/>
        /// - <see cref="IResult.IsFailure"/>
        /// - <see cref="IResult.Errors"/>
        /// - <see cref="IResult.Messages"/>
        /// - <see cref="IResult.ErrorMessage"/>
        /// - <see cref="IResult.Status"/>
        /// </remarks>
        /// <returns>A reference to the mock instance for fluent chaining.</returns>
        public static void ApplyResultProperties<TMock, TResult>(this Mock<TMock> mock, TResult result)
                    where TMock : class, IResult
                    where TResult : IResult
        {
            ArgumentNullException.ThrowIfNull(mock, nameof(mock));
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            mock.SetupGet<bool>(m => m.IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet<bool>(m => m.IsFailure).Returns(result.IsFailure);
            mock.SetupGet<IReadOnlyList<ErrorInfo>>(m => m.Errors).Returns(result.Errors);
            mock.SetupGet<IReadOnlyList<string>>(m => m.Messages).Returns(result.Messages);
            mock.SetupGet<string?>(m => m.ErrorMessage).Returns(result.ErrorMessage ?? string.Empty);
            mock.SetupGet<IResultStatus>(m => m.Status).Returns(result.Status);
        }

        /// <summary>
        /// Applies the properties of a <see cref="IResult"/> to a mock of <see cref="IEndpointOutcome"/>.
        /// This method sets up the mock to return the properties of the provided <paramref name="result"/>.
        /// </summary>
        /// <param name="mock">The mock instance to apply the properties to.</param>
        /// <param name="result">The result instance whose properties will be applied to the mock.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="mock"/> or <paramref name="result"/> is null.</exception>
        public static void ApplyResultProperties(this Mock<IEndpointOutcome> mock, IResult result)
        {
            ArgumentNullException.ThrowIfNull(mock, nameof(mock));
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            mock.SetupGet(m => m.IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet(m => m.IsFailure).Returns(result.IsFailure);
            mock.SetupGet(m => m.Errors).Returns(result.Errors);
            mock.SetupGet(m => m.Messages).Returns(result.Messages);
            mock.SetupGet(m => m.ErrorMessage).Returns(result.ErrorMessage);
            mock.SetupGet(m => m.Status).Returns(result.Status);
        }

        /// <summary>
        /// Applies the properties of a <see cref="IResult{TValue}"/> to a mock of <see cref="IEndpointOutcome{TValue}"/>.
        /// This method sets up the mock to return the properties of the provided <paramref name="result"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value produced on success.</typeparam>
        /// <param name="mock">The mock instance to apply the properties to.</param>
        /// <param name="result">The result instance whose properties will be applied to the mock.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="mock"/> or <paramref name="result"/> is null.</exception>
        public static void ApplyResultProperties<TValue>(this Mock<IEndpointOutcome<TValue>> mock, IResult<TValue> result)
        {
            ArgumentNullException.ThrowIfNull(mock, nameof(mock));
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            mock.SetupGet(m => m.IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet(m => m.IsFailure).Returns(result.IsFailure);
            mock.SetupGet(m => m.Errors).Returns(result.Errors);
            mock.SetupGet(m => m.Messages).Returns(result.Messages);
            mock.SetupGet(m => m.ErrorMessage).Returns(result.ErrorMessage);
            mock.SetupGet(m => m.Status).Returns(result.Status);
        }
    }
}
