// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EndpointResultExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

using Zentient.Endpoints.Core;
using Zentient.Endpoints.Http;
using Zentient.Results;

namespace Zentient.Endpoints.Http.Tests
{
    /// <summary>
    /// Contains sophisticated and exhaustive unit tests for the <see cref="EndpointResultExtensions"/> class.
    /// These tests ensure proper conversion from Zentient.Results to EndpointResults,
    /// and from EndpointResults to ASP.NET Core IResults, delegating to the appropriate mappers.
    /// </summary>
    public sealed partial class EndpointResultExtensionsTests
    {
        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/>
        /// correctly delegates to the registered mapper for a successful result.
        /// </summary>
        [Fact]
        public void ToHttpResult_IEndpointResult_Successful_DelegatesToMapper()
        {
            // Arrange
            Mock<IEndpointResultToHttpResultMapper> mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            EndpointResult<string> endpointResult = EndpointResult<string>.From("Success!");
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok("Mapped!");

            mockMapper.Setup(m => m.Map(endpointResult, httpContext)).Returns(expectedIResult);

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = endpointResult.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointResult, httpContext), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/>
        /// correctly delegates to the registered mapper for a failed result.
        /// </summary>
        [Fact]
        public void ToHttpResult_IEndpointResult_Failed_DelegatesToMapper()
        {
            // Arrange
            Mock<IEndpointResultToHttpResultMapper> mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            ErrorInfo error = new ErrorInfo(ErrorCategory.InternalServerError, "TEST_ERROR", "Test error.");
            EndpointResult<int> endpointResult = EndpointResult<int>.From(error);
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Problem("Mapped Problem!");

            mockMapper.Setup(m => m.Map(endpointResult, httpContext)).Returns(expectedIResult);

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = endpointResult.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointResult, httpContext), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/>
        /// throws <see cref="InvalidOperationException"/> if <see cref="IEndpointResultToHttpResultMapper"/> is not registered in DI.
        /// </summary>
        [Fact]
        public void ToHttpResult_IEndpointResult_MapperNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            DefaultHttpContext httpContext = new DefaultHttpContext();
            IEndpointResult endpointResult = EndpointResult<Unit>.From(Unit.Value);
            IServiceProvider services = new ServiceCollection().BuildServiceProvider();
            httpContext.RequestServices = services;

            // Act
            Action act = () => endpointResult.ToHttpResult(httpContext);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("No service for type 'Zentient.Endpoints.Http.IEndpointResultToHttpResultMapper' has been registered*");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/>
        /// throws <see cref="ArgumentNullException"/> if the endpoint result is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToHttpResult_IEndpointResult_NullEndpointResult_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointResult nullEndpointResult = null!;
            Mock<IEndpointResultToHttpResultMapper> mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);

            // Act
            Action act = () => nullEndpointResult.ToHttpResult(httpContext);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/>
        /// throws <see cref="ArgumentNullException"/> if the HTTP context argument is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToHttpResult_IEndpointResult_NullHttpContext_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointResult endpointResult = EndpointResult<string>.From("Test");
            HttpContext nullHttpContext = null!;

            // Act
            Action act = () => endpointResult.ToHttpResult(nullHttpContext);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContext");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult"/>
        /// correctly delegates to the base <see cref="EndpointResultExtensions.ToHttpResult(IEndpointResult, HttpContext)"/> for a strongly-typed successful result.
        /// </summary>
        [Fact]
        public void ToHttpResult_GenericEndpointResult_Successful_DelegatesToMapper()
        {
            // Arrange
            Mock<IEndpointResultToHttpResultMapper> mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);
            EndpointResult<DateTime> endpointResult = EndpointResult<DateTime>.From(DateTime.Now);
            Microsoft.AspNetCore.Http.IResult expectedIResult = Microsoft.AspNetCore.Http.Results.Ok("Mapped DateTime!");
            mockMapper.Setup(m => m.Map(endpointResult, httpContext)).Returns(expectedIResult);

            // Act
            Microsoft.AspNetCore.Http.IResult actualResult = endpointResult.ToHttpResult(httpContext);

