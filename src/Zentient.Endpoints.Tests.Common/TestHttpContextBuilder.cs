// <copyright file="TestHttpContextBuilder.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

// File: src/Zentient.Endpoints.Tests.Common/TestHttpContextBuilder.cs

using System;
using System.IO;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// A fluent builder for creating <see cref="DefaultHttpContext"/> instances for comprehensive testing scenarios.
    /// </summary>
    internal sealed class TestHttpContextBuilder
    {
        private readonly DefaultHttpContext _context = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHttpContextBuilder"/> class.
        /// </summary>
        public TestHttpContextBuilder()
        {
            _context.Response.Body = new MemoryStream();
            _context.Request.Body = new MemoryStream();
        }

        /// <summary>
        /// Configures the HTTP method of the request.
        /// </summary>
        public TestHttpContextBuilder WithMethod(string method)
        {
            _context.Request.Method = method;
            return this;
        }

        /// <summary>
        /// Configures the request URL scheme (e.g., "http", "https").
        /// </summary>
        public TestHttpContextBuilder WithScheme(string scheme)
        {
            _context.Request.Scheme = scheme;
            return this;
        }

        /// <summary>
        /// Configures the request host.
        /// </summary>
        public TestHttpContextBuilder WithHost(string host)
        {
            _context.Request.Host = new HostString(host);
            return this;
        }

        /// <summary>
        /// Configures the path base for the request (e.g., "/api/v1").
        /// </summary>
        public TestHttpContextBuilder WithPathBase(string pathBase)
        {
            _context.Request.PathBase = new PathString(pathBase);
            return this;
        }

        /// <summary>
        /// Configures the request path (e.g., "/products/123").
        /// </summary>
        public TestHttpContextBuilder WithPath(string path)
        {
            _context.Request.Path = new PathString(path);
            return this;
        }

        /// <summary>
        /// Configures the services available via <see cref="HttpContext.RequestServices"/>.
        /// </summary>
        public TestHttpContextBuilder WithServices(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            _context.RequestServices = services.BuildServiceProvider();
            return this;
        }

        /// <summary>
        /// Adds a specific service to the <see cref="HttpContext.RequestServices"/>.
        /// </summary>
        public TestHttpContextBuilder WithService<TService>(TService instance) where TService : class
        {
            var services = new ServiceCollection();
            services.AddSingleton(instance);
            _context.RequestServices = services.BuildServiceProvider();
            return this;
        }

        /// <summary>
        /// Configures the response status code.
        /// </summary>
        public TestHttpContextBuilder WithResponseStatusCode(int statusCode)
        {
            _context.Response.StatusCode = statusCode;
            return this;
        }

        /// <summary>
        /// Configures the response content type.
        /// </summary>
        public TestHttpContextBuilder WithResponseContentType(string contentType)
        {
            _context.Response.ContentType = contentType;
            return this;
        }

        /// <summary>
        /// Configures a specific response header.
        /// </summary>
        public TestHttpContextBuilder WithResponseHeader(string key, string value)
        {
            _context.Response.Headers[key] = value;
            return this;
        }

        /// <summary>
        /// Configures the request body.
        /// </summary>
        public TestHttpContextBuilder WithRequestBody(string bodyContent)
        {
            var bytes = Encoding.UTF8.GetBytes(bodyContent);
            _context.Request.Body.SetLength(0);
            _context.Request.Body.Write(bytes, 0, bytes.Length);
            _context.Request.Body.Position = 0;
            _context.Request.ContentLength = bytes.Length;
            return this;
        }

        /// <summary>
        /// Configures the trace identifier for the HttpContext.
        /// </summary>
        public TestHttpContextBuilder WithTraceIdentifier(string traceIdentifier)
        {
            _context.TraceIdentifier = traceIdentifier;
            return this;
        }

        /// <summary>
        /// Builds and returns the <see cref="DefaultHttpContext"/> instance.
        /// </summary>
        public HttpContext Build() => _context;
    }
}
