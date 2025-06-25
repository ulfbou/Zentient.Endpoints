// <copyright file="HeaderWrappedResult.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Zentient.Endpoints.Http.Mapping
{
    /// <summary>
    /// An internal <see cref="IResult"/> wrapper that applies custom headers and a Location URI
    /// to the HTTP response before executing an inner <see cref="IResult"/>.
    /// </summary>
    internal sealed class HeaderWrappedResult : IResult
    {
        private readonly IResult _innerResult;
        private readonly ImmutableList<KeyValuePair<string, string>> _headers;
        private readonly Uri? _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderWrappedResult"/> class.
        /// </summary>
        /// <param name="innerResult">
        /// The inner <see cref="IResult"/> to execute after applying headers.
        /// </param>
        /// <param name="headers">The immutable list of headers to apply. Can be empty.</param>
        /// <param name="location">The Location URI to apply. Can be null.</param>
        public HeaderWrappedResult(
            IResult innerResult,
            ImmutableList<KeyValuePair<string, string>> headers, Uri? location)
        {
            _innerResult = innerResult ?? throw new ArgumentNullException(nameof(innerResult));
            _headers = headers ?? ImmutableList<KeyValuePair<string, string>>.Empty;
            _location = location;
        }

        /// <summary>
        /// Executes the result asynchronously, applying headers and then the inner result.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (!_headers.IsEmpty)
            {
                foreach (var (key, value) in _headers)
                {
                    httpContext.Response.Headers.Append(key, value);
                }
            }

            if (_location is not null)
            {
                httpContext.Response.Headers.Location = _location.OriginalString;
            }

            await _innerResult.ExecuteAsync(httpContext).ConfigureAwait(false);
        }
    }
}
