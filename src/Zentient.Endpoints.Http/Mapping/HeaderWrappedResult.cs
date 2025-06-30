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
        private readonly ImmutableDictionary<string, string> _headers;
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
            ImmutableDictionary<string, string> headers,
            Uri? location)
        {
            this._innerResult = innerResult ?? throw new ArgumentNullException(nameof(innerResult));
            this._headers = headers ?? ImmutableDictionary<string, string>.Empty;
            this._location = location;
        }

        /// <summary>Gets the inner <see cref="IResult"/> that is wrapped by this result.</summary>
        /// <value>The inner <see cref="IResult"/> that is wrapped by this result.</value>
        internal IResult Result => this._innerResult;

        /// <summary>
        /// Executes the result asynchronously, applying headers and then the inner result.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

            if (!this._headers.IsEmpty)
            {
                foreach (var (key, value) in this._headers)
                {
                    httpContext.Response.Headers.Append(key, value);
                }
            }

            if (this._location is not null)
            {
                httpContext.Response.Headers.Location = this._location.OriginalString;
            }

            await this._innerResult.ExecuteAsync(httpContext).ConfigureAwait(false);
        }
    }
}
