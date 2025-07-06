// <copyright file="TestHttpResultFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Zentient.Endpoints.Http.Models;
using Zentient.Endpoints.Http.Mapping;

namespace Zentient.Endpoints.Http.Tests.Helpers
{
    /// <summary>
    /// A factory for creating common <see cref="IResult"/> instances,
    /// useful for asserting expected HTTP responses in tests.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public static class TestHttpResultFactory
    {
        private static readonly JsonSerializerOptions _defaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP 200 OK with JSON content.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The object to be serialized as JSON.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult Ok<T>(T value) =>
            Microsoft.AspNetCore.Http.Results.Json(value, _defaultJsonSerializerOptions, statusCode: StatusCodes.Status200OK);

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP 201 Created with JSON content and a Location header.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <param name="value">The object to be serialized as JSON.</param>
        /// <param name="locationUri">The URI for the Location header.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult Created<T>(T value, Uri locationUri)
        {
            ArgumentNullException.ThrowIfNull(locationUri);
            return Microsoft.AspNetCore.Http.Results.Created(locationUri, value);
        }

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP 204 No Content response.
        /// </summary>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult NoContent() =>
            Microsoft.AspNetCore.Http.Results.NoContent();

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP 400 Bad Request with Problem Details.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult BadRequest(ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);
            problemDetails.Status = StatusCodes.Status400BadRequest;
            return CreateProblem(problemDetails);
        }

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP 404 Not Found with Problem Details.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult NotFound(ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);
            problemDetails.Status = StatusCodes.Status404NotFound;
            return CreateProblem(problemDetails);
        }

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP Problem Details response
        /// with the status code from the <see cref="ProblemDetails"/> object.
        /// </summary>
        /// <param name="problemDetails">The <see cref="ProblemDetails"/> instance.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult Problem(ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);
            return CreateProblem(problemDetails);
        }

        /// <summary>
        /// Creates an <see cref="IResult"/> representing an HTTP response with a custom status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>An <see cref="IResult"/> instance.</returns>
        public static IResult StatusCode(int statusCode) =>
            Microsoft.AspNetCore.Http.Results.StatusCode(statusCode);

        /// <summary>
        /// Creates a <see cref="HeaderWrappedResult"/> (internal wrapper) for testing scenarios
        /// where headers and location are applied before an inner result.
        /// </summary>
        /// <param name="innerResult">The inner IResult to wrap.</param>
        /// <param name="headers">Custom headers to include.</param>
        /// <param name="location">A Location URI to include.</param>
        /// <returns>A <see cref="HeaderWrappedResult"/> instance.</returns>
        public static IResult HeaderWrapped(IResult innerResult, IReadOnlyDictionary<string, string>? headers = null, Uri? location = null) =>
            new HeaderWrappedResult(
                innerResult,
                headers?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty,
                location);

        private static IResult CreateProblem(ProblemDetails pd) =>
            Microsoft.AspNetCore.Http.Results.Problem(
                detail: pd.Detail,
                instance: pd.Instance,
                statusCode: pd.Status,
                title: pd.Title,
                type: pd.Type,
                extensions: pd.Extensions);
    }
}
