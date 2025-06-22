// <copyright file="JsonResult.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zentient.Endpoints.Http
{
    /// <summary>
    /// Represents an <see cref="Microsoft.AspNetCore.Http.IResult"/> that returns JSON content using System.Text.Json.
    /// </summary>
    internal sealed class JsonResult : IResult
    {
        private readonly object? _value;
        private readonly int? _statusCode;
        private readonly string? _contentType;
        private readonly JsonSerializerOptions? _serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonResult"/> class.
        /// </summary>
        /// <param name="value">The object to be serialized to JSON.</param>
        /// <param name="statusCode">Optional: The HTTP status code to set for the response. Defaults to 200 OK.</param>
        /// <param name="contentType">Optional: The content type for the response. Defaults to "application/json" or "application/problem+json" for ProblemDetails.</param>
        /// <param name="serializerOptions">Optional: <see cref="JsonSerializerOptions"/> to use for serialization.</param>
        public JsonResult(
            object? value,
            int? statusCode = null,
            string? contentType = null,
            JsonSerializerOptions? serializerOptions = null)
        {
            this._value = value;
            this._statusCode = statusCode;
            this._contentType = contentType;
            this._serializerOptions = serializerOptions;
        }

        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        /// <value>The HTTP status code, defaults to 200 OK if not specified.</value>
        public int StatusCode => this._statusCode ?? StatusCodes.Status200OK;

        /// <inheritdoc />
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (this._statusCode.HasValue)
            {
                httpContext.Response.StatusCode = this._statusCode.Value;
            }

            if (!string.IsNullOrEmpty(this._contentType))
            {
                httpContext.Response.ContentType = this._contentType;
            }
            else if (this._value is ProblemDetails)
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;
            }
            else
            {
                httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            }

            if (this._value != null)
            {
                await JsonSerializer.SerializeAsync(
                    httpContext.Response.Body,
                    this._value,
                    this._value.GetType(),
                    this._serializerOptions,
                    httpContext.RequestAborted)
                .ConfigureAwait(false);
            }
        }
    }
}
