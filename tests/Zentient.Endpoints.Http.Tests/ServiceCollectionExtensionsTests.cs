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
using System.Threading.Tasks;

using Xunit;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using FluentAssertions;

using Moq;

using Zentient.Endpoints;
using Zentient.Results;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Models;
using System.Net.Http;
using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Options;
using Zentient.Endpoints.Http.Validation;
using Zentient.Endpoints.Http.Filters;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // --- AddZentientEndpointsHttp Tests ---

        [Fact]
        public void AddZentientEndpointsHttp_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null!;

            // Act
            Action act = () => services.AddZentientEndpointsHttp();

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>()
                .WithParameterName("services");
        }

        [Fact]
        public void AddZentientEndpointsHttp_RegistersCoreServicesAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging();

            // Act
            services.AddZentientEndpointsHttp();

            // Assert
            services.Should().ContainSingle(s => s.ServiceType == typeof(IEndpointOutcomeToHttpMapper) && s.Lifetime == ServiceLifetime.Scoped);
            services.Should().ContainSingle(s => s.ServiceType == typeof(IProblemDetailsMapper) && s.Lifetime == ServiceLifetime.Scoped);
            services.Should().ContainSingle(s => s.ServiceType == typeof(IProblemTypeUriGenerator) && s.Lifetime == ServiceLifetime.Scoped);
            services.Should().ContainSingle(s => s.ServiceType == typeof(ISuccessResponseFactory) && s.Lifetime == ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddZentientEndpointsHttp_AddsEndpointsHttpOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging();

            // Act
            services.AddZentientEndpointsHttp();

            // Assert
            // This asserts that an IConfigureOptions for EndpointsHttpOptions exists, meaning options are set up.
            services.Should().ContainSingle(s => s.ServiceType == typeof(IConfigureOptions<EndpointsHttpOptions>));
        }

        [Fact]
        public void AddZentientEndpointsHttp_ConfiguresEndpointsHttpOptions_ViaConfigureOptionsAction()
        {
            // Arrange
            var services = new ServiceCollection();
            // FIX: Remove boolean flag, rely on direct option assertion
            // bool configureOptionsCalled = false;
            string expectedTitle = "Custom Problem Title";
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging(); // ADDED: Required for ILogger dependencies

            // Act
            services.AddZentientEndpointsHttp(options =>
            {
                options.ProblemDetails.DefaultTitle = expectedTitle;
                // configureOptionsCalled = true; // Removed this line
            });

            // Assert
            // configureOptionsCalled.Should().BeTrue(); // Removed this line

            // Build service provider and retrieve options to verify
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<EndpointsHttpOptions>>().Value;

            options.ProblemDetails.DefaultTitle.Should().Be(expectedTitle);
        }

        [Fact]
        public void AddZentientEndpointsHttp_RegistersEndpointsHttpOptionsValidatorAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging(); // ADDED: Required for ILogger dependencies

            // Act
            services.AddZentientEndpointsHttp();

            // Assert
            services.Should().ContainSingle(s => s.ServiceType == typeof(IValidateOptions<EndpointsHttpOptions>) && s.ImplementationType == typeof(EndpointsHttpOptionsValidator) && s.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddZentientEndpointsHttp_ConfiguresJsonSerializerOptions_ViaConfigureJsonOptionsAction()
        {
            // Arrange
            var services = new ServiceCollection();
            var expectedPropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging(); // ADDED: Required for ILogger dependencies

            // Act
            services.AddZentientEndpointsHttp(
                configureJsonOptions: jsonOptions =>
                {
                    jsonOptions.PropertyNamingPolicy = expectedPropertyNamingPolicy;
                });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<EndpointsHttpOptions>>().Value;

            options.JsonSerializerOptions.PropertyNamingPolicy.Should().Be(expectedPropertyNamingPolicy);
        }

        [Theory]
        [InlineData("http://example.com/api/errors", "http://example.com/api/errors/")]
        [InlineData("http://example.com/api/errors/", "http://example.com/api/errors/")]
        [InlineData(null, null)] // Test null case
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "InlineData requires string literals; conversion to Uri is handled internally.")]
        public void AddZentientEndpointsHttp_PostConfiguresBaseTypeUri_ToAlwaysEndWithSlash(string? initialUriString, string? expectedUriString)
        {
            // Arrange
            var services = new ServiceCollection();
            var initialUri = initialUriString != null ? new Uri(initialUriString) : null;
            var expectedUri = expectedUriString != null ? new Uri(expectedUriString) : null;
            // FIX: Add logging to satisfy ILogger dependencies for services like DefaultProblemDetailsMapper
            services.AddLogging(); // ADDED: Required for ILogger dependencies


            // Act
            services.AddZentientEndpointsHttp(options =>
            {
                options.ProblemDetails.BaseTypeUri = initialUri;
            });

            // Build service provider and retrieve options to verify
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<EndpointsHttpOptions>>().Value;

            // Assert
            options.ProblemDetails.BaseTypeUri.Should().Be(expectedUri);
        }

        [Fact]
        public void AddZentientEndpointsHttp_DoesNotAddNormalizeEndpointOutcomeFilterGlobally_ByDefault()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // Already added for ILogger dependency
            // FIX: Add a mock for IWebHostEnvironment, as DefaultProblemDetailsMapper now depends on it.
            services.AddSingleton<IWebHostEnvironment>(new Mock<IWebHostEnvironment>().Object);


            // Act
            services.AddZentientEndpointsHttp(); // No configureOptions, so AddNormalizeEndpointOutcomeFilterGlobally should be false

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var filters = serviceProvider.GetServices<IEndpointFilter>();

            // Assert that NormalizeEndpointOutcomeFilter is NOT among the registered IEndpointFilters
            filters.Where(f => f is not null).Should().NotContain(f => f.GetType() == typeof(NormalizeEndpointOutcomeFilter));

            // Also verify that the option itself is false by default
            var options = serviceProvider.GetRequiredService<IOptions<EndpointsHttpOptions>>().Value;
            options.AddNormalizeEndpointOutcomeFilterGlobally.Should().BeFalse();
        }

        [Fact]
        public void AddZentientEndpointsHttp_AddsNormalizeEndpointOutcomeFilterGlobally_WhenConfigured()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // Already added for ILogger dependency
            // FIX: Add a mock for IWebHostEnvironment, as DefaultProblemDetailsMapper now depends on it.
            services.AddSingleton<IWebHostEnvironment>(new Mock<IWebHostEnvironment>().Object);

            // Act
            services.AddZentientEndpointsHttp(options =>
            {
                options.AddNormalizeEndpointOutcomeFilterGlobally = true;
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var filters = serviceProvider.GetServices<IEndpointFilter>();

            // Assert that NormalizeEndpointOutcomeFilter IS among the registered IEndpointFilters
            filters.Should().ContainSingle(f => f.GetType() == typeof(NormalizeEndpointOutcomeFilter));

            // Also verify that the option itself is true
            var options = serviceProvider.GetRequiredService<IOptions<EndpointsHttpOptions>>().Value;
            options.AddNormalizeEndpointOutcomeFilterGlobally.Should().BeTrue();
        }

        // --- WithNormalizeEndpointOutcomeFilter Tests ---

        [Fact]
        public void WithNormalizeEndpointOutcomeFilter_ThrowsArgumentNullException_WhenBuilderIsNull()
        {
            // Arrange
            RouteHandlerBuilder builder = null!;

            // Act
            Action act = () => builder.WithNormalizeEndpointOutcomeFilter();

            // Assert
            act.Should().ThrowExactly<ArgumentNullException>()
                .WithParameterName("builder");
        }

        // This test remains commented out as RouteHandlerBuilder is a sealed class and cannot be mocked directly by Moq.
        // It requires an integration test setup or a different mocking framework/approach.
        /*
        [Fact]
        public void WithNormalizeEndpointOutcomeFilter_AddsEndpointFilter()
        {
            // Arrange
            var mockBuilder = new Mock<RouteHandlerBuilder>();

            mockBuilder.Setup(b => b.AddEndpointFilter<NormalizeEndpointOutcomeFilter>())
                .Returns(mockBuilder.Object)
                .Verifiable();

            // Act
            var resultBuilder = mockBuilder.Object.WithNormalizeEndpointOutcomeFilter();

            // Assert
            resultBuilder.Should().BeSameAs(mockBuilder.Object, "because the method should return the same builder instance for chaining");

            mockBuilder.Verify(b => b.AddEndpointFilter<NormalizeEndpointOutcomeFilter>(), Times.Once());
        }
        */
    }
}
#pragma warning restore CS1591
