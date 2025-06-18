// <copyright file="EndpointOutcomeExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Zentient.Endpoints;
using Zentient.Results;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointOutcome"/> and <see cref="EndpointOutcome{TResult}"/>
    /// to facilitate integration with ASP.NET Core HTTP responses.
    /// </summary>
    public static class EndpointOutcomeExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEndpointOutcome"/> to a <see cref="Microsoft.AspNetCore.Http.IResult"/>
        /// asynchronously using the registered <see cref="IEndpointOutcomeToHttpMapper"/>.
        /// </summary>
        /// <param name="endpointResult">The endpoint result to convert.</param>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the mapped HTTP response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpointResult"/> or <paramref name="httpContext"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IEndpointOutcomeToHttpMapper"/> is not registered in the service provider.</exception>
        public static async Task<Microsoft.AspNetCore.Http.IResult> ToHttpResult(this IEndpointOutcome endpointResult, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            IEndpointOutcomeToHttpMapper mapper = httpContext.RequestServices.GetRequiredService<IEndpointOutcomeToHttpMapper>();
            return await mapper.Map(endpointResult, httpContext).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the original <see cref="EndpointOutcome{TResult}"/> instance.
        /// This extension method primarily exists for conceptual clarity when working with Minimal APIs.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <param name="endpointResult">The endpoint result to return.</param>
        /// <returns>The original <see cref="EndpointOutcome{TResult}"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="endpointResult"/> is <c>null</c>.</exception>
        public static IEndpointOutcome<TResult> ToMinimalApiResult<TResult>(this IEndpointOutcome<TResult> endpointResult)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            return endpointResult;
        }

        /// <summary>
        /// Converts a <see cref="Zentient.Results.IResult{TValue}"/> to an <see cref="EndpointOutcome{TValue}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the result value.</typeparam>
        /// <param name="result">The business result to convert.</param>
        /// <param name="transportMetadata">Optional transport metadata to associate with the outcome.</param>
        /// <returns>An <see cref="EndpointOutcome{TValue}"/> wrapping the business result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IEndpointOutcome<TValue> ToEndpointOutcome<TValue>(this IResult<TValue> result, TransportMetadata? transportMetadata = null)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return EndpointOutcome<TValue>.From(result, transportMetadata);
        }

        /// <summary>
        /// Converts a non-generic <see cref="Zentient.Results.IResult"/> to an <see cref="IEndpointOutcome{Unit}"/>.
        /// This ensures all messages and the original status from the <see cref="Zentient.Results.IResult"/> are carried over.
        /// </summary>
        /// <param name="result">The non-generic business result to convert.</param>
        /// <param name="transportMetadata">Optional transport metadata to associate with the outcome.</param>
        /// <returns>An <see cref="IEndpointOutcome{Unit}"/> representing the outcome.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is <c>null</c>.</exception>
        public static IEndpointOutcome<Unit> ToEndpointOutcome(this Zentient.Results.IResult result, TransportMetadata? transportMetadata = null)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess)
            {
                // Create an IResult<Unit> from the non-generic IResult, preserving messages and status.
                // Then, use EndpointOutcome<Unit>.From which correctly wraps this IResult<Unit>.
                IResult<Unit> unitResultWithOriginalInfo = Result<Unit>.Success(
                    Unit.Value,
                    status: result.Status,
                    messages: result.Messages);

                return EndpointOutcome<Unit>.From(unitResultWithOriginalInfo, transportMetadata);
            }

            ErrorInfo error = result.Errors != null && result.Errors.Any()
                ? result.Errors[0]
                : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unknown error occurred.");

            IResult<Unit> failureResultWithOriginalInfo = Result<Unit>.Failure(
                default(Unit),
                error,
                result.Status);
            return EndpointOutcome<Unit>.From(failureResultWithOriginalInfo, transportMetadata);
        }
    }
}
