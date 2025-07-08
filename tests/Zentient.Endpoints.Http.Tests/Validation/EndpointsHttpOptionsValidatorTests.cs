// <copyright file="EndpointOutcomeExtensionsTests.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Zentient.Endpoints.Http.Validation;
using Zentient.Endpoints.Tests.Common;
using Zentient.Results;
using Zentient.Endpoints.Http.Options;
using Microsoft.AspNetCore.Http;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests.Http.Validation
{
    public class EndpointsHttpOptionsValidatorTests
    {
        private readonly EndpointsHttpOptionsValidator _validator = new EndpointsHttpOptionsValidator();

        [Fact]
        public void Validate_ProblemDetailsBaseTypeUri_Success_NullUri()
        {
            // Arrange
            var options = TestOptionsFactory.CreateProblemDetailsOptions(baseTypeUri: null).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Validate_ProblemDetailsBaseTypeUri_Success_ValidAbsoluteUriEndingWithSlash()
        {
            // Arrange
            var options = TestOptionsFactory.CreateProblemDetailsOptions(baseTypeUri: new Uri("https://example.com/errors/")).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Validate_ProblemDetailsBaseTypeUri_Failure_RelativeUri()
        {
            // Arrange
            var options = new EndpointsHttpOptions();
            options.ProblemDetails.BaseTypeUri = new Uri("/api/errors", UriKind.Relative);

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain("ProblemDetails.BaseTypeUri must be an absolute URI.");
        }

        [Fact]
        public void Validate_ProblemDetailsBaseTypeUri_Failure_AbsoluteUriNotEndingWithSlash()
        {
            // Arrange
            var options = TestOptionsFactory.CreateProblemDetailsOptions(baseTypeUri: new Uri("https://example.com/errors")).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain("ProblemDetails.BaseTypeUri should end with a '/' to allow proper concatenation with error codes.");
        }

        [Fact]
        public void Validate_ProblemDetailsCategoryToStatusCodeMap_Success_EmptyMap()
        {
            // Arrange
            var options = TestOptionsFactory.CreateProblemDetailsOptions(categoryToStatusCodeMap: new Dictionary<string, int>()).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Validate_ProblemDetailsCategoryToStatusCodeMap_Success_ValidMap()
        {
            // Arrange
            var validMap = new Dictionary<string, int>
            {
                { ErrorCategory.Validation.ToString(), StatusCodes.Status400BadRequest },
                { ErrorCategory.NotFound.ToString(), StatusCodes.Status404NotFound },
                { "CustomCategory", StatusCodes.Status418ImATeapot }
            };
            var options = TestOptionsFactory.CreateProblemDetailsOptions(categoryToStatusCodeMap: validMap).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Validate_ProblemDetailsCategoryToStatusCodeMap_Failure_EmptyCategoryKey()
        {
            // Arrange
            var options = new EndpointsHttpOptions();
            options.ProblemDetails.CategoryToStatusCodeMap.Add(string.Empty, StatusCodes.Status400BadRequest);

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain("ProblemDetails.CategoryToStatusCodeMap contains a null or empty category key.");
        }

        [Theory]
        [InlineData(99)]
        [InlineData(600)]
        public void Validate_ProblemDetailsCategoryToStatusCodeMap_Failure_InvalidStatusCode(int invalidCode)
        {
            // Arrange
            var options = TestOptionsFactory.CreateProblemDetailsOptions(
                categoryToStatusCodeMap: new Dictionary<string, int> { { "TestCategory", invalidCode } }).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain($"ProblemDetails.CategoryToStatusCodeMap[TestCategory] has an invalid HTTP status code: {invalidCode}.");
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(202)]
        [InlineData(203)]
        [InlineData(204)]
        [InlineData(205)]
        [InlineData(299)]
        public void Validate_SuccessResponseDefaultOkStatusCode_Success_Valid2xxCode(int statusCode)
        {
            // Arrange
            var options = TestOptionsFactory.CreateSuccessResponseOptions(defaultOkStatusCode: statusCode).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData(199)]
        [InlineData(300)]
        [InlineData(400)]
        [InlineData(500)]
        public void Validate_SuccessResponseDefaultOkStatusCode_Failure_InvalidCode(int invalidCode)
        {
            // Arrange
            var options = TestOptionsFactory.CreateSuccessResponseOptions(defaultOkStatusCode: invalidCode).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain("SuccessResponse.DefaultOkStatusCode must be a 2xx status code (200–299).");
        }

        [Fact]
        public void Validate_SuccessResponseDefaultNoContentStatusCode_Success_Is204()
        {
            // Arrange
            var options = TestOptionsFactory.CreateSuccessResponseOptions(defaultNoContentStatusCode: 204).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Theory]
        [InlineData(200)]
        [InlineData(201)]
        [InlineData(205)]
        [InlineData(404)]
        public void Validate_SuccessResponseDefaultNoContentStatusCode_Failure_IsNot204(int invalidCode)
        {
            // Arrange
            var options = TestOptionsFactory.CreateSuccessResponseOptions(defaultNoContentStatusCode: invalidCode).Value;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().ContainSingle()
                .Which.Should().Contain("SuccessResponse.DefaultNoContentStatusCode must be 204 (No Content) as per HTTP specification.");
        }

        [Fact]
        public void Validate_CombinedFailures_AllRulesInvalid()
        {
            // Arrange
            var options = new EndpointsHttpOptions();

            options.ProblemDetails.BaseTypeUri = new Uri("/relative/uri", UriKind.Relative);
            options.ProblemDetails.CategoryToStatusCodeMap.Add("InvalidCategory", 99);
            options.ProblemDetails.CategoryToStatusCodeMap.Add(string.Empty, 400);

            options.SuccessResponse.DefaultOkStatusCode = 100;
            options.SuccessResponse.DefaultNoContentStatusCode = 200;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().HaveCount(5);

            result.Failures.Should().Contain("ProblemDetails.BaseTypeUri must be an absolute URI.");
            result.Failures.Should().Contain("ProblemDetails.CategoryToStatusCodeMap[InvalidCategory] has an invalid HTTP status code: 99.");
            result.Failures.Should().Contain("ProblemDetails.CategoryToStatusCodeMap contains a null or empty category key.");
            result.Failures.Should().Contain("SuccessResponse.DefaultOkStatusCode must be a 2xx status code (200–299).");
            result.Failures.Should().Contain("SuccessResponse.DefaultNoContentStatusCode must be 204 (No Content) as per HTTP specification.");
        }

        [Fact]
        public void Validate_CombinedFailures_MixOfValidAndInvalid()
        {
            // Arrange
            var options = new EndpointsHttpOptions();

            options.ProblemDetails.BaseTypeUri = new Uri("https://valid.com/errors/");
            options.ProblemDetails.CategoryToStatusCodeMap.Add("ValidCategory", 400);
            options.ProblemDetails.CategoryToStatusCodeMap.Add("AnotherInvalid", 600);

            options.SuccessResponse.DefaultOkStatusCode = 200;
            options.SuccessResponse.DefaultNoContentStatusCode = 201;

            // Act
            var result = _validator.Validate(nameof(EndpointsHttpOptions), options);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Failures.Should().HaveCount(2);

            result.Failures.Should().Contain("ProblemDetails.CategoryToStatusCodeMap[AnotherInvalid] has an invalid HTTP status code: 600.");
            result.Failures.Should().Contain("SuccessResponse.DefaultNoContentStatusCode must be 204 (No Content) as per HTTP specification.");

            result.Failures.Should().NotContain("ProblemDetails.BaseTypeUri must be an absolute URI.");
            result.Failures.Should().NotContain("ProblemDetails.BaseTypeUri should end with a '/'");
            result.Failures.Should().NotContain("SuccessResponse.DefaultOkStatusCode must be a 2xx status code (200–299).");
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
