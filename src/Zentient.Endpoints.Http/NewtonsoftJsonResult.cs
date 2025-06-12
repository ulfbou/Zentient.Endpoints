// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewtonsoftJsonResult.cs" company="Zentient Framework Team">
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
    /// <summary>
    /// A custom <see cref="IResult"/> implementation that serializes a given object
    /// to the HTTP response using Newtonsoft.Json.
    /// </summary>
    public class NewtonsoftJsonResult : IResult
    {
        private readonly object? _value;
        private readonly int? _statusCode;
        private readonly string? _contentType;
        private readonly JsonSerializerSettings? _serializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonResult"/> class.
        /// </summary>
        /// <param name="value">The object to serialize to the response body.</param>
        /// <param name="statusCode">The HTTP status code to set for the response.</param>
        /// <param name="contentType">The Content-Type header value for the response.</param>
        /// <param name="serializerSettings">Optional Newtonsoft.Json serializer settings.</param>
        public NewtonsoftJsonResult(
            object? value,
            int? statusCode = null,
            string? contentType = null,
            JsonSerializerSettings? serializerSettings = null)
        {
            this._value = value;
            this._statusCode = statusCode;
            this._contentType = contentType;
            this._serializerSettings = serializerSettings;
        }

        /// <summary>Gets the HTTP status code for this result.</summary>
        /// <value>The HTTP status code.</value>
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
                string json = JsonConvert.SerializeObject(this._value, this._serializerSettings);
                await httpContext.Response.WriteAsync(json, httpContext.RequestAborted).ConfigureAwait(false);
            }
        }
    }
}
