// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointResultExtensions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Zentient.Endpoints.Core;
using Zentient.Results;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointResult"/> and <see cref="EndpointResult{TResult}"/>
    /// to facilitate integration with ASP.NET Core HTTP responses.
    /// </summary>
    public static class EndpointResultExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEndpointResult"/> into an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>
        /// by resolving and using the registered <see cref="IEndpointResultToHttpResultMapper"/>.
        /// This method requires an <see cref="Microsoft.AspNetCore.Http.HttpContext"/> to resolve services.
        /// </summary>
        /// <remarks>
        /// This extension is primarily for direct use in scenarios where an <see cref="Microsoft.AspNetCore.Http.HttpContext"/>
        /// is available and a direct conversion is desired (e.g., in legacy controller actions
        /// or complex middleware scenarios not handled by <see cref="NormalizeEndpointResultFilter"/>).
        /// </remarks>
        /// <param name="endpointResult">The endpoint result to convert.</param>
        /// <param name="httpContext">The current <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.</param>
        /// <returns>An <see cref="Microsoft.AspNetCore.Http.IResult"/> representation of the endpoint result.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="IEndpointResultToHttpResultMapper"/> cannot be resolved from the
        /// <see cref="Microsoft.AspNetCore.Http.HttpContext"/>'s service provider.
        /// </exception>
        public static Microsoft.AspNetCore.Http.IResult ToHttpResult(this IEndpointResult endpointResult, HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            return ((IEndpointResult)endpointResult).ToHttpResult(httpContext);
        }
        /// <summary>
        /// Converts an <see cref="EndpointResult{TResult}"/> into an ASP.NET Core <see cref="Microsoft.AspNetCore.Http.IResult"/>
        /// suitable for Minimal API handlers. This method leverages the global
        /// <see cref="NormalizeEndpointResultFilter"/> via `AddEndpointFilter`.
        /// </summary>
        /// <remarks>
        /// This extension is primarily for ergonomic use in Minimal API endpoint definitions.
        /// The actual mapping happens later in the pipeline via the registered filter.
        /// </remarks>
        /// <typeparam name="TResult">The type of the value in the endpoint result.</typeparam>
        /// <param name="endpointResult">The endpoint result to convert.</param>
        /// <returns>The original <see cref="EndpointResult{TResult}"/> instance. The conversion to
        /// <see cref="Microsoft.AspNetCore.Http.IResult"/>is handled by a globally registered
        /// <see cref="NormalizeEndpointResultFilter"/>.</returns>
        public static EndpointResult<TResult> ToMinimalApiResult<TResult>(this EndpointResult<TResult> endpointResult)
            where TResult : notnull
        {
            ArgumentNullException.ThrowIfNull(endpointResult, nameof(endpointResult));

            return endpointResult;
        }

        /// <summary>
        /// Converts an <see cref="Zentient.Results.IResult{TValue}"/> from Zentient.Results into an <see cref="EndpointResult{TValue}"/>
        /// ready for the HTTP pipeline.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the result.</typeparam>
        /// <param name="result">The Zentient.Results IResult instance.</param>
        /// <returns>An <see cref="EndpointResult{TValue}"/> instance.</returns>
        public static EndpointResult<TValue> ToEndpointResult<TValue>(this Zentient.Results.IResult<TValue> result)
            where TValue : notnull
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));
            return Core.EndpointResult<TValue>.From(result);
        }

        /// <summary>
        /// Converts a Zentient.Results IResult (non-generic) into an <see cref="EndpointResult{Unit}"/>
        /// ready for the HTTP pipeline. This is for operations that don't return a specific value.
        /// </summary>
        /// <param name="result">The Zentient.Results IResult instance.</param>
        /// <returns>An <see cref="EndpointResult{Unit}"/> instance.</returns>
        public static EndpointResult<Core.Unit> ToEndpointResult(this Zentient.Results.IResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.IsSuccess)
            {
                return Core.EndpointResult<Core.Unit>.From(Core.Unit.Value);
            }

            ErrorInfo error = result.Errors != null && result.Errors.Count > 0
                              ? result.Errors[0]
                              : new ErrorInfo(ErrorCategory.InternalServerError, code: "InternalError", message: "An unknown error occurred.");

            return Core.EndpointResult<Core.Unit>.From(error);
        }
    }
}
