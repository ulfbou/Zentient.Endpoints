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
        /// Creates a mocked <see cref="IEndpointOutcome"/> instance based on a Zentient <see cref="Results.IResult"/>.
        /// </summary>
        /// <param name="baseResult">The underlying <see cref="Results.IResult"/> for the outcome.</param>
        /// <param name="transportMetadata">Optional <see cref="TransportMetadata"/> to associate with the outcome.</param>
        /// <returns>A mocked <see cref="IEndpointOutcome"/> object.</returns>
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

        /// <summary>
        /// Creates a mocked generic <see cref="IEndpointOutcome{TValue}"/> instance based on a Zentient <see cref="IResult{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="baseResult">The underlying <see cref="IResult{TValue}"/> for the outcome.</param>
        /// <param name="transportMetadata">Optional <see cref="TransportMetadata"/> to associate with the outcome.</param>
        /// <returns>A mocked generic <see cref="IEndpointOutcome{TValue}"/> object.</returns>
        public static IEndpointOutcome<TValue> CreateGenericEndpointOutcomeMock<TValue>(
            IResult<TValue> baseResult,
            TransportMetadata? transportMetadata = null)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(baseResult, nameof(baseResult));

            Mock<IEndpointOutcome<TValue>> mock = new Mock<IEndpointOutcome<TValue>>();
            mock.SetupGet(e => e.IsSuccess).Returns(baseResult.IsSuccess);
            mock.SetupGet(e => e.IsFailure).Returns(baseResult.IsFailure);
            mock.SetupGet(e => e.Errors).Returns(baseResult.Errors);
            mock.SetupGet(e => e.Messages).Returns(baseResult.Messages);
            mock.SetupGet(e => e.ErrorMessage).Returns(baseResult.ErrorMessage);
            mock.SetupGet(e => e.Status).Returns(baseResult.Status);
            mock.SetupGet(e => e.Metadata).Returns(transportMetadata ?? new TransportMetadata());

            if (baseResult.IsSuccess)
            {
                mock.SetupGet(e => e.Value).Returns(baseResult.Value!);
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
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
