// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
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
using Newtonsoft.Json.Linq;

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
        /// correctly registers <see cref="IProblemTypeUriGenerator"/> as scoped.
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
        /// Verifies that <see cref="ServiceCollectionExtensions.WithNormalizeEndpointResultFilter"/>
        /// correctly adds the <see cref="NormalizeEndpointResultFilter"/> to a Minimal API endpoint.
        /// This is an integration test to verify the filter's application and effect.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
        [Fact]
        [SuppressMessage("Minor Code Smell", "CA1506:Avoid excessive class coupling", Justification = "Integration test inherently has high coupling.")]
        public async Task WithNormalizeEndpointResultFilter_AppliesFilterCorrectly()
        {
            // Arrange
            // FIX "Use explicit type instead of 'var'": Use WebHostBuilder
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddZentientEndpointsHttp(); // Only register the services here
                    services.AddRouting();
                    services.AddControllers()
                            .AddNewtonsoftJson()
                            .AddApplicationPart(Assembly.GetExecutingAssembly());
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        // Apply the filter to the specific Minimal API endpoint
                        endpoints.MapGet("/test-endpoint", () => EndpointResult<string>.From("Hello World"))
                                 .WithNormalizeEndpointResultFilter(); // Apply the filter here
                    });
                });

            using TestServer server = new TestServer(hostBuilder);
            using HttpClient client = server.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(new Uri("/test-endpoint", UriKind.Relative));

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue("because the EndpointResult should be successfully converted to HTTP 200 OK.");
            (await response.Content.ReadAsStringAsync()).Should().Be("\"Hello World\"", "because the filter should have mapped the EndpointResult to the correct JSON string.");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Verifies that <see cref="ServiceCollectionExtensions.WithNormalizeEndpointResultFilter"/>
        /// correctly handles a failed <see cref="EndpointResult{TResult}"/> by mapping it to <see cref="ProblemDetails"/>.
        /// This test uses <see cref="TestServer"/> for end-to-end validation.
        /// </summary>
        [Fact]
        [SuppressMessage("Minor Code Smell", "CA1506:Avoid excessive class coupling", Justification = "Integration test inherently has high coupling.")]
        public async Task WithNormalizeEndpointResultFilter_AppliesFilterCorrectly_FailedResult()
        {
            const string ResNotFound = "RES_NOT_FOUND";
            const string ResNotFoundDescription = "Resource not found.";
            const string TestFailEndpoint = "/test-fail-endpoint";

            // Arrange
            ErrorInfo errorInfo = new ErrorInfo(ErrorCategory.NotFound, ResNotFound, ResNotFoundDescription);
            var endpointResult = EndpointResult<object>.From(errorInfo);
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers()
                            .AddNewtonsoftJson()
                            .AddApplicationPart(Assembly.GetExecutingAssembly());

                    services.AddZentientEndpointsHttp();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/test-fail-endpoint", () => endpointResult)
                                 .WithNormalizeEndpointResultFilter();
                    });
                });

            using TestServer server = new TestServer(hostBuilder);
            using HttpClient client = server.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(new Uri(TestFailEndpoint, UriKind.Relative));
            string responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/problem+json");

            Dictionary<string, object?>? jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object?>>(responseBody);
            jsonObject.Should().NotBeNull();

            jsonObject.Should().ContainKey(ProblemDetailsConstants.Status);
            jsonObject[ProblemDetailsConstants.Status]
                .Should().Be(ResultStatuses.NotFound.Code, "because the status code should be mapped as a top-level property.");

            jsonObject.Should().ContainKey(ProblemDetailsConstants.Title);
            jsonObject[ProblemDetailsConstants.Title]
                .Should().Be(ResultStatuses.NotFound.Description, "because the title should be mapped as a top-level property.");

            jsonObject.Should().ContainKey(ProblemDetailsConstants.Detail);
            jsonObject[ProblemDetailsConstants.Detail]
                .Should().Be(ResNotFoundDescription, "because the detail should be mapped as a top-level property.");

            jsonObject.Should().ContainKey(ProblemDetailsConstants.Instance);
            jsonObject[ProblemDetailsConstants.Title]
                .Should().Be(ResultStatuses.NotFound.Description, "because the title should be mapped as a top-level property.");

            // Extensions properties
            jsonObject.Should().ContainKey(nameof(ProblemDetailsConstants.Extensions));

            var extensions = jsonObject[nameof(ProblemDetailsConstants.Extensions)] as Newtonsoft.Json.Linq.JObject;
            extensions.Should().NotBeNull();

            extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            extensions[ProblemDetailsConstants.Extensions.ErrorCode]!
                .Value<string>()
                .Should().Be(ResNotFound, "because the error code should be mapped as an extension.");

            extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.TraceId);
            extensions[ProblemDetailsConstants.Extensions.TraceId]!
                .Value<string>()
                .Should().NotBeNull("because the trace identifier should be included in the response for diagnostics.");
        }
    }
}

/// <summary>
/// A dummy controller for testing purposes within TestServer.
/// This needs to be public for discovery by AddApplicationPart.
/// </summary>
[ApiController]
[Route("[controller]")]
internal sealed class TestEndpointController : ControllerBase
{
    /// <summary>
    /// Static property to hold the next <see cref="IEndpointResult"/> for test cases.
    /// </summary>
    public static IEndpointResult? NextEndpointResultForTest { get; set; }

    /// <summary>
    /// Test endpoint that returns a successful <see cref="EndpointResult{TResult}"/>.
    /// </summary>
    /// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="EndpointResult{TResult}"/> with a string value.</returns>
    [HttpGet("test-endpoint")]
    public static ActionResult<EndpointResult<string>> GetTestEndpoint()
    {
        // For the first test, return a successful EndpointResult
        if (NextEndpointResultForTest is null)
        {
            // Default success for the first test, if not explicitly set
            return EndpointResult<string>.From("Hello World");
        }
        // For the failed test, return the pre-configured error result
        return NextEndpointResultForTest as EndpointResult<string> ?? EndpointResult<string>.From("Fallback Success");
    }

    /// <summary>
    /// Test endpoint that simulates a failure by returning a pre-set <see cref="IEndpointResult"/> for testing error handling.
    /// </summary>
    /// <returns>An <see cref="ActionResult{T}"/> containing the next <see cref="IEndpointResult"/> for test failure.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="NextEndpointResultForTest"/> is not set.</exception>
    [HttpGet("test-fail-endpoint")]
    public static ActionResult<IEndpointResult> GetTestFailEndpoint()
    {
        // Always return the static value for the failed test, which should be pre-set.
        if (TestEndpointController.NextEndpointResultForTest is null)
        {
            throw new InvalidOperationException("NextEndpointResultForTest was not set for failure test.");
        }
        return new ActionResult<IEndpointResult>(TestEndpointController.NextEndpointResultForTest);
    }

    // Reset static state after each test
    public TestEndpointController()
    {
        NextEndpointResultForTest = null;
    }
}
