// <copyright file="EndpointOutcomeHttpExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Zentient.Endpoints.Http;
using Zentient.Endpoints.Http.Extensions;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Results;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides HTTP-specific extension methods for <see cref="IEndpointOutcome"/>
    /// to enrich its metadata for HTTP response generation.
    /// </summary>
    public static class EndpointOutcomeHttpExtensions
    {
        /// <summary>
        /// Gets the HTTP status code hint from the outcome's metadata, or 200 if not set.
        /// </summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <returns>The HTTP status code hint, or 200 if not set.</returns>
        public static int? GetHttpStatusCodeHint(
            this IEndpointOutcome outcome)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            outcome.Metadata.TryGetTag<int?>("http.status_code_hint", out var statusCode);
            return statusCode;
        }

        /// <summary>
        /// Fluently sets an HTTP status code hint on the outcome's metadata.
        /// This hint guides the HTTP mapper in determining the final HTTP status code.
        /// </summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="statusCode">The HTTP status code to hint.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome WithHttpStatusCodeHint(
            this IEndpointOutcome outcome,
            int statusCode)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentOutOfRangeException.ThrowIfNegative(statusCode, nameof(statusCode));
            return outcome.WithMetadata(m => m.WithHttpStatusCodeHint(statusCode));
        }

        /// <summary>
        /// Fluently sets an HTTP status code hint on the outcome's metadata for a generic outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The current generic endpoint outcome.</param>
        /// <param name="statusCode">The HTTP status code to hint.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome{TValue}" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome<TValue> WithHttpStatusCodeHint<TValue>(
            this IEndpointOutcome<TValue> outcome,
            int statusCode)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentOutOfRangeException.ThrowIfNegative(statusCode, nameof(statusCode));
            return outcome.WithMetadata(m => m.WithHttpStatusCodeHint(statusCode));
        }

        /// <summary>
        /// Fluently attaches a pre-built ProblemDetails object to the outcome's metadata.
        /// This will bypass the default ProblemDetails mapping logic in the HTTP mapper.
        /// </summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="problemDetails">The ProblemDetails object to attach.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome WithProblemDetails(
            this IEndpointOutcome outcome,
            ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(problemDetails, nameof(problemDetails));
            return outcome.WithMetadata(m => m.WithProblemDetailsOverride(problemDetails));
        }

        /// <summary>
        /// Fluently attaches a pre-built ProblemDetails object to the outcome's metadata for a
        /// generic outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The current generic endpoint outcome.</param>
        /// <param name="problemDetails">The ProblemDetails object to attach.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome{TValue}" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome<TValue> WithProblemDetails<TValue>(
        this IEndpointOutcome<TValue> outcome,
        ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(problemDetails, nameof(problemDetails));
            return outcome.WithMetadata(m => m.WithProblemDetailsOverride(problemDetails));
        }

        /// <summary>
        /// Fluently adds a custom HTTP header to the outcome's metadata.
        /// </summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="key">The header key (e.g., "X-Correlation-ID").</param>
        /// <param name="value">The header value.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome WithHeader(
            this IEndpointOutcome outcome,
            string key,
            string value)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            return outcome.WithMetadata(m => m.WithHeader(key, value));
        }

        /// <summary>
        /// Fluently adds a custom HTTP header to the outcome's metadata for a generic outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The current generic endpoint outcome.</param>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        /// <returns>
        /// A new <see cref = "IEndpointOutcome{TValue}" /> instance with the updated metadata.
        /// </returns>
        public static IEndpointOutcome<TValue> WithHeader<TValue>(
            this IEndpointOutcome<TValue> outcome,
            string key,
            string value)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            return outcome.WithMetadata(m => m.WithHeader(key, value));
        }

        /// <summary>Fluently sets the Location HTTP header on the outcome's metadata.</summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="uriString">The URI string for the Location header.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> instance with the updated metadata (or the original if URI is invalid).</returns>
        /// <remarks>
        /// If the provided URI string is invalid or empty, a warning is logged (if
        /// <see chref="ILogger" /> is available in metadata), and the header is not set.
        /// This method does not throw an exception for invalid URI strings.
        /// </remarks>
        [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerMessage delegates are not used here to keep the code simple and avoid additional boilerplate for a single log statement.")]
        public static IEndpointOutcome WithLocation(this IEndpointOutcome outcome, string uriString)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            var logger = outcome.Metadata.GetLogger();

            if (string.IsNullOrWhiteSpace(uriString))
            {
                logger?.LogWarning("WithLocation received null or empty URI string. Location header will not be set.");
                return outcome;
            }

            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            {
                logger?.LogWarning("WithLocation received invalid URI string: '{UriString}'. Location header will not be set.", uriString);
                return outcome;
            }

            return outcome.WithMetadata(m => m.WithLocation(uri));
        }

        /// <summary>Fluently sets the Location HTTP header on the outcome's metadata.</summary>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="uri">The URI string for the Location header.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> instance with the updated metadata (or the original if URI is invalid).</returns>
        /// <remarks>
        /// If the provided URI string is invalid or empty, a warning is logged (if
        /// <see chref="ILogger" /> is available in metadata), and the header is not set.
        /// This method does not throw an exception for invalid URI strings.
        /// </remarks>
        public static IEndpointOutcome WithLocation(this IEndpointOutcome outcome, Uri uri)
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));

            var logger = outcome.Metadata.GetLogger();
            return outcome.WithMetadata(m => m.WithLocation(uri));
        }

        /// <summary>
        /// Fluently sets the Location HTTP header on the outcome's metadata for a generic outcome.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The current generic endpoint outcome.</param>
        /// <param name="uriString">The URI string for the Location header.</param>
        /// <returns>
        /// A new <see cref="IEndpointOutcome{TValue}"/> instance with the updated metadata
        /// (or the original if URI is invalid).
        /// </returns>
        /// <remarks>
        /// Invalid URI strings are logged as warnings (if ILogger is available)
        /// and do not cause an exception.
        /// </remarks>
        [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerMessage delegates are not used here to keep the code simple and avoid additional boilerplate for a single log statement.")]
        public static IEndpointOutcome<TValue> WithLocation<TValue>(this IEndpointOutcome<TValue> outcome, string uriString)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(uriString, nameof(uriString));

            var logger = outcome.Metadata.GetLogger();

            if (string.IsNullOrWhiteSpace(uriString))
            {
                logger?.LogWarning("WithLocation received null or empty URI string. Location header will not be set.");
                return outcome;
            }

            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            {
                logger?.LogWarning("WithLocation received invalid URI string: '{UriString}'. Location header will not be set.", uriString);
                return outcome;
            }

            return outcome.WithMetadata(m => m.WithLocation(uri));
        }

        /// <summary>Fluently sets the Location HTTP header on the outcome's metadata.</summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The current endpoint outcome.</param>
        /// <param name="uri">The <see cref="Uri"/> for the Location header.</param>
        /// <returns>A new <see cref="IEndpointOutcome"/> instance with the updated metadata (or the original if URI is invalid).</returns>
        /// <remarks>
        /// If the provided URI string is invalid or empty, a warning is logged (if
        /// <see chref="ILogger" /> is available in metadata), and the header is not set.
        /// This method does not throw an exception for invalid URI strings.
        /// </remarks>
        public static IEndpointOutcome WithLocation<TValue>(this IEndpointOutcome<TValue> outcome, Uri uri)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(outcome, nameof(outcome));
            ArgumentNullException.ThrowIfNull(uri, nameof(uri));

            var logger = outcome.Metadata.GetLogger();
            return outcome.WithMetadata(m => m.WithLocation(uri));
        }

        /// <summary>
        /// Explicitly maps an <see chref="IEndpointOutcome" /> to a
        /// <see chref="Microsoft.AspNetCore.Http.IResult" /> using the registered
        /// <see chref="IEndpointOutcomeToHttpMapper" />. This is typically used when the
        /// <see chref="NormalizeEndpointOutcomeFilter" /> is not applied automatically.
        /// </summary>
        /// <param name="outcome">The endpoint outcome to map.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// A <see chref="Task" /> representing the asynchronous operation,
        /// returning the mapped <see chref="IResult" />.
        /// </returns>
        public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync(
            this IEndpointOutcome outcome,
            HttpContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            var mapper = context.RequestServices.GetRequiredService<IEndpointOutcomeToHttpMapper>();
            return await mapper.Map(outcome, context, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Explicitly maps a generic <see chref="IEndpointOutcome" /> to a
        /// <see chref="Microsoft.AspNetCore.Http.IResult" /> using the registered
        /// <see chref="IEndpointOutcomeToHttpMapper" />.
        /// </summary>
        /// <typeparam name="TValue">The type of the outcome's value.</typeparam>
        /// <param name="outcome">The generic endpoint outcome to map.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// A <see chref="Task" /> representing the asynchronous operation,
        /// returning the mapped <see chref="IResult" />.
        /// </returns>
        public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResultAsync<TValue>(
        this IEndpointOutcome<TValue> outcome,
        HttpContext context,
        CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            var mapper = context.RequestServices.GetRequiredService<IEndpointOutcomeToHttpMapper>();
            return await mapper.Map(outcome, context, cancellationToken).ConfigureAwait(false);
        }
    }
}
