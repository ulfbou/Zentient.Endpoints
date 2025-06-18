// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultProblemDetailsMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Xunit;

using Zentient.Endpoints.Http;
using Zentient.Results;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public sealed class DefaultProblemDetailsMapperTests
    {
        private static readonly Uri DefaultTestProblemTypeBaseUri = new Uri("https://testdomain.com/errors/");
        private readonly Mock<IProblemTypeUriGenerator> _mockProblemTypeUriGenerator;

        public DefaultProblemDetailsMapperTests()
        {
            this._mockProblemTypeUriGenerator = new Mock<IProblemTypeUriGenerator>();
            this._mockProblemTypeUriGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.Is<string?>(s => string.IsNullOrEmpty(s))))
                .Returns(new Uri(ProblemDetailsConstants.DefaultBaseUri));
            this._mockProblemTypeUriGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.Is<string>(s => !string.IsNullOrEmpty(s))))
                .Returns((string code) => new Uri(DefaultTestProblemTypeBaseUri, code!.ToUpperInvariant().Replace(' ', '-')));
            this._mockProblemTypeUriGenerator
                .Setup(g => g.GenerateProblemTypeUri(null))
                .Returns(new Uri(ProblemDetailsConstants.DefaultBaseUri));
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
            result.Detail.Should().Be("No error information was provided.");
            result.Instance.Should().Be("/api/resource");
            result.Extensions.Should().NotContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            result.Extensions.Should().NotContainKey(ProblemDetailsConstants.Extensions.Detail);
            result.Extensions[ProblemDetailsConstants.Extensions.TraceId].Should().Be(TraceId);
            result.Type.Should().Be(ProblemDetailsConstants.DefaultBaseUri);
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri(null), Times.Once);
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
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("VAL001"), Times.Once);
        }

        [Theory]
        [InlineData(ErrorCategory.NotFound, HttpStatusCode.NotFound, "Not Found")]
        [InlineData(ErrorCategory.Conflict, HttpStatusCode.Conflict, "Conflict")]
        [InlineData(ErrorCategory.Unauthorized, HttpStatusCode.Unauthorized, "Unauthorized")]
        [InlineData(ErrorCategory.Forbidden, HttpStatusCode.Forbidden, "Forbidden")]
        [InlineData(ErrorCategory.InternalServerError, HttpStatusCode.InternalServerError, "Error")]
        [InlineData(ErrorCategory.Timeout, HttpStatusCode.RequestTimeout, "Request Timeout")]
        [InlineData(ErrorCategory.ServiceUnavailable, HttpStatusCode.ServiceUnavailable, "Service Unavailable")]
        [InlineData(ErrorCategory.TooManyRequests, HttpStatusCode.TooManyRequests, "Too Many Requests")]
        [InlineData(ErrorCategory.Concurrency, HttpStatusCode.Conflict, "Conflict")]
        [InlineData(ErrorCategory.ProblemDetails, HttpStatusCode.BadRequest, "Bad Request")]
        public async Task Map_ErrorInfo_Category_MapsToExpectedStatusAndTitleAsync(ErrorCategory category, HttpStatusCode expectedStatus, string expectedTitle)
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(category, "ERR", "Error message", "Error detail");
            var mockGenerator = new Mock<IProblemTypeUriGenerator>();
            mockGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.IsAny<string?>()))
                .Returns((string? code) => code == null ? new Uri(ProblemDetailsConstants.DefaultBaseUri) : new Uri($"https://testdomain.com/errors/{code.ToUpperInvariant().Replace(' ', '-')}"));
            DefaultProblemDetailsMapper mapper = new DefaultProblemDetailsMapper(mockGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Status.Should().Be((int)expectedStatus);
            result.Title.Should().Be(expectedTitle);
            mockGenerator.Verify(g => g.GenerateProblemTypeUri("ERR"), Times.Once);
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
                innerErrors: innerErrors,
                extensions: customExtensions);

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
            result.Extensions[ProblemDetailsConstants.Extensions.InnerErrors].Should().BeEquivalentTo(innerErrors);
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("CONFLICT"), Times.Once);
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
                extensions: new Dictionary<string, object?>(),
                innerErrors: new List<ErrorInfo>());

            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            HttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = await mapper.Map(error, context);

            // Assert
            result.Extensions.Should().NotContainKey("customKey1");
            result.Extensions.Should().NotContainKey("innerErrors");
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("NOTFOUND"), Times.Once);
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
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri(string.Empty), Times.Once);
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
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("Invalid Input"), Times.Once);
        }

        private static DefaultProblemDetailsMapper CreateMapperWithMockGenerator(IProblemTypeUriGenerator generator)
            => new DefaultProblemDetailsMapper(generator);

        private static DefaultProblemDetailsMapper CreateMapper()
            => new DefaultProblemDetailsMapper();

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