            // Assert
            actualResult.Should().BeSameAs(expectedIResult);
            mockMapper.Verify(m => m.Map(endpointResult, httpContext), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult"/>
        /// throws <see cref="ArgumentNullException"/> if the endpoint result is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToHttpResult_GenericEndpointResult_NullEndpointResult_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointResult<string> nullEndpointResult = null!;
            Mock<IEndpointResultToHttpResultMapper> mockMapper = new Mock<IEndpointResultToHttpResultMapper>();
            DefaultHttpContext httpContext = CreateHttpContextWithMapper(mockMapper);

            // Act
            Action act = () => nullEndpointResult.ToHttpResult(httpContext);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToHttpResult"/>
        /// throws <see cref="ArgumentNullException"/> if the <see cref="HttpContext" /> argument is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToHttpResult_GenericEndpointResult_NullHttpContext_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointResult<string> endpointResult = EndpointResult<string>.From("Test");
            HttpContext nullHttpContext = null!;

            // Act
            Action act = () => endpointResult.ToHttpResult(nullHttpContext);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("httpContext");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToMinimalApiResult{TResult}(EndpointResult{TResult})"/>
        /// returns the original <see cref="EndpointResult{TResult}"/> instance. This method is primarily
        /// for type hinting and does not perform actual mapping.
        /// </summary>
        [Fact]
        public void ToMinimalApiResult_ReturnsOriginalInstance()
        {
            // Arrange
            EndpointResult<int> originalResult = EndpointResult<int>.From(42);

            // Act
            EndpointResult<int> returnedResult = originalResult.ToMinimalApiResult();

            // Assert
            returnedResult.Should().BeSameAs(originalResult);
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToMinimalApiResult{TResult}(EndpointResult{TResult})"/>
        /// throws <see cref="ArgumentNullException"/> if the endpoint result is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToMinimalApiResult_NullEndpointResult_ThrowsArgumentNullException()
        {
            // Arrange
            EndpointResult<int> nullEndpointResult = null!;

            // Act
            Action act = () => nullEndpointResult.ToMinimalApiResult();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("endpointResult");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult{TValue}(Zentient.Results.IResult{TValue})"/>
        /// correctly converts a successful <see cref="Zentient.Results.IResult{TValue}"/> to an <see cref="EndpointResult{TValue}"/>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_GenericIResult_Successful_ConvertsCorrectly()
        {
            // Arrange
            IResult<string> zentientResult = Zentient.Results.Result.Success("Data");

            // Act
            EndpointResult<string> endpointResult = zentientResult.ToEndpointResult();

            // Assert
            endpointResult.Should().NotBeNull();
            endpointResult.IsSuccess.Should().BeTrue();
            endpointResult.Result.Should().Be("Data");
            endpointResult.BaseTransport.Should().NotBeNull();
            endpointResult.BaseTransport.HttpStatusCode.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult{TValue}(Zentient.Results.IResult{TValue})"/>
        /// correctly converts a failed <see cref="Zentient.Results.IResult{TValue}"/> to an <see cref="EndpointResult{TValue}"/>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_GenericIResult_Failed_ConvertsCorrectly()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Validation, "CODE", "Message");
            IResult<string> zentientResult = Zentient.Results.Result.Failure<string>(error);

            // Act
            EndpointResult<string> endpointResult = zentientResult.ToEndpointResult();

            // Assert
            endpointResult.Should().NotBeNull();
            endpointResult.IsSuccess.Should().BeFalse();
            endpointResult.Error.Should().BeEquivalentTo(error);
            endpointResult.BaseTransport.Should().NotBeNull();
            endpointResult.BaseTransport.HttpStatusCode.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult{TValue}(Zentient.Results.IResult{TValue})"/>
        /// throws <see cref="ArgumentNullException"/> if the result is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_GenericIResult_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            Zentient.Results.IResult<string> nullResult = null!;

            // Act
            Action act = () => nullResult.ToEndpointResult();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult(Zentient.Results.IResult)"/>
        /// correctly converts a successful non-generic <see cref="Zentient.Results.IResult"/> to <see cref="EndpointResult{Unit}"/>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_NonGenericIResult_Successful_ConvertsToUnit()
        {
            // Arrange
            Results.IResult zentientResult = Result.Success();

            // Act
            EndpointResult<Unit> endpointResult = zentientResult.ToEndpointResult();

            // Assert
            endpointResult.Should().NotBeNull();
            endpointResult.IsSuccess.Should().BeTrue();
            endpointResult.Result.Should().Be(Unit.Value);
            endpointResult.BaseTransport.Should().NotBeNull();
            endpointResult.BaseTransport.HttpStatusCode.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult(Zentient.Results.IResult)"/>
        /// correctly converts a failed non-generic <see cref="Zentient.Results.IResult"/> with errors to <see cref="EndpointResult{Unit}"/>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_NonGenericIResult_FailedWithErrors_ConvertsToUnitWithError()
        {
            // Arrange
            ErrorInfo error = new ErrorInfo(ErrorCategory.Unauthorized, "AUTH_FAIL", "Authentication failed.");
            Results.IResult zentientResult = Result.Failure(error);

            // Act
            EndpointResult<Unit> endpointResult = zentientResult.ToEndpointResult();

            // Assert
            endpointResult.Should().NotBeNull();
            endpointResult.IsSuccess.Should().BeFalse();
            endpointResult.Error.Should().BeEquivalentTo(error);
            endpointResult.BaseTransport.Should().NotBeNull();
            endpointResult.BaseTransport.HttpStatusCode.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult(Zentient.Results.IResult)"/>
        /// correctly handles a failed non-generic <see cref="Zentient.Results.IResult"/> with no errors
        /// by generating a default internal error.
        /// </summary>
        [Fact]
        public void ToEndpointResult_NonGenericIResult_FailedWithNoErrors_GeneratesDefaultInternalError()
        {
            // Arrange
            // Simulate an IResult that is a failure but has no errors (edge case)
            Mock<Zentient.Results.IResult> mockResult = new Mock<Zentient.Results.IResult>();
            mockResult.SetupGet(r => r.IsSuccess).Returns(false);
            mockResult.SetupGet(r => r.Errors).Returns(new List<ErrorInfo>().AsReadOnly());

            // Act
            EndpointResult<Unit> endpointResult = mockResult.Object.ToEndpointResult();

            // Assert
            endpointResult.Should().NotBeNull();
            endpointResult.IsSuccess.Should().BeFalse();
            endpointResult.Error.Should().NotBeNull();
            ((ErrorInfo)endpointResult.Error!).Code.Should().Be("InternalError");
            ((ErrorInfo)endpointResult.Error!).Message.Should().Contain("An unknown error occurred.");
            endpointResult.BaseTransport.Should().NotBeNull();
            endpointResult.BaseTransport.HttpStatusCode.Should().BeNull();
        }

        /// <summary>
        /// Tests that <see cref="EndpointResultExtensions.ToEndpointResult(Zentient.Results.IResult)"/>
        /// throws <see cref="ArgumentNullException"/> if the result is <c>null</c>.
        /// </summary>
        [Fact]
        public void ToEndpointResult_NonGenericIResult_NullResult_ThrowsArgumentNullException()
        {
            // Arrange
            Zentient.Results.IResult nullResult = null!;

            // Act
            Action act = () => nullResult.ToEndpointResult();

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("result");
        }

        /// <summary>
        /// Creates a <see cref="DefaultHttpContext"/> with a mocked service provider
        /// that can resolve <see cref="IEndpointResultToHttpResultMapper"/>.
        /// </summary>
        /// <param name="mapperMock">The mock <see cref="IEndpointResultToHttpResultMapper"/> to register.</param>
        /// <returns>A configured <see cref="DefaultHttpContext"/>.</returns>
        private static DefaultHttpContext CreateHttpContextWithMapper(Mock<IEndpointResultToHttpResultMapper> mapperMock)
        {
            // 1. Create a new ServiceCollection to register services for this context
            var services = new ServiceCollection();

            // 2. Register your mocked mapper instance
            //    Use AddSingleton for simplicity in tests, as it ensures the exact same instance is returned.
            services.AddSingleton(mapperMock.Object);

            // 3. Build a ServiceProvider from the ServiceCollection
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            // 4. Create a DefaultHttpContext
            var httpContext = new DefaultHttpContext();

            // 5. Crucially, set the RequestServices property of the HttpContext
            //    This is where the extension method will look for its dependencies.
            httpContext.RequestServices = serviceProvider;

            return httpContext;
        }
    }
}
