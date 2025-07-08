// <copyright file="EndpointOutcomeToHttpMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using FluentAssertions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Constants;
using Zentient.Endpoints.Http.Constants;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Options;
using Zentient.Results;
using Zentient.Results.Constants;

using Zentient.Endpoints.Tests.Common;


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Zentient.Endpoints.Http.Tests.Mapping
{
    public partial class EndpointOutcomeToHttpMapperTests
    {
        private readonly Mock<IProblemDetailsMapper> _mockProblemDetailsMapper;
        private readonly Mock<IProblemTypeUriGenerator> _mockProblemTypeUriGenerator;
        private readonly Mock<ISuccessResponseFactory> _mockSuccessResponseFactory;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly EndpointsHttpOptions _defaultOptions;
        private readonly EndpointOutcomeToHttpMapper _mapper;

        public EndpointOutcomeToHttpMapperTests()
        {
            _mockProblemDetailsMapper = new Mock<IProblemDetailsMapper>();
            _mockProblemTypeUriGenerator = new Mock<IProblemTypeUriGenerator>();
            _mockSuccessResponseFactory = new Mock<ISuccessResponseFactory>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            _defaultOptions = TestOptionsFactory.CreateDefault().Value;
            _defaultOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            _defaultOptions.ProblemDetails.IncludeStackTrace = false;
            _defaultOptions.ProblemDetails.IncludeErrorCodeInExtensions = true;

            if (!_defaultOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                _defaultOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }

            _mapper = new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object,
                _mockProblemTypeUriGenerator.Object,
                _mockSuccessResponseFactory.Object,
            Microsoft.Extensions.Options.Options.Create(_defaultOptions),
                _mockEnvironment.Object);
        }

        [Fact]
        public async Task Map_ThrowsArgumentNullException_WhenOutcomeIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();

            // Act
            Func<Task> act = async () => await _mapper.Map(null!, httpContext);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("outcome");
        }

        [Fact]
        public async Task Map_ThrowsArgumentNullException_WhenHttpContextIsNull()
        {
            // Arrange
            var outcome = EndpointOutcome.Success();

            // Act
            Func<Task> act = async () => await _mapper.Map(outcome, null!);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task Map_ReturnsHeaderWrappedResult_WhenHeadersArePresentAndNoLocation()
        {
            // Arrange
            var headers = ImmutableDictionary.Create<string, string>().Add("X-Custom-Header", "Value1");
            var metadata = new TransportMetadata().WithTag(HttpMetadataKeys.Headers, headers);
            var outcome = EndpointOutcome.Success(transportMetadata: metadata);


            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext(_mapper.JsonSerializerOptions);

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Message = "Success" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<HeaderWrappedResult>();
            var headerWrappedResult = result as HeaderWrappedResult;
            headerWrappedResult.Should().NotBeNull();
            headerWrappedResult!.Result.Should().BeOfType<JsonHttpResult<object>>();
            var innerJsonResult = headerWrappedResult.Result as JsonHttpResult<object>;
            innerJsonResult.Should().NotBeNull();

            await headerWrappedResult.ExecuteAsync(httpContext);

            httpContext.Response.Headers.Should().ContainKey("X-Custom-Header").WhoseValue.Should().Contain("Value1");
            httpContext.Response.Headers.ContainsKey("Location").Should().BeFalse();
        }

        [Fact]
        public async Task Map_ReturnsHeaderWrappedResult_WhenLocationIsPresentAndNoHeaders()
        {
            // Arrange
            var locationUri = new Uri("/api/resource/123", UriKind.Relative);
            var metadata = new TransportMetadata().WithTag(HttpMetadataKeys.LocationUri, locationUri);
            var outcome = EndpointOutcome.Success(transportMetadata: metadata);
            var httpContext = new DefaultHttpContext();

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Message = "Success" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<HeaderWrappedResult>();
            var headerWrappedResult = result as HeaderWrappedResult;
            headerWrappedResult.Should().NotBeNull();
            headerWrappedResult!.Headers.Should().BeEmpty();
            headerWrappedResult.Location.Should().Be(locationUri);
        }

        [Fact]
        public async Task Map_ReturnsHeaderWrappedResult_WhenBothHeadersAndLocationArePresent()
        {
            // Arrange
            var locationUri = new Uri("/api/resource/123", UriKind.Relative);
            var headers = ImmutableDictionary.Create<string, string>().Add("X-Custom-Header", "Value1");
            var metadata = new TransportMetadata()
                .WithTag(HttpMetadataKeys.Headers, headers)
                .WithTag(HttpMetadataKeys.LocationUri, locationUri);
            var outcome = EndpointOutcome.Success(transportMetadata: metadata);
            var httpContext = new DefaultHttpContext();

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Message = "Success" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<HeaderWrappedResult>();
            var headerWrappedResult = result as HeaderWrappedResult;
            headerWrappedResult.Should().NotBeNull();
            headerWrappedResult!.Headers.Should().ContainKey("X-Custom-Header").WhoseValue.Should().Be("Value1");
            headerWrappedResult.Location.Should().Be(locationUri);
        }

        [Fact]
        public async Task Map_ReturnsInnerResult_WhenNoHeadersOrLocationInMetadata()
        {
            // Arrange
            var outcome = EndpointOutcome.Success(transportMetadata: new TransportMetadata());
            var httpContext = new DefaultHttpContext();

            _mockSuccessResponseFactory.Setup(f => f.CreateSuccessResponse(
                It.IsAny<IEndpointOutcome>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>()))
                .Returns(new { Message = "Success" });

            // Act
            var result = await _mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<JsonHttpResult<object>>();
        }

        private EndpointOutcomeToHttpMapper CreateMapper(EndpointsHttpOptions options, IWebHostEnvironment environment)
        {
            return new EndpointOutcomeToHttpMapper(
                _mockProblemDetailsMapper.Object,
                _mockProblemTypeUriGenerator.Object,
                _mockSuccessResponseFactory.Object,
                Microsoft.Extensions.Options.Options.Create(options),
                environment);
        }

        private static TResult UnwrapResult<TResult>(Microsoft.AspNetCore.Http.IResult result) where TResult : Microsoft.AspNetCore.Http.IResult
        {
            if (result is HeaderWrappedResult headerWrappedResult)
            {
                return (TResult)headerWrappedResult.Result;
            }
            return (TResult)result;
        }
    }
}
#pragma warning restore CS1591
