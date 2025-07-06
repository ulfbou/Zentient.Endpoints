// <copyright file="EndpointOutcomeMockHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable; // For ImmutableDictionary
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection; // For Mocking services
using Microsoft.Extensions.Logging;             // For ILogger

using Moq; // For mocking interfaces

using Zentient.Endpoints.Http.Constants;      // Assuming HttpMetadataKeys is here
using Zentient.Endpoints.Http.Extensions;    // For TransportMetadataHttpExtensions
using Zentient.Endpoints.Http.Mapping;
using Zentient.Results;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests.Common
{
    [SuppressMessage("Design", "CA1515:Use value objects where appropriate", Justification = "This is a test helper, not a domain model.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This is a test helper class, documentation is not required for tests.")]
    public static class EndpointOutcomeMockHelper
    {
        /// <summary>
        /// Creates a mock <see cref="IEndpointOutcome"/> or <see cref="IEndpointOutcome{T}"/> instance
        /// based on the provided <see cref="IResult{T}"/> and optional <see cref="TransportMetadata"/>.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result to use for populating the mock outcome.</param>
        /// <param name="metadata">Optional transport metadata to associate with the outcome. If null, a new instance is used.</param>
        /// <returns>
        /// An <see cref="IEndpointOutcome"/> if <typeparamref name="T"/> is <see cref="Unit"/>; 
        /// otherwise, an <see cref="IEndpointOutcome{T}"/>.
        /// </returns>
        public static IEndpointOutcome CreateMock<T>(IResult<T> result, TransportMetadata? metadata = null)
        {
            return typeof(T) == typeof(Unit)
                ? CreateEndpointOutcomeMock((Zentient.Results.IResult)result!, metadata)
                : CreateGenericEndpointOutcomeMock(result, metadata);
        }

        public static IEndpointOutcome CreateEndpointOutcomeMock(
                    Results.IResult baseResult,
                    TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(baseResult, nameof(baseResult));

            Mock<IEndpointOutcome> mock = new Mock<IEndpointOutcome>();
            mock.SetupGet(e => e.IsSuccess).Returns(baseResult.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(baseResult.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(baseResult.Errors);
            mock.SetupGet(e => e.Messages).Returns(baseResult.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(baseResult.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(baseResult.Status);
            mock.SetupGet(e => e.Metadata).Returns(transportMetadata ?? new TransportMetadata());

            return mock.Object;
        }

        public static IEndpointOutcome<T> CreateGenericEndpointOutcomeMock<T>(IResult<T> result, TransportMetadata? metadata)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            Mock<IEndpointOutcome<T>> mock = new Mock<IEndpointOutcome<T>>();
            mock.SetupGet(e => e.IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(result.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(result.Errors);
            mock.SetupGet(e => e.Messages).Returns(result.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(result.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(result.Status);
            mock.SetupGet(e => e.Metadata).Returns(metadata ?? new TransportMetadata());

            if (result.IsSuccess)
            {
                mock.SetupGet(e => e.Value).Returns(result.Value!);
            }
            else
            {
                mock.SetupGet(e => e.Value).Throws<InvalidOperationException>();
            }

            return mock.Object;
        }

        /// <summary>
        /// Creates a generic <see cref="IEndpointOutcome{T}"/> from a Zentient <see cref="IResult{T}"/> and transport metadata.
        /// </summary>
        /// <typeparam name="T">The type of the outcome's value.</typeparam>
        /// <param name="zentientResult">The Zentient result to convert into an endpoint outcome.</param>
        /// <param name="transport">Transport metadata to associate with the outcome.</param>
        /// <returns>A new <see cref="IEndpointOutcome{T}"/> instance.</returns>
        public static IEndpointOutcome<T> CreateGenericEndpointOutcome<T>(IResult<T> zentientResult, TransportMetadata transport)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(zentientResult, nameof(zentientResult));
            ArgumentNullException.ThrowIfNull(transport, nameof(transport));

            return new EndpointOutcome<T>(zentientResult, transport);
        }

        private static IEndpointOutcome CreateNonGenericMock(
            Results.IResult baseResult,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(baseResult, nameof(baseResult));

            Mock<IEndpointOutcome> mock = new Mock<IEndpointOutcome>();
            mock.SetupGet(e => e.IsSuccess).Returns(baseResult.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(baseResult.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(baseResult.Errors);
            mock.SetupGet(e => e.Messages).Returns(baseResult.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(baseResult.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(baseResult.Status);
            mock.SetupGet(e => e.Metadata).Returns(transportMetadata ?? new TransportMetadata());

            return mock.Object;
        }

        private static IEndpointOutcome CreateGenericMock<T>(IResult<T> result, TransportMetadata? metadata)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            Mock<IEndpointOutcome<T>> mock = new Mock<IEndpointOutcome<T>>();
            mock.SetupGet(e => e.IsSuccess).Returns(result.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(result.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(result.Errors);
            mock.SetupGet(e => e.Messages).Returns(result.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(result.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(result.Status);
            mock.SetupGet(e => e.Metadata).Returns(metadata ?? new TransportMetadata());

            if (result.IsSuccess)
            {
                mock.SetupGet(e => e.Value).Returns(result.Value!);
            }
            else
            {
                mock.SetupGet(e => e.Value).Throws<InvalidOperationException>();
            }

            return mock.Object;
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
