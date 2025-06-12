// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmptyResultWithStatusCode.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace Zentient.Endpoints.Http
{
    /// <summary>Represents an empty HTTP result with a specified status code.</summary>
    public class EmptyResultWithStatusCode : IResult
    {
        private readonly int _statusCode;
        private readonly string? _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyResultWithStatusCode"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to set in the response.</param>
        /// <param name="contentType">The Content-Type header value for the response.</param>
        public EmptyResultWithStatusCode(
            int statusCode,
            string? contentType = null)
        {
            this._statusCode = statusCode;
            this._contentType = contentType;
        }

        /// <summary>Gets the HTTP status code for this result.</summary>
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
