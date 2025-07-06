// <copyright file="EndpointOutcomeMockHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Zentient.Endpoints;
using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Extensions;
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
                ? CreateEndpointOutcomeMock(result!, metadata)
                : CreateGenericEndpointOutcomeMock(result, metadata);
        }

        public static IEndpointOutcome CreateEndpointOutcomeMock(
            Results.IResult baseResult,
            TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(baseResult, nameof(baseResult));
            var mock = new Mock<IEndpointOutcome>();
            mock.ApplyResultProperties(baseResult);
            mock.SetupGet(e => e.Metadata).Returns(transportMetadata ?? new TransportMetadata());
            return mock.Object;
        }

        public static IEndpointOutcome<T> CreateGenericEndpointOutcomeMock<T>(IResult<T> result, TransportMetadata? metadata)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            var mock = new Mock<IEndpointOutcome<T>>();
            mock.ApplyResultProperties(result);
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
        {
            ArgumentNullException.ThrowIfNull(zentientResult, nameof(zentientResult));
            ArgumentNullException.ThrowIfNull(transport, nameof(transport));
            return new EndpointOutcome<T>(zentientResult, transport);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
