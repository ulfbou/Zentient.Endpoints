// <copyright file="ServiceCollectionExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

using Zentient.Results;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public sealed partial class ServiceCollectionExtensionsTests
    {
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

        [Fact]
        public void AddZentientEndpointsHttp_RegistersIEndpointOutcomeToHttpResultMapperAsScoped()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            ServiceDescriptor? descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IEndpointOutcomeToHttpMapper));
            descriptor.Should().NotBeNull("because IEndpointOutcomeToHttpResultMapper should be registered.");
            descriptor!.ImplementationType.Should().Be<EndpointOutcomeHttpMapper>();
            descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

            IEndpointOutcomeToHttpMapper? mapper = serviceProvider.GetService<IEndpointOutcomeToHttpMapper>();
            mapper.Should().BeOfType<EndpointOutcomeHttpMapper>();
            mapper.Should().NotBeNull();
        }

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

        [Fact]
        public void AddZentientEndpointsHttp_DoesNotReplaceExistingEndpointOutcomeToHttpResultMapper()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IEndpointOutcomeToHttpMapper mockMapper = Mock.Of<IEndpointOutcomeToHttpMapper>();
            services.AddSingleton(mockMapper);

            // Act
            services.AddZentientEndpointsHttp();
            using ServiceProvider serviceProvider = services.BuildServiceProvider();

            // Assert
            IEndpointOutcomeToHttpMapper? resolvedMapper = serviceProvider.GetService<IEndpointOutcomeToHttpMapper>();
            resolvedMapper.Should().BeSameAs(mockMapper, "because TryAddScoped should not replace an existing registration.");
        }

        [Fact]
        public void WithNormalizeEndpointOutcomeFilter_NullBuilder_ThrowsArgumentNullException()
        {
            // Arrange
            RouteHandlerBuilder nullBuilder = null!;

            // Act
            Action act = () => nullBuilder.WithNormalizeEndpointOutcomeFilter();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("builder");
        }

        [Fact]
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Integration test for filter application requires setting up a full ASP.NET Core host, involving multiple types.")]
        public async Task WithNormalizeEndpointOutcomeFilter_AppliesFilterCorrectly()
        {
            // Arrange
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddZentientEndpointsHttp();
                    services.AddRouting();
                    services.AddControllers()
                            .AddApplicationPart(Assembly.GetExecutingAssembly());
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        IResult<string> result = Result<string>.Success("Hello World");
                        endpoints.MapGet("/test-endpoint", () => EndpointOutcome<string>.From(result))
                                 .WithNormalizeEndpointOutcomeFilter();
                    });
                });

            using TestServer server = new TestServer(hostBuilder);
            using HttpClient client = server.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(new Uri("/test-endpoint", UriKind.Relative));
            string responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue("because the EndpointOutcome should be successfully converted to HTTP 200 OK.");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            SuccessResponse<string>? deserializedResponse = JsonSerializer.Deserialize<SuccessResponse<string>>(responseBody);
            deserializedResponse.Should().NotBeNull("because the response body should be a deserializable SuccessResponse.");
            deserializedResponse!.Data.Should().Be("Hello World", "because the filter should have mapped the EndpointOutcome's value into the 'data' field.");
            deserializedResponse.StatusCode.Should().Be(ResultStatuses.Success.Code, "because the status code in the response object should match the mapped HTTP status.");
            deserializedResponse.StatusDescription.Should().Be(ResultStatuses.Success.Description, "because the status description in the response object should reflect the success.");
            deserializedResponse.Messages.Should().BeEmpty("because no messages were provided in the successful result.");

            response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypeNames.Application.Json);
        }

        [Fact]
        public async Task WithNormalizeEndpointOutcomeFilter_AppliesFilterCorrectly_FailedResult()
        {
            const string ResNotFound = "RES_NOT_FOUND";
            const string ResNotFoundDescription = "Resource not found.";
            const string TestFailEndpoint = "/test-fail-endpoint";
            const string CustomExtensionKey = "customTestProperty";
            const string CustomExtensionValue = "This is a custom test value.";

            // Arrange: Create ErrorInfo with explicit metadata to ensure 'extensions' are present.
            // According to RFC 9457, extensions are arbitrary members.
            var errorInfo = new ErrorInfo(
                category: ErrorCategory.NotFound,
                code: ResNotFound,
                message: ResNotFoundDescription,
                detail: null, // Keep null as message maps to ProblemDetails.Detail
                metadata: new Dictionary<string, object?> {
                    { CustomExtensionKey, CustomExtensionValue }
                }.ToImmutableDictionary());
            IEndpointOutcome<object> endpointResult = EndpointOutcome<object>.From(errorInfo);

            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddControllers(options =>
                    {
                        // You can add any MVC options here if needed, e.g., options.Filters.Add(...)
                    })
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                        options.JsonSerializerOptions.WriteIndented = true; // Still useful for debugging raw output
                    })
                    .AddApplicationPart(Assembly.GetExecutingAssembly()); // Only call this once

                    services.AddZentientEndpointsHttp();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/test-fail-endpoint", () => endpointResult)
                                 .WithNormalizeEndpointOutcomeFilter();
                    });
                });

            using TestServer server = new TestServer(hostBuilder);
            using HttpClient client = server.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(new Uri(TestFailEndpoint, UriKind.Relative));
            string responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because the EndpointOutcome should be mapped to HTTP 404 Not Found.");
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/problem+json", "because the response should be in problem+json format for errors, as per RFC 9457.");

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            // Assert standard Problem Details members (RFC 9457)
            root.TryGetProperty(ProblemDetailsConstants.Status, out JsonElement statusProp).Should().BeTrue("because the status code should be mapped as a top-level property.");
            statusProp.GetInt32().Should().Be(ResultStatuses.NotFound.Code, "because the status code should be mapped as a top-level property.");

            root.TryGetProperty(ProblemDetailsConstants.Title, out JsonElement titleProp).Should().BeTrue("because the title should be mapped as a top-level property.");
            titleProp.GetString().Should().Be(ResultStatuses.NotFound.Description, "because the title should be mapped as a top-level property.");

            root.TryGetProperty(ProblemDetailsConstants.Detail, out JsonElement detailProp).Should().BeTrue("because the detail should be mapped as a top-level property.");
            detailProp.GetString().Should().Be(ResNotFoundDescription, "because the detail (ErrorInfo.Message) should be mapped as a top-level property.");

            root.TryGetProperty(ProblemDetailsConstants.Instance, out JsonElement instanceProp).Should().BeTrue("because the instance should be the request path.");
            instanceProp.GetString().Should().Be(TestFailEndpoint, "because the instance should be the request path.");

            // Assert extensions object presence and type
            root.TryGetProperty(ProblemDetailsConstants.Extensions.ErrorCode, out JsonElement errorCodeProp).Should().BeTrue("because the error code should be mapped as a flattened extension.");
            errorCodeProp.GetString().Should().Be(ResNotFound, "because the error code should be mapped as a flattened extension.");

            root.TryGetProperty(ProblemDetailsConstants.Extensions.TraceId, out JsonElement traceIdProp).Should().BeTrue("because the trace identifier should be included as a flattened extension for diagnostics.");
            traceIdProp.GetString().Should().NotBeNullOrEmpty("because the trace identifier should be included as a flattened extension for diagnostics.");

            root.TryGetProperty(CustomExtensionKey, out JsonElement customProp).Should().BeTrue($"because the custom extension '{CustomExtensionKey}' should be present as a flattened property.");
            customProp.GetString().Should().Be(CustomExtensionValue, $"because the custom extension '{CustomExtensionKey}' should have its value as a flattened property.");
        }

        [ApiController]
        [Route("[controller]")]
        internal sealed class TestEndpointController : ControllerBase
        {
            public static IEndpointOutcome? NextEndpointOutcomeForTest { get; set; }

            [HttpGet("test-endpoint")]
            // Change the return type from ActionResult<EndpointOutcome<string>> to IEndpointOutcome
            // This allows the NormalizeEndpointResultFilter to correctly intercept and process the outcome.
            public static IEndpointOutcome GetTestEndpoint()
            {
                if (NextEndpointOutcomeForTest is null)
                {
                    return EndpointOutcome<string>.Success("Hello World");
                }

                // Return the pre-configured outcome directly.
                // Since NextEndpointOutcomeForTest is already IEndpointOutcome,
                // no further casting or fallback creation is needed at this point for the return.
                // The logic within the filter will handle the specific type (e.e.g, IEndpointOutcome<string>)
                // and transform it into an IResult.
                return NextEndpointOutcomeForTest;
            }

            [HttpGet("test-fail-endpoint")]
            public static ActionResult<IEndpointOutcome> GetTestFailEndpoint()
            {
                if (TestEndpointController.NextEndpointOutcomeForTest is null)
                {
                    throw new InvalidOperationException("NextEndpointOutcomeForTest was not set for failure test.");
                }

                return new ActionResult<IEndpointOutcome>(NextEndpointOutcomeForTest);
            }

            public TestEndpointController()
            {
                NextEndpointOutcomeForTest = null;
            }
        }
    }
}
#pragma warning restore CS1591
