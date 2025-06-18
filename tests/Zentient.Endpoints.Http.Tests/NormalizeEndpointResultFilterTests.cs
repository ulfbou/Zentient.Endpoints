// <copyright file="NormalizeEndpointOutcomeFilterTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Results;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public sealed class NormalizeEndpointOutcomeFilterTests
    {
        private readonly Mock<IEndpointOutcomeToHttpMapper> _mockMapper;
        private readonly NormalizeEndpointOutcomeFilter _filter;

        public NormalizeEndpointOutcomeFilterTests()
        {
            this._mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            this._filter = new NormalizeEndpointOutcomeFilter(this._mockMapper.Object);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextReturnsEndpointOutcome_MapsAndReturnsIResult()
        {
            // Arrange
            string testValue = "Success!";
            // Corrected: Use the Success factory method
            IEndpointOutcome<string> endpointOutcome = EndpointOutcome<string>.Success(testValue);
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok(testValue);
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(endpointOutcome);

            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(expectedIResult));

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            this._mockMapper.Verify(m => m.Map(endpointOutcome, context.HttpContext), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextReturnsEndpointOutcomeUnit_MapsAndReturnsIResult()
        {
            // Arrange
            // Corrected: Use the NoContent factory method for Unit outcomes
            IEndpointOutcome<Unit> endpointOutcome = EndpointOutcome<Unit>.NoContent();
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.NoContent();
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(endpointOutcome);

            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(expectedIResult));

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            // Verify with the IEndpointOutcome interface type
            this._mockMapper.Verify(m => m.Map(endpointOutcome, context.HttpContext), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextReturnsFailedEndpointOutcome_MapsAndReturnsIResult()
        {
            // Arrange
            ErrorInfo errorInfo = new ErrorInfo(ErrorCategory.InternalServerError, "TEST_ERROR", "A test error occurred.");
            EndpointOutcome<int> failedEndpointOutcome = (EndpointOutcome<int>)EndpointOutcome<int>.From(errorInfo);
            Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails { Title = "Test Problem" };
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Problem(
                title: problemDetails.Title,
                type: problemDetails.Type,
                statusCode: problemDetails.Status,
                detail: problemDetails.Detail,
                instance: problemDetails.Instance);
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate next = (ctx) => ValueTask.FromResult<object?>(failedEndpointOutcome);

            this._mockMapper
                .Setup(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()))
                .Returns(Task.FromResult(expectedIResult));

            // Act
            object? actualResult = await this._filter.InvokeAsync(context, next);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            this._mockMapper.Verify(m => m.Map(failedEndpointOutcome, context.HttpContext), Times.Once);
        }

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
            actualResult.Should().BeSameAs(originalIResult);
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()), Times.Never);
        }

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
            actualResult.Should().BeSameAs(plainObject);
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()), Times.Never);
        }

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
            actualResult.Should().BeNull();
            this._mockMapper.Verify(m => m.Map(It.IsAny<IEndpointOutcome>(), It.IsAny<HttpContext>()), Times.Never);
        }

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

        [Fact]
        public async Task InvokeAsync_NullNextDelegate_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointFilterInvocationContext context = CreateMockContext();
            EndpointFilterDelegate nullNext = null!;

            // Act & Assert
            Func<Task> act = async () => await this._filter.InvokeAsync(context, nullNext);
            await act.Should().ThrowExactlyAsync<ArgumentNullException>().WithParameterName("next");
        }

        private static EndpointFilterInvocationContext CreateMockContext(HttpContext? httpContext = null)
        {
            Mock<EndpointFilterInvocationContext> mockContext = new Mock<EndpointFilterInvocationContext>();
            mockContext.Setup(c => c.HttpContext).Returns(httpContext ?? new DefaultHttpContext());
            return mockContext.Object;
        }
    }
}
#pragma warning restore CS1591
