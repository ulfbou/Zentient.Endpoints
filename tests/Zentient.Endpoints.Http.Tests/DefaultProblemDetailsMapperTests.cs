// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultProblemDetailsMapperTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="DefaultProblemDetailsMapper"/> class.
    /// </summary>
    public sealed class DefaultProblemDetailsMapperTests
    {
        private static readonly Uri DefaultTestProblemTypeBaseUri = new Uri("https://testdomain.com/errors/");
        private readonly Mock<IProblemTypeUriGenerator> _mockProblemTypeUriGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultProblemDetailsMapperTests"/> class.
        /// </summary>
        public DefaultProblemDetailsMapperTests()
        {
            this._mockProblemTypeUriGenerator = new Mock<IProblemTypeUriGenerator>();
            this._mockProblemTypeUriGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.Is<string?>(s => string.IsNullOrEmpty(s))))
                .Returns(new Uri(ProblemDetailsConstants.DefaultBaseUri));
            this._mockProblemTypeUriGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.Is<string>(s => !string.IsNullOrEmpty(s))))
                .Returns((string code) => new Uri(DefaultTestProblemTypeBaseUri, code!.ToUpperInvariant().Replace(' ', '-')));
        }

        /// <summary>
        /// Tests that mapping a null error info returns a 500 Internal Server Error problem details.
        /// </summary>
        [Fact]
        public void Map_NullErrorInfo_ReturnsInternalServerErrorProblemDetails()
        {
            const string TraceId = "trace-123";
            const string ApiResource = "/api/resource";

            // Arrange
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            DefaultHttpContext context = CreateHttpContext(TraceId, ApiResource);

            // Act
            ProblemDetails result = mapper.Map(null, context);

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

        /// <summary>
        /// Tests that a validation error maps to a 400 Bad Request problem details.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_ValidationCategory_MapsToBadRequest()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, "VAL001", "Validation failed.", "Field X is required.");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            DefaultHttpContext context = CreateHttpContext("trace-456", "/api/validate");

            // Act
            ProblemDetails result = mapper.Map(error, context);

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

        /// <summary>
        /// Verifies that each error category maps to the expected status and title.
        /// </summary>
        /// <param name="category">The error category to test.</param>
        /// <param name="expectedStatus">The expected HTTP status code.</param>
        /// <param name="expectedTitle">The expected title for the problem details.</param>
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
        public void Map_ErrorInfo_Category_MapsToExpectedStatusAndTitle(ErrorCategory category, HttpStatusCode expectedStatus, string expectedTitle)
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(category, "ERR", "Error message", "Error detail");
            var mockGenerator = new Mock<IProblemTypeUriGenerator>();
            mockGenerator
                .Setup(g => g.GenerateProblemTypeUri(It.IsAny<string?>()))
                .Returns((string? code) => code == null ? new Uri(ProblemDetailsConstants.DefaultBaseUri) : new Uri($"https://testdomain.com/errors/{code.ToUpperInvariant().Replace(' ', '-')}"));
            DefaultProblemDetailsMapper mapper = new DefaultProblemDetailsMapper(mockGenerator.Object);
            DefaultHttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = mapper.Map(error, context);

            // Assert
            result.Status.Should().Be((int)expectedStatus);
            result.Title.Should().Be(expectedTitle);
            mockGenerator.Verify(g => g.GenerateProblemTypeUri("ERR"), Times.Once);
        }

        /// <summary>
        /// Verifies that extensions and inner errors are mapped to problem details extensions.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_WithExtensionsAndInnerErrors_MapsExtensions()
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
            DefaultHttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = mapper.Map(error, context);

            // Assert
            result.Extensions.Should().ContainKey("customKey1");
            result.Extensions["customKey1"].Should().Be("customValue");
            result.Extensions.Should().ContainKey("customKey2");
            result.Extensions["customKey2"].Should().Be(123);

            result.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.InnerErrors);
            result.Extensions[ProblemDetailsConstants.Extensions.InnerErrors].Should().BeEquivalentTo(innerErrors);
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("CONFLICT"), Times.Once);
        }

        /// <summary>
        /// Verifies that empty extensions and inner errors are not added to problem details extensions.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_EmptyExtensionsAndInnerErrors_DoesNotAddCustomExtensions()
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
            DefaultHttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = mapper.Map(error, context);

            // Assert
            result.Extensions.Should().NotContainKey("customKey1");
            result.Extensions.Should().NotContainKey("innerErrors");
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("NOTFOUND"), Times.Once);
        }

        /// <summary>
        /// Verifies that passing a null HttpContext throws an exception.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_NullHttpContext_Throws()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.InternalServerError, "ERR", "Error");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);

            // Act
            Action act = () => mapper.Map(error, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContext");
        }

        /// <summary>
        /// Verifies that null or empty code or detail are not added as extensions, but detail remains in Detail property.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_EmptyCodeOrDetail_DoesNotAddCodeToExtensions()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, string.Empty, "Validation failed", string.Empty);
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            DefaultHttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = mapper.Map(error, context);

            // Assert
            result.Extensions.Should().NotContainKey("errorCode");
            result.Detail.Should().Be("Validation failed");
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri(string.Empty), Times.Once);
            result.Type.Should().Be(ProblemDetailsConstants.DefaultBaseUri);
        }

        /// <summary>
        /// Verifies that the problem type URI is generated using the injected generator.
        /// </summary>
        [Fact]
        public void Map_ErrorInfo_ProblemTypeUri_UsesGenerator()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, "Invalid Input", "Validation failed");
            DefaultProblemDetailsMapper mapper = CreateMapperWithMockGenerator(this._mockProblemTypeUriGenerator.Object);
            DefaultHttpContext context = CreateHttpContext();

            // Act
            ProblemDetails result = mapper.Map(error, context);

            // Assert
            result.Type.Should().Be($"{DefaultTestProblemTypeBaseUri}INVALID-INPUT");
            this._mockProblemTypeUriGenerator.Verify(g => g.GenerateProblemTypeUri("Invalid Input"), Times.Once);
        }

        /// <summary>
        /// Helper method to create a <see cref="DefaultProblemDetailsMapper"/> with a specific problem type URI generator.
        /// </summary>
        /// <param name="generator">The <see cref="IProblemTypeUriGenerator"/> to use.</param>
        /// <returns>A new <see cref="DefaultProblemDetailsMapper"/> instance.</returns>
        private static DefaultProblemDetailsMapper CreateMapperWithMockGenerator(IProblemTypeUriGenerator generator)
            => new DefaultProblemDetailsMapper(generator);

        /// <summary>
        /// Helper method to create a <see cref="DefaultProblemDetailsMapper"/> with the default problem type URI generator.
        /// </summary>
        /// <returns>A new <see cref="DefaultProblemDetailsMapper"/> instance.</returns>
        private static DefaultProblemDetailsMapper CreateMapper()
            => new DefaultProblemDetailsMapper();

        /// <summary>
        /// Helper method to create a <see cref="DefaultHttpContext"/> for tests.
        /// </summary>
        /// <param name="traceId">Optional trace identifier.</param>
        /// <param name="path">Optional request path.</param>
        /// <returns>A configured <see cref="DefaultHttpContext"/>.</returns>
        private static DefaultHttpContext CreateHttpContext(string? traceId = null, string? path = "/test")
        {
            DefaultHttpContext context = new DefaultHttpContext();

            if (traceId != null)
            {
                context.TraceIdentifier = traceId;
            }

            if (path != null)
            {
                context.Request.Path = path;
            }

            return context;
        }
    }
}
