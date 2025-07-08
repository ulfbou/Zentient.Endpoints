// <copyright file="EndpointOutcomeExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Xunit;

using Zentient.Endpoints.Http;
using Zentient.Endpoints.Tests.Common;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public class NoContentResponseTests
    {
        [Theory]
        [InlineData(StatusCodes.Status204NoContent, null, "application/json")]
        [InlineData(StatusCodes.Status200OK, null, "application/json")]
        [InlineData(StatusCodes.Status204NoContent, "text/plain", "text/plain")]
        [InlineData(StatusCodes.Status400BadRequest, "application/xml", "application/xml")]
        public async Task NoContentResponse_ExecuteAsync_SetsCorrectStatusCodeAndContentType(
            int statusCode,
            string? contentType,
            string expectedContentType)
        {
            // Arrange
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();
            var noContentResponse = new NoContentResponse(statusCode, contentType);

            // Act
            await noContentResponse.ExecuteAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(statusCode, "because the status code should be set correctly");
            httpContext.Response.ContentType.Should().Be(expectedContentType, "because the content type should be set correctly");
            responseStream.Length.Should().Be(0, "because no content should be written to the response body");
        }

        [Fact]
        public async Task NoContentResponse_ExecuteAsync_ThrowsArgumentNullException_WhenHttpContextIsNull()
        {
            // Arrange
            var noContentResponse = new NoContentResponse(StatusCodes.Status204NoContent);

            // Act
            Func<Task> act = async () => await noContentResponse.ExecuteAsync(null!);

            // Assert
            await act.Should().ThrowExactlyAsync<ArgumentNullException>()
                .WithParameterName("httpContext", "because httpContext cannot be null");
        }

        [Fact]
        public void NoContentResponse_StatusCodeProperty_ReturnsCorrectValue()
        {
            // Arrange
            var statusCode = StatusCodes.Status200OK;
            var noContentResponse = new NoContentResponse(statusCode);

            // Act
            var actualStatusCode = noContentResponse.StatusCode;

            // Assert
            actualStatusCode.Should().Be(statusCode, "because the StatusCode property should reflect the constructor argument");
        }

        [Fact]
        public async Task NoContentResponse_No204Content_SetsStatusCodeTo204AndApplicationJson()
        {
            // Arrange
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();
            var noContentResponse = NoContentResponse.No204Content;

            // Act
            await noContentResponse.ExecuteAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent, "because No204Content static property should set status code to 204");
            httpContext.Response.ContentType.Should().Be("application/json", "because No204Content should default content type to application/json");
            responseStream.Length.Should().Be(0, "because no content should be written for a 204 response");
        }

        [Fact]
        public async Task NoContentResponse_OkNoContent_SetsStatusCodeTo200AndApplicationJson()
        {
            // Arrange
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();
            var noContentResponse = NoContentResponse.OkNoContent;

            // Act
            await noContentResponse.ExecuteAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK, "because OkNoContent static property should set status code to 200");
            httpContext.Response.ContentType.Should().Be("application/json", "because OkNoContent should default content type to application/json");
            responseStream.Length.Should().Be(0, "because no content should be written for a 200 no content response");
        }
    }
}
#pragma warning restore CS1591
