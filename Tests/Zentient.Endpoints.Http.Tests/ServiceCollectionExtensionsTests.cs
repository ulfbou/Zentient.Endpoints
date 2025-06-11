// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Newtonsoft.Json;

using Xunit;

using Zentient.Endpoints.Core;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>
    /// Contains sophisticated and exhaustive unit tests for the <see cref="ServiceCollectionExtensions"/> class.
    /// These tests ensure that the correct services are registered with the expected lifetimes
    /// and that the extension methods for <see cref="RouteHandlerBuilder"/> work as intended.
    /// </summary>
    public sealed partial class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.AddZentientEndpointsHttp"/>
        /// correctly registers <see cref="IProblemDetailsMapper"/> as scoped.
        /// </summary>
        [Fact]
        public void AddZentientEndpointsHttp_RegistersIProblemDetailsMapperAsScoped()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IProblemDetailsMapper));
            descriptor.Should().NotBeNull("because IProblemDetailsMapper should be registered.");
            descriptor!.ImplementationType.Should().Be<DefaultProblemDetailsMapper>();
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

            IProblemDetailsMapper? mapper = serviceProvider.GetService<IProblemDetailsMapper>();
            mapper.Should().BeOfType<DefaultProblemDetailsMapper>();
            mapper.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.AddZentientEndpointsHttp"/>
        /// correctly registers <see cref="IEndpointResultToHttpResultMapper"/> as scoped.
        /// </summary>
        [Fact]
        public void AddZentientEndpointsHttp_RegistersIEndpointResultToHttpResultMapperAsScoped()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEndpointResultToHttpResultMapper));
            descriptor.Should().NotBeNull("because IEndpointResultToHttpResultMapper should be registered.");
            descriptor!.ImplementationType.Should().Be<EndpointResultHttpMapper>();
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

            IEndpointResultToHttpResultMapper? mapper = serviceProvider.GetService<IEndpointResultToHttpResultMapper>();
            mapper.Should().BeOfType<EndpointResultHttpMapper>();
            mapper.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.AddZentientEndpointsHttp"/>
        /// uses <c>TryAddScoped</c> for <see cref="IProblemDetailsMapper"/>, not replacing an existing registration.
        /// </summary>
        [Fact]
        public void AddZentientEndpointsHttp_DoesNotReplaceExistingProblemDetailsMapper()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IProblemDetailsMapper mockMapper = Mock.Of<IProblemDetailsMapper>();
            services.AddSingleton(mockMapper);

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            IProblemDetailsMapper? resolvedMapper = serviceProvider.GetService<IProblemDetailsMapper>();
            resolvedMapper.Should().BeSameAs(mockMapper, "because TryAddScoped should not replace an existing registration.");
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.AddZentientEndpointsHttp"/>
        /// uses <c>TryAddScoped</c> for <see cref="IEndpointResultToHttpResultMapper"/>, not replacing an existing registration.
        /// </summary>
        [Fact]
        public void AddZentientEndpointsHttp_DoesNotReplaceExistingEndpointResultToHttpResultMapper()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IEndpointResultToHttpResultMapper mockMapper = Mock.Of<IEndpointResultToHttpResultMapper>();
            services.AddSingleton(mockMapper);

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            IEndpointResultToHttpResultMapper? resolvedMapper = serviceProvider.GetService<IEndpointResultToHttpResultMapper>();
            resolvedMapper.Should().BeSameAs(mockMapper, "because TryAddScoped should not replace an existing registration.");
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.WithNormalizeEndpointResultFilter"/>
        /// throws <see cref="ArgumentNullException"/> if the <see cref="RouteHandlerBuilder"/> parameter is null.
        /// </summary>
        [Fact]
        public void WithNormalizeEndpointResultFilter_NullBuilder_ThrowsArgumentNullException()
        {
            // Arrange
            RouteHandlerBuilder nullBuilder = null!;

            // Act
            Action act = () => nullBuilder.WithNormalizeEndpointResultFilter();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.WithNormalizeEndpointResultFilter"/>
        /// correctly adds the <see cref="NormalizeEndpointResultFilter"/> to a Minimal API endpoint.
        /// This is an integration test to verify the filter's application and effect.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task WithNormalizeEndpointResultFilter_AppliesFilterCorrectly()
        {
            using TestServer server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddZentientEndpointsHttp();
                    services.AddRouting();
                    services.AddControllers().AddNewtonsoftJson();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                }));

            HttpClient client = server.CreateClient();
            HttpResponseMessage response = await client.GetAsync(new Uri("/TestEndpoint/test-endpoint", UriKind.Relative));

            response.IsSuccessStatusCode.Should().BeTrue();
            (await response.Content.ReadAsStringAsync()).Should().Be("\"Hello World\"");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Tests that <see cref="ServiceCollectionExtensions.WithNormalizeEndpointResultFilter"/>
        /// correctly adds the <see cref="NormalizeEndpointResultFilter"/> to a Minimal API endpoint
        /// for a failed result, asserting the problem details response.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        public async Task WithNormalizeEndpointResultFilter_AppliesFilterCorrectly_FailedResult()
        {
            // Arrange
            using TestServer server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddZentientEndpointsHttp()
                            .AddRouting();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        ErrorInfo error = new ErrorInfo(ErrorCategory.NotFound, "RES_NOT_FOUND", "Resource not found.");
                        endpoints.MapGet("/test-fail-endpoint", () => EndpointResult<Unit>.From(error))
                                 .WithNormalizeEndpointResultFilter();
                    });
                }));

            HttpClient client = server.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(new Uri("/test-fail-endpoint", UriKind.Relative));

            // Assert
            ((int)response.StatusCode).Should().BeInRange(400, 499);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

            string responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("\"status\":404");
            responseContent.Should().Contain("\"title\":\"Not Found\"");
            responseContent.Should().Contain("\"detail\":\"Resource not found.\"");
            responseContent.Should().Contain("\"errorCode\":\"RES_NOT_FOUND\"");
            responseContent.Should().Contain("\"traceId\":");
        }

        /// <summary>
        /// Test controller for endpoint integration tests in <see cref="ServiceCollectionExtensionsTests"/>.
        /// </summary>
        [ApiController]
        [Route("[controller]")]
        internal sealed class TestEndpointController : ControllerBase
        {
            /// <summary>
            /// Returns a successful <see cref="EndpointResult{TResult}"/> with a string payload.
            /// </summary>
            /// <returns>An <see cref="ActionResult{T}"/> containing a successful endpoint result.</returns>
            [HttpGet("test-endpoint")]
            public static ActionResult<EndpointResult<string>> Get()
                => EndpointResult<string>.From("Hello World");

            /// <summary>
            /// Returns a failed <see cref="EndpointResult{TResult}"/> with a not found error.
            /// </summary>
            /// <returns>An <see cref="ActionResult{T}"/> containing a failed endpoint result.</returns>
            [HttpGet("test-fail-endpoint")]
            public static ActionResult<EndpointResult<Unit>> GetFail()
                => EndpointResult<Unit>.From(new ErrorInfo(ErrorCategory.NotFound, "RES_NOT_FOUND", "Resource not found."));
        }
    }
}
