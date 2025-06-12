// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NormalizeEndpointResultFilterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using Moq;

using Xunit;

using Zentient.Endpoints.Core;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>
    /// Contains sophisticated and exhaustive unit tests for the <see cref="NormalizeEndpointResultFilter"/> class.
    /// These tests ensure the filter correctly intercepts and maps EndpointResult instances
    /// to <see cref="Microsoft.AspNetCore.Http.IResult"/>, and passes through other result types without interference.
    /// </summary>
    public sealed class NormalizeEndpointResultFilterTests
    {
        private readonly Mock<IEndpointResultToHttpResultMapper> _mockMapper;
        private readonly NormalizeEndpointResultFilter _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeEndpointResultFilterTests"/> class.
        /// </summary>
        public NormalizeEndpointResultFilterTests()
        {
            this._mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            this._filter = new NormalizeEndpointResultFilter(this._mockMapper.Object);
        }

        /// <summary>
        /// Tests that the filter correctly intercepts an <see cref="EndpointResult{TResult}"/>
        /// from the next delegate and uses the mapper to convert it to an <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsEndpointResult_MapsAndReturnsIResult()
        {
            // Arrange
            string testValue = "Success!";
            EndpointResult<string> endpointResult = EndpointResult<string>.From(testValue); // Concrete type
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok(testValue);
            EndpointFilterInvocationContext context = CreateMockContext();

            // Setup the next delegate to return our EndpointResult (as object?)
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(endpointResult);

            // Setup the mapper to return a specific IResult when called
            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()))
                .Returns(expectedIResult);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            this._mockMapper.Verify(m => m.Map(endpointResult, context.HttpContext), Times.Once);
        }

        /// <summary>
        /// Tests that the filter correctly handles an <see cref="EndpointResult{Unit}"/> (no value)
        /// from the next delegate and uses the mapper.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsEndpointResultUnit_MapsAndReturnsIResult()
        {
            // Arrange
            EndpointResult<Unit> endpointResult = EndpointResult<Unit>.From(Unit.Value); // Concrete Unit result
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.NoContent();
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(endpointResult);

            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()))
                .Returns(expectedIResult);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            this._mockMapper.Verify(m => m.Map(endpointResult, context.HttpContext), Times.Once);
        }

        /// <summary>
        /// Tests that the filter correctly handles a failed <see cref="IEndpointResult"/>
        /// from the next delegate and uses the mapper.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsFailedEndpointResult_MapsAndReturnsIResult()
        {
            // Arrange
            ErrorInfo errorInfo = new ErrorInfo(ErrorCategory.InternalServerError, "TEST_ERROR", "A test error occurred.");
            EndpointResult<int> failedEndpointResult = EndpointResult<int>.From(errorInfo);
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = "Test Problem" };
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Problem(
                title: problemDetails.Title,
                type: problemDetails.Type,
                statusCode: problemDetails.Status,
                detail: problemDetails.Detail,
                instance: problemDetails.Instance);
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(failedEndpointResult);

            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()))
                .Returns(expectedIResult);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            this._mockMapper.Verify(m => m.Map(failedEndpointResult, context.HttpContext), Times.Once);
        }

        /// <summary>
        /// Tests that the filter does NOT interfere when the next delegate returns a standard <see cref="Microsoft.AspNetCore.Http.IResult"/>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsStandardIResult_DoesNotMapAndReturnsOriginal()
        {
            // Arrange
            Microsoft.AspNetCore.Http.IResult originalIResult = Microsoft.AspNetCore.Http.Results.Ok("Standard API response");
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(originalIResult);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(originalIResult); // Should return the original IResult
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()), Times.Never); // Mapper should NOT be called
        }

        /// <summary>
        /// Tests that the filter does NOT interfere when the next delegate returns a plain object (POCO).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsPlainObject_DoesNotMapAndReturnsOriginal()
        {
            // Arrange
            object plainObject = new { Message = "Plain object response" };
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(plainObject);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(plainObject); // Should return the original object
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()), Times.Never); // Mapper should NOT be called
        }

        /// <summary>
        /// Tests that the filter does NOT interfere when the next delegate returns <c>null</c>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_WhenNextReturnsNull_DoesNotMapAndReturnsOriginal()
        {
            // Arrange
            object? nullResult = null;
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult(nullResult);

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeNull(); // Should return null
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointResult>(), It.IsAny<HttpContext>()), Times.Never);
        }

        /// <summary>
        /// Tests that <see cref="NormalizeEndpointResultFilter.InvokeAsync(Microsoft.AspNetCore.Http.EndpointFilterInvocationContext, Microsoft.AspNetCore.Http.EndpointFilterDelegate)"/> throws <see cref="ArgumentNullException"/>
        /// if the context is <c>null</c>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointFilterInvocationContext nullContext = null!;
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(null);

            // Act & Assert
            Func<Task> act = async () => await this._filter.InvokeAsync(nullContext, next).ConfigureAwait(false);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("context");
        }

        /// <summary>
        /// Tests that <see cref="NormalizeEndpointResultFilter.InvokeAsync(Microsoft.AspNetCore.Http.EndpointFilterInvocationContext, Microsoft.AspNetCore.Http.EndpointFilterDelegate)"/> throws <see cref="ArgumentNullException"/>
        /// if the next delegate is <c>null</c>.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Fact]
        public async Task InvokeAsync_NullNextDelegate_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate nullNext = null!;

            // Act & Assert
            Func<Task> act = async () => await this._filter.InvokeAsync(context, nullNext).ConfigureAwait(false);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("next");
        }

        /// <summary>
        /// Creates a mock <see cref="EndpointFilterInvocationContext"/> with a default <see cref="DefaultHttpContext"/>.
        /// </summary>
        /// <param name="httpContext">Optional: A specific <see cref="HttpContext"/> to use for the context.</param>
        /// <returns>A mock <see cref="EndpointFilterInvocationContext"/>.</returns>
        private static EndpointFilterInvocationContext CreateMockContext(HttpContext? httpContext = null)
        {
            Mock<EndpointFilterInvocationContext> mockContext = new Mock<EndpointFilterInvocationContext>();
            mockContext.Setup(c => c.HttpContext).Returns(httpContext ?? new DefaultHttpContext());
            return mockContext.Object;
        }
    }
}
