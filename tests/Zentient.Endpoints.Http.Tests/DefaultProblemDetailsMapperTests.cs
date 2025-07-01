// <copyright file="DefaultProblemDetailsMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

using Zentient.Endpoints.Http;
using Zentient.Endpoints.Http.Mapping;
using Zentient.Results;
using Zentient.Results.Constants;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public sealed class DefaultProblemDetailsMapperTests
    {
        private static readonly Uri DefaultTestProblemTypeBaseUri = new Uri("https://testdomain.com/errors/");
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IProblemTypeUriGenerator> _mockProblemTypeUriGenerator;

        public DefaultProblemDetailsMapperTests()
        {
            this._mockHttpContext = new Mock<HttpContext>();
            this._mockProblemTypeUriGenerator = new Mock<IProblemTypeUriGenerator>();
            this._mockProblemTypeUriGenerator
                .Setup(g => g.Generate(It.Is<string?>(s => string.IsNullOrEmpty(s)), It.IsAny<HttpContext>()))
                .Returns((string? code, HttpContext ctx) => new ValueTask<string>(ProblemDetailsConstants.DefaultBaseUri));
            this._mockProblemTypeUriGenerator
                .Setup(g => g.Generate(It.Is<string>(s => !string.IsNullOrEmpty(s)), It.IsAny<HttpContext>()))
                .Returns((string code, HttpContext ctx) => new ValueTask<string>(new Uri(DefaultTestProblemTypeBaseUri, code!.ToUpperInvariant().Replace(' ', '-')).ToString()));
            this._mockProblemTypeUriGenerator
                .Setup(g => g.Generate(null, It.IsAny<HttpContext>()))
                .Returns((string? code, HttpContext ctx) => new ValueTask<string>(ProblemDetailsConstants.DefaultBaseUri));
        }

        [Fact]
        public async Task Map_NullErrorInfo_ReturnsInternalServerErrorProblemDetailsAsync()
        {
            const string TraceId = "trace-123";
            const string ApiResource = "/api/resource";

            // Arrange
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext(TraceId, ApiResource);

            // Act
            ProblemDetails result = await mapper.Map(null, context);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Title.Should().Be(ResultStatuses.Error.Description);
            result.Detail.Should().Be("An unexpected error occurred and no specific error information was provided.");
            result.Instance.Should().Be(ApiResource);
            result.Extensions.Should().NotContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            result.Extensions.Should().NotContainKey(ProblemDetailsConstants.Extensions.Detail);
            result.Extensions[ProblemDetailsConstants.Extensions.TraceId].Should().Be(TraceId);
            result.Type.Should().Be($"{DefaultTestProblemTypeBaseUri}{ErrorCodes.InternalServerError.ToUpperInvariant().Replace(' ', '-')}");
            this._mockProblemTypeUriGenerator.Verify(g => g.Generate(ErrorCodes.InternalServerError, It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Map_ErrorInfo_ValidationCategory_MapsToBadRequestAsync()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, "VAL001", "Validation failed.", "Field X is required.");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext("trace-456", "/api/validate");

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Status.Should().Be((int)HttpStatusCode.BadRequest);
            result.Title.Should().Be("Bad Request");
            result.Detail.Should().Be("Validation failed.");
            result.Instance.Should().Be("/api/validate");
            result.Extensions[ProblemDetailsConstants.Extensions.ErrorCode].Should().Be("VAL001");
            result.Extensions[ProblemDetailsConstants.Extensions.Detail].Should().Be("Field X is required.");
            result.Extensions[ProblemDetailsConstants.Extensions.TraceId].Should().Be("trace-456");
            result.Type.Should().Be($"{DefaultTestProblemTypeBaseUri}VAL001");
            this._mockProblemTypeUriGenerator.Verify(g => g.Generate("VAL001", It.IsAny<HttpContext>()), Times.Once);
        }

        [Theory]
        [InlineData(ErrorCategory.NotFound, HttpStatusCode.NotFound, ResultStatusConstants.Description.NotFound)]
        [InlineData(ErrorCategory.Conflict, HttpStatusCode.Conflict, "Conflict")]
        [InlineData(ErrorCategory.Authentication, HttpStatusCode.Unauthorized, "Unauthorized")]
        [InlineData(ErrorCategory.Authorization, HttpStatusCode.Forbidden, "Forbidden")]
        [InlineData(ErrorCategory.InternalServerError, HttpStatusCode.InternalServerError, "Internal Server Error")]
        [InlineData(ErrorCategory.Timeout, HttpStatusCode.RequestTimeout, "Request Timeout")]
        [InlineData(ErrorCategory.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable")]
        [InlineData(ErrorCategory.TooManyRequests, HttpStatusCode.TooManyRequests, "Too Many Requests")]
        [InlineData(ErrorCategory.Concurrency, HttpStatusCode.Conflict, "Conflict")]
        [InlineData(ErrorCategory.ProblemDetails, HttpStatusCode.BadRequest, "Bad Request")]
        [InlineData(ErrorCategory.ResourceGone, HttpStatusCode.Gone, "Gone")]
        [InlineData(ErrorCategory.NotImplemented, HttpStatusCode.NotImplemented, "Not Implemented")]
        [InlineData(ErrorCategory.RateLimit, HttpStatusCode.TooManyRequests, "Too Many Requests")]
        public async Task Map_ErrorInfo_Category_MapsToExpectedStatusAndTitleAsync(
            ErrorCategory category,
            HttpStatusCode expectedStatus,
            string expectedTitle)
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(category, "ERR", "Error message", "Error detail");
            var mockGenerator = new Mock<IProblemTypeUriGenerator>();
            mockGenerator
                .Setup(g => g.Generate(It.IsAny<string?>(), It.IsAny<HttpContext>()))
                .Returns((string? code, HttpContext ctx) => new ValueTask<string>(code == null ? ProblemDetailsConstants.DefaultBaseUri : $"https://testdomain.com/errors/{code.ToUpperInvariant().Replace(' ', '-')}"));

            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DefaultProblemDetailsMapper>>();
            var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            mockEnv.SetupGet(e => e.EnvironmentName).Returns("Development");
            Microsoft.Extensions.Options.IOptions<Options.EndpointsHttpOptions> options = Microsoft.Extensions.Options.Options.Create(new Options.EndpointsHttpOptions());

            DefaultProblemDetailsMapper mapper = new DefaultProblemDetailsMapper(
                mockGenerator.Object,
                mockLogger.Object,
                mockEnv.Object,
                options
            );
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Status.Should().Be((int)expectedStatus);
            result.Title.Should().Be(expectedTitle);
            mockGenerator.Verify(g => g.Generate("ERR", It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Map_ErrorInfo_WithExtensionsAndInnerErrors_MapsExtensionsAsync()
        {
            // Arrange
            Dictionary<string, object?> customExtensions = new Dictionary<string, object?>
            {
                { "customKey1", "customValue" },
                { "customKey2", 123 },
            };
            List<ErrorInfo> innerErrors = new List<ErrorInfo>
            {
                new ErrorInfo(ErrorCategory.Validation, "INNER1", "Inner error 1"),
                new ErrorInfo(ErrorCategory.Conflict, "INNER2", "Inner error 2"),
            };
            ErrorInfo error = new ErrorInfo(
                ErrorCategory.Conflict,
                "CONFLICT",
                "Conflict occurred",
                "Details",
                metadata: customExtensions.ToImmutableDictionary(),
                innerErrors: innerErrors.ToImmutableList());

            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Extensions.Should().ContainKey("customKey1");
            result.Extensions["customKey1"].Should().Be("customValue");
            result.Extensions.Should().ContainKey("customKey2");
            result.Extensions["customKey2"].Should().Be(123);

            result.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.InnerErrors);

            var expectedInnerErrorsInExtensions = innerErrors
                .Select(inner => new Dictionary<string, object?>
                {
                    { "category", inner.Category.ToString().ToUpperInvariant() },
                    { "code", inner.Code },
                    { "message", inner.Message },
                    { "detail", inner.Detail },
                })
                .ToList();

            result.Extensions[ProblemDetailsConstants.Extensions.InnerErrors]
                .Should()
                .BeEquivalentTo(expectedInnerErrorsInExtensions, options => options.WithoutStrictOrdering());

            this._mockProblemTypeUriGenerator.Verify(g => g.Generate("CONFLICT", It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Map_ErrorInfo_EmptyExtensionsAndInnerErrors_DoesNotAddCustomExtensionsAsync()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(
                category: ErrorCategory.NotFound,
                code: "NOTFOUND",
                message: "Not found",
                detail: "No details",
                metadata: ImmutableDictionary<string, object?>.Empty, // <-- Changed 'extensions' to 'metadata' and use ImmutableDictionary.Empty
                innerErrors: ImmutableList<ErrorInfo>.Empty);      // <-- Use ImmutableList.Empty

            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Extensions.Should().NotContainKey("customKey1");
            // The InnerErrors extension should still be present if your mapper always adds it,
            // even if the list is empty. If it only adds when not empty, then NotContainKey is fine.
            // Based on typical ProblemDetails implementations, empty collections are usually omitted or empty.
            // Double-check your mapper's behavior regarding empty inner errors.
            result.Extensions.Should().NotContainKey("innerErrors");
            this._mockProblemTypeUriGenerator.Verify(g => g.Generate("NOTFOUND", It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public void Map_ErrorInfo_NullHttpContext_Throws()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.InternalServerError, "ERR", "Error");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);

            // Act
            Func<Task> act = () => mapper.Map(error, null!);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task Map_ErrorInfo_EmptyCodeOrDetail_DoesNotAddCodeToExtensionsAsync()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, string.Empty, "Validation failed", string.Empty);
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Extensions.Should().NotContainKey("errorCode");
            result.Detail.Should().Be("Validation failed");
            this._mockProblemTypeUriGenerator.Verify(g => g.Generate(string.Empty, It.IsAny<HttpContext>()), Times.Once);
            result.Type.Should().Be(ProblemDetailsConstants.DefaultBaseUri);
        }

        [Fact]
        public async Task Map_ErrorInfo_ProblemTypeUri_UsesGeneratorAsync()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, "Invalid Input", "Validation failed");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Type.Should().Be($"{DefaultTestProblemTypeBaseUri}INVALID-INPUT");
            this._mockProblemTypeUriGenerator.Verify(g => g.Generate("Invalid Input", It.IsAny<HttpContext>()), Times.Once);
        }

        private static DefaultProblemDetailsMapper CreateMapperWithMockGenerator(IProblemTypeUriGenerator generator)
        {
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DefaultProblemDetailsMapper>>();
            var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            mockEnv.SetupGet(e => e.EnvironmentName).Returns("Development");
            Microsoft.Extensions.Options.IOptions<Options.EndpointsHttpOptions> options = Microsoft.Extensions.Options.Options.Create(new Options.EndpointsHttpOptions());
            return new DefaultProblemDetailsMapper(generator, mockLogger.Object, mockEnv.Object, options);
        }

        private static DefaultProblemDetailsMapper CreateMapper()
        {
            var mockGenerator = new Mock<IProblemTypeUriGenerator>();
            mockGenerator
                .Setup(g => g.Generate(It.IsAny<string?>(), It.IsAny<HttpContext>()))
                .Returns((string? code, HttpContext ctx) => new ValueTask<string>(ProblemDetailsConstants.DefaultBaseUri));
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<DefaultProblemDetailsMapper>>();
            var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            mockEnv.SetupGet(e => e.EnvironmentName).Returns("Development");
            Microsoft.Extensions.Options.IOptions<Zentient.Endpoints.Http.Options.EndpointsHttpOptions> options = Microsoft.Extensions.Options.Options.Create(new Zentient.Endpoints.Http.Options.EndpointsHttpOptions());
            return new DefaultProblemDetailsMapper(mockGenerator.Object, mockLogger.Object, mockEnv.Object, options);
        }

        private static HttpContext CreateHttpContext(string? traceId = null, string? path = "/test")
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockHttpRequest = new Mock<HttpRequest>();

            mockHttpContext.SetupGet(c => c.Request).Returns(mockHttpRequest.Object);
            mockHttpRequest.SetupGet(r => r.Path).Returns(new PathString(path));
            mockHttpContext.SetupProperty(c => c.TraceIdentifier, traceId ?? Guid.NewGuid().ToString());

            return mockHttpContext.Object;
        }
    }
}
#pragma warning restore CS1591
