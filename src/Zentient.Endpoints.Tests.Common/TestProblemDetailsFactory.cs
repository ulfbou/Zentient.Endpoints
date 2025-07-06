// <copyright file="TestProblemDetailsFactory.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A factory for creating <see cref="ProblemDetails"/> instances for testing purposes.
    /// </summary>
    [SuppressMessage("Design", "CA1515:Member names should begin with a capital letter", Justification = "Consistent with ASP.NET Core conventions for fluent builders.")]
    public static class TestProblemDetailsFactory
    {
        private static class Defaults
        {
            public const string TypeBase = "about:blank";
            public const string TitleServer = "Internal Server Error";
            public const string DetailServer = "An unexpected error occurred.";
        }

        /// <summary>
        /// Creates a base <see cref="ProblemDetails"/> instance with common defaults.
        /// </summary>
        /// <param name="status">The HTTP status code. Defaults to 500.</param>
        /// <param name="type">The problem type URI. Defaults to "about:blank".</param>
        /// <param name="title">The short, human-readable summary. Defaults to "Internal Server Error".</param>
        /// <param name="detail">A human-readable explanation of the error. Defaults to "An unexpected error occurred."</param>
        /// <param name="instance">A URI reference that identifies the specific occurrence of the problem. Defaults to a new GUID URI.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance.</returns>
        public static ProblemDetails CreateBase(
            int status = StatusCodes.Status500InternalServerError,
            string type = Defaults.TypeBase,
            string title = Defaults.TitleServer,
            string detail = Defaults.DetailServer,
            string? instance = null)
        {
            return new ProblemDetails
            {
                Status = status,
                Type = type,
                Title = title,
                Detail = detail,
                Instance = instance ?? $"urn:error:{Guid.NewGuid()}",
                Extensions = new Dictionary<string, object?>()
            };
        }

        /// <summary>
        /// Creates a <see cref="ValidationProblemDetails"/> instance for validation errors (400 Bad Request).
        /// </summary>
        /// <param name="detail">A human-readable explanation of the error. Defaults to "One or more validation errors occurred."</param>
        /// <param name="errors">A dictionary of validation errors (field name to array of messages).</param>
        /// <param name="type">The problem type URI. Defaults to "/errors/validation-error".</param>
        /// <param name="instance">A URI reference that identifies the specific occurrence of the problem.</param>
        /// <returns>A <see cref="ValidationProblemDetails"/> instance.</returns>
        public static ValidationProblemDetails ValidationProblem(
            string detail = "One or more validation errors occurred.",
            IDictionary<string, string[]>? errors = null,
            string type = "/errors/validation-error",
            string? instance = null)
        {
            return new ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = type,
                Title = "Validation Error",
                Detail = detail,
                Instance = instance ?? $"urn:error:{Guid.NewGuid()}",
                Errors = errors ?? new Dictionary<string, string[]>()
            };
        }

        /// <summary>
        /// Creates a <see cref="ProblemDetails"/> instance for "not found" errors (404 Not Found).
        /// </summary>
        /// <param name="detail">A human-readable explanation of the error. Defaults to "The requested resource was not found."</param>
        /// <param name="type">The problem type URI. Defaults to "/errors/not-found".</param>
        /// <param name="instance">A URI reference that identifies the specific occurrence of the problem.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance.</returns>
        public static ProblemDetails NotFoundProblem(
            string detail = "The requested resource was not found.",
            string type = "/errors/not-found",
            string? instance = null)
        {
            return CreateBase(
                status: StatusCodes.Status404NotFound,
                type: type,
                title: "Not Found",
                detail: detail,
                instance: instance);
        }

        /// <summary>
        /// Creates a <see cref="ProblemDetails"/> instance for "unauthorized" errors (401 Unauthorized).
        /// </summary>
        /// <param name="detail">A human-readable explanation of the error. Defaults to "Authentication required or failed."</param>
        /// <param name="type">The problem type URI. Defaults to "/errors/unauthorized".</param>
        /// <param name="instance">A URI reference that identifies the specific occurrence of the problem.</param>
        /// <returns>A <see cref="ProblemDetails"/> instance.</returns>
        public static ProblemDetails UnauthorizedProblem(
            string detail = "Authentication required or failed.",
            string type = "/errors/unauthorized",
            string? instance = null)
        {
            return CreateBase(
                status: StatusCodes.Status401Unauthorized,
                type: type,
                title: "Unauthorized",
                detail: detail,
                instance: instance);
        }
    }

    /// <summary>
    /// Extension methods for <see cref="ProblemDetails"/> to support fluent mutation in tests.
    /// </summary>
    internal static class ProblemDetailsExtensions
    {
        /// <summary>
        /// Adds or updates an extension property on a <see cref="ProblemDetails"/> instance.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="ProblemDetails"/> (supports derived types).</typeparam>
        /// <param name="details">The <see cref="ProblemDetails"/> instance to extend.</param>
        /// <param name="key">The key for the extension.</param>
        /// <param name="value">The value for the extension.</param>
        /// <returns>The modified <see cref="ProblemDetails"/> instance.</returns>
        public static T WithExtension<T>(this T details, string key, object value)
            where T : ProblemDetails
        {
            details.Extensions ??= new Dictionary<string, object?>();
            details.Extensions[key] = value;
            return details;
        }
    }
}
