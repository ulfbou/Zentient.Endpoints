// <copyright file="EndpointOutcomeToHttpMapperTests.Constructor.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Options;
using Zentient.Endpoints.Tests.Common;
using System.Reflection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Zentient.Endpoints.Http.Tests.Mapping
{
    public partial class EndpointOutcomeToHttpMapperTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProblemDetailsMapperIsNull()
        {
            // Assign to a variable to satisfy CA1806
            EndpointOutcomeToHttpMapper? unused = null;
            Action act = () => unused = new EndpointOutcomeToHttpMapper(
                null!, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(_defaultOptions), _mockEnvironment.Object);
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("problemDetailsMapper");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProblemTypeUriGeneratorIsNull()
        {
            EndpointOutcomeToHttpMapper? unused = null;
            Action act = () => unused = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, null!, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(_defaultOptions), _mockEnvironment.Object);
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("problemTypeUriGenerator");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenSuccessResponseFactoryIsNull()
        {
            EndpointOutcomeToHttpMapper? unused = null;
            Action act = () => unused = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, null!,
                Microsoft.Extensions.Options.Options.Create(_defaultOptions), _mockEnvironment.Object);
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("successResponseFactory");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenOptionsIsNull()
        {
            EndpointOutcomeToHttpMapper? unused = null;
            Action act = () => unused = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                null!, _mockEnvironment.Object);
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("options");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenEnvironmentIsNull()
        {
            EndpointOutcomeToHttpMapper? unused = null;
            Action act = () => unused = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(_defaultOptions), null!);
            act.Should().ThrowExactly<ArgumentNullException>().WithParameterName("environment");
        }

        [Fact]
        public void Constructor_UsesProvidedJsonSerializerOptions()
        {
            // Arrange
            var customOptions = TestOptionsFactory.CreateWithCustomJsonSerializer(jsonOptions =>
            {
                jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
                jsonOptions.WriteIndented = true;
                jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            }).Value;

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(customOptions), _mockEnvironment.Object);

            // Assert
            mapper.JsonSerializerOptions.Should().NotBeNull();
            mapper.JsonSerializerOptions.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.KebabCaseLower);
            mapper.JsonSerializerOptions.WriteIndented.Should().BeTrue();
            mapper.JsonSerializerOptions.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
            mapper.JsonSerializerOptions.Converters.Should().Contain(c => c is JsonStringEnumConverter);
        }

        [Fact]
        public void Constructor_DefaultsJsonSerializerOptions_WhenNoneProvided()
        {
            // Arrange
            var optionsWithoutJsonOptions = new EndpointsHttpOptions { JsonSerializerOptions = null! };

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(optionsWithoutJsonOptions), _mockEnvironment.Object);

            // Assert
            var jsonOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_jsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualJsonOptions = jsonOptionsField?.GetValue(mapper) as JsonSerializerOptions;

            actualJsonOptions.Should().NotBeNull();
            actualJsonOptions!.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
            actualJsonOptions.WriteIndented.Should().BeFalse("because environment is production by default");
            actualJsonOptions.DefaultIgnoreCondition.Should().Be(JsonIgnoreCondition.WhenWritingNull);
            actualJsonOptions.Converters.Should().Contain(c => c is JsonStringEnumConverter);
        }

        [Fact]
        public void Constructor_SetsWriteIndentedTrue_InDevelopmentEnvironment()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Development");
            var optionsWithoutJsonOptions = new EndpointsHttpOptions { JsonSerializerOptions = null! };

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(optionsWithoutJsonOptions), _mockEnvironment.Object);

            // Assert
            var jsonOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_jsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualJsonOptions = jsonOptionsField?.GetValue(mapper) as JsonSerializerOptions;

            actualJsonOptions.Should().NotBeNull();
            actualJsonOptions!.WriteIndented.Should().BeTrue();
        }

        [Fact]
        public void Constructor_AddsJsonStringEnumConverter_IfNotPresent()
        {
            // Arrange
            var customOptions = new EndpointsHttpOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions()
            };

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(customOptions), _mockEnvironment.Object);

            // Assert
            var jsonOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_jsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualJsonOptions = jsonOptionsField?.GetValue(mapper) as JsonSerializerOptions;

            actualJsonOptions.Should().NotBeNull();
            actualJsonOptions!.Converters.Should().ContainSingle(c => c is JsonStringEnumConverter);
        }

        [Fact]
        public void Constructor_DoesNotAddJsonStringEnumConverter_IfAlreadyPresent()
        {
            // Arrange
            var customOptions = new EndpointsHttpOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions()
            };
            customOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(customOptions), _mockEnvironment.Object);

            // Assert
            var jsonOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_jsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualJsonOptions = jsonOptionsField?.GetValue(mapper) as JsonSerializerOptions;

            actualJsonOptions.Should().NotBeNull();
            actualJsonOptions!.Converters.Where(c => c is JsonStringEnumConverter).Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_InitializesOptionsFieldsCorrectly()
        {
            // Arrange
            var customSuccessOptions = new SuccessResponseOptions { DefaultOkStatusCode = 201 };
            var customProblemOptions = new Zentient.Endpoints.Http.Options.ProblemDetailsOptions { DefaultTitle = "Custom Default Problem" };

            var customEndpointsOptions = new EndpointsHttpOptions
            {
                SuccessResponse = customSuccessOptions,
                ProblemDetails = customProblemOptions
            };

            // Act
            var mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object, _mockProblemTypeUriGenerator.Object, _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(customEndpointsOptions), _mockEnvironment.Object);

            // Assert
            var successOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_successResponseOptions", BindingFlags.NonPublic | BindingFlags.Instance);
            var problemOptionsField = typeof(EndpointOutcomeToHttpMapper).GetField("_problemDetailsOptions", BindingFlags.NonPublic | BindingFlags.Instance);

            (successOptionsField?.GetValue(mapper) as SuccessResponseOptions).Should().BeSameAs(customSuccessOptions);
            (problemOptionsField?.GetValue(mapper) as Zentient.Endpoints.Http.Options.ProblemDetailsOptions).Should().BeSameAs(customProblemOptions);
        }
    }
}
#pragma warning restore CS1591
