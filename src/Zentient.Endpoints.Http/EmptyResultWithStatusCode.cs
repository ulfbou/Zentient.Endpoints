// <copyright file="EmptyResultWithStatusCode.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

// TODO: Consider renaming this class to `NoContentResponse` or `EmptyResponseResult` for clarity.
// TODO: Consider whether this class should provide an optional message in the HttpContext.Response.Body
namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Represents an <see cref="Microsoft.AspNetCore.Http.IResult"/> that returns an empty response
    /// with a specific HTTP status code and content type.
    /// </summary>
    internal sealed class EmptyResultWithStatusCode : Microsoft.AspNetCore.Http.IResult
    {
        private readonly int _statusCode;
        private readonly string? _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultWithStatusCode"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code for the response.</param>
        /// <param name="contentType">Optional: The content type for the response. Defaults to "application/json".</param>
        public EmptyResultWithStatusCode(
            int statusCode,
            string? contentType = null)
        {
            this._statusCode = statusCode;
            this._contentType = contentType;
        }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        /// <value>The HTTP status code.</value>
        public int StatusCode => this._statusCode;

        /// <inheritdoc />
        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            httpContext.Response.StatusCode = this._statusCode;

            if (!string.IsNullOrEmpty(this._contentType))
            {
                httpContext.Response.ContentType = this._contentType;
            }
            else
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            }

            return Task.CompletedTask;
        }
    }
}
