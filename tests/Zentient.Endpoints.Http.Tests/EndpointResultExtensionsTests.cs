// <copyright file="EndpointOutcomeExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

using Zentient.Endpoints;
using Zentient.Endpoints.Http;
using Zentient.Results;

#pragma warning disable CS1591
namespace Zentient.Endpoints.Http.Tests
{
    public sealed class EndpointOutcomeExtensionsTests
    {
        [Fact]
        public async Task ToHttpResult_IEndpointOutcome_Successful_DelegatesToMapperAsync()
        {
            // Arrange
            Mock<IEndpointOutcomeToHttpMapper> mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            IEndpointOutcome<string> endpointOutcome = EndpointOutcome<string>.Success("Success!");
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok("Mapped!");
            mockMapper.Setup(m => m.Map(endpointOutcome, httpContext)).Returns(Task.FromResult(expectedIResult));

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = await endpointOutcome.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointOutcome, httpContext), Times.Once);
        }

        [Fact]
        public async Task ToHttpResult_IEndpointOutcome_Failed_DelegatesToMapper()
        {
            // Arrange
            Mock<IEndpointOutcomeToHttpMapper> mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            ErrorInfo error = new ErrorInfo(ErrorCategory.InternalServerError, "TEST_ERROR", "Test error.");
            IEndpointOutcome<int> endpointOutcome = EndpointOutcome<int>.From(error);
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Problem("Mapped Problem!");

            mockMapper.Setup(m => m.Map(endpointOutcome, httpContext)).Returns(Task.FromResult(expectedIResult));

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = await endpointOutcome.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointOutcome, httpContext), Times.Once);
        }

        [Fact]
        public void ToHttpResult_IEndpointOutcome_MapperNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            DefaultHttpContext httpContext = new DefaultHttpContext();
            IEndpointOutcome endpointOutcome = EndpointOutcome<Unit>.NoContent(); // Corrected for Unit
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            httpContext.RequestServices = services;

            // Act
            Func<Task> act = async () => await endpointOutcome.ToHttpResult(httpContext).ConfigureAwait(false);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
               .WithMessage("No service for type 'Zentient.Endpoints.Http.IEndpointOutcomeToHttpMapper' has been registered*");
        }

        [Fact]
        public async Task ToHttpResult_IEndpointOutcome_NullEndpointOutcome_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointOutcome nullEndpointOutcome = null!;
            Mock<IEndpointOutcomeToHttpMapper> mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);

            // Act
            Func<Task> act = () => nullEndpointOutcome.ToHttpResult(httpContext);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("endpointResult");
        }

        [Fact]
        public async Task ToHttpResult_IEndpointOutcome_NullHttpContext_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointOutcome endpointOutcome = EndpointOutcome<string>.Success("Test");
            HttpContext nullHttpContext = null!;

            // Act
            Func<Task> act = () => endpointOutcome.ToHttpResult(nullHttpContext);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("httpContext");
        }

        [Fact]
        public async Task ToHttpResult_GenericEndpointOutcome_Successful_DelegatesToMapperAsync()
        {
            // Arrange
            Mock<IEndpointOutcomeToHttpMapper> mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            IEndpointOutcome<DateTime> endpointOutcome = EndpointOutcome<DateTime>.Success(DateTime.Now);
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok("Mapped DateTime!");
            mockMapper.Setup(m => m.Map(endpointOutcome, httpContext)).Returns(Task.FromResult(expectedIResult));

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = await endpointOutcome.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointOutcome, httpContext), Times.Once);
        }

        [Fact]
        public void ToHttpResult_GenericEndpointOutcome_NullEndpointOutcome_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointOutcome<string> nullEndpointOutcome = null!;
            Mock<IEndpointOutcomeToHttpMapper> mockMapper = new Mock<IEndpointOutcomeToHttpMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);

            // Act
            Func<Task> act = async () => await nullEndpointOutcome.ToHttpResult(httpContext).ConfigureAwait(false);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("endpointResult");
        }

        [Fact]
        public async Task ToHttpResult_GenericEndpointOutcome_NullHttpContext_ThrowsArgumentNullExceptionAsync()
        {
            // Arrange
            IEndpointOutcome<string> endpointOutcome = EndpointOutcome<string>.Success("Test value");
            HttpContext nullHttpContext = null!;

            // Act
            Func<Task<Microsoft.AspNetCore.Http.IResult>> act = async () => await endpointOutcome.ToHttpResult(nullHttpContext);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(act);
        }

        [Fact]
        public void ToMinimalApiResult_ReturnsOriginalInstance()
        {
            // Arrange
            IEndpointOutcome<int> originalResult = EndpointOutcome<int>.Success(42);

            // Act
            IEndpointOutcome<int> returnedResult = originalResult.ToMinimalApiResult();

            // Assert
            returnedResult.Should().BeSameAs(originalResult);
        }

        [Fact]
        public void ToMinimalApiResult_NullEndpointOutcome_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointOutcome<int> nullEndpointOutcome = null!;

            // Act
            Action act = () => nullEndpointOutcome.ToMinimalApiResult();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        [Fact]
        public void ToEndpointOutcome_GenericIResult_Successful_ConvertsCorrectly()
        {
            // Arrange
            IResult<string> zentientResult = Zentient.Results.Result<string>.Success("Data");

            // Act
            IEndpointOutcome<string> endpointOutcome = zentientResult.ToEndpointOutcome();

            // Assert
            endpointOutcome.Should().NotBeNull();
            endpointOutcome.IsSuccess.Should().BeTrue();
            endpointOutcome.Value.Should().Be("Data");
            endpointOutcome.TransportMetadata.Should().NotBeNull();
            endpointOutcome.TransportMetadata.HttpStatusCode.Should().BeNull();
        }

        [Fact]
        public void ToEndpointOutcome_GenericIResult_Failed_ConvertsCorrectly()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.InternalServerError, "TEST_ERROR", "Test error.");
            IResult<string> zentientResult = error.AsError<string>(null);

            // Act
            IEndpointOutcome<string> endpointOutcome = zentientResult.ToEndpointOutcome();

            // Assert
            endpointOutcome.Should().NotBeNull();
            endpointOutcome.IsSuccess.Should().BeFalse();
            endpointOutcome.Errors.Should().NotBeNull();
            endpointOutcome.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(error);
            endpointOutcome.TransportMetadata.Should().NotBeNull();
            endpointOutcome.TransportMetadata.HttpStatusCode.Should().BeNull();
        }

        [Fact]
        public void ToEndpointOutcome_GenericIResult_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            Zentient.Results.IResult<string> nullResult = null!;

            // Act
            Action act = () => nullResult.ToEndpointOutcome();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        [Fact]
        public void ToEndpointOutcome_NonGenericIResult_Successful_ConvertsToUnit()
        {
            // Arrange
            Results.IResult zentientResult = Result.Success();

            // Act
            IEndpointOutcome<Unit> endpointOutcome = zentientResult.ToEndpointOutcome();

            // Assert
            endpointOutcome.Should().NotBeNull();
            endpointOutcome.IsSuccess.Should().BeTrue();
            endpointOutcome.Value.Should().Be(Unit.Value);
            endpointOutcome.TransportMetadata.Should().NotBeNull();
            endpointOutcome.TransportMetadata.HttpStatusCode.Should().BeNull();
        }

        [Fact]
        public void ToEndpointOutcome_NonGenericIResult_FailedWithErrors_ConvertsToUnitWithError()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Authentication, "AUTH_FAIL", "Authentication failed.");
            Results.IResult zentientResult = Result.Failure(error);

            // Act
            IEndpointOutcome<Unit> endpointOutcome = zentientResult.ToEndpointOutcome();

            // Assert
            endpointOutcome.Should().NotBeNull();
            endpointOutcome.IsSuccess.Should().BeFalse();
            endpointOutcome.ErrorMessage.Should().Be(error.Message);
            endpointOutcome.Errors.Should().ContainSingle().And.ContainEquivalentOf(error);
            endpointOutcome.TransportMetadata.Should().NotBeNull();
            endpointOutcome.TransportMetadata.HttpStatusCode.Should().BeNull();
        }

        [Fact]
        public void ToEndpointOutcome_NonGenericIResult_FailedWithNoErrors_GeneratesDefaultInternalError()
        {
            // Arrange
            Mock<Zentient.Results.IResult> mockResult = new Mock<Zentient.Results.IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(false);
            mockResult.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>().AsReadOnly());
            mockResult.SetupGet(r => r.Status).Returns(ResultStatuses.Error);
            mockResult.SetupGet(r => r.ErrorMessage).Returns("An unknown error occurred.");

            // Act
            IEndpointOutcome<Unit> endpointOutcome = mockResult.Object.ToEndpointOutcome();

            // Assert
            endpointOutcome.Should().NotBeNull();
            endpointOutcome.IsSuccess.Should().BeFalse();
            endpointOutcome.ErrorMessage.Should().NotBeNull();
            endpointOutcome.Errors.Should().ContainSingle();
            endpointOutcome.Errors[0].Code.Should().Be("InternalError");
            endpointOutcome.Errors[0].Message.Should().Contain("An unknown error occurred.");
            endpointOutcome.TransportMetadata.Should().NotBeNull();
            endpointOutcome.TransportMetadata.HttpStatusCode.Should().BeNull();
        }

        [Fact]
        public void ToEndpointOutcome_NonGenericIResult_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            Zentient.Results.IResult nullResult = null!;

            // Act
            Action act = () => nullResult.ToEndpointOutcome();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        private static DefaultHttpContext CreateHttpContextWithMapper(Mock<IEndpointOutcomeToHttpMapper> mapperMock)
        {
            var services = new ServiceCollection();
            services.AddSingleton(mapperMock.Object);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            return httpContext;
        }
    }
}
#pragma warning restore CS1591
