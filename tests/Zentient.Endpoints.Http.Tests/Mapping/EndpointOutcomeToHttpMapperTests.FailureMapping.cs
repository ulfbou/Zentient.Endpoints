// <copyright file="EndpointOutcomeToHttpMapperTests.FailureMapping.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Xunit;
using FluentAssertions;
using Moq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

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
        [Fact]
        public async Task Map_Failure_UsesProblemDetailsOverride_WhenPresent()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var customProblem = TestProblemDetailsFactory.CreateBase(
                status: StatusCodes.Status400BadRequest,
                title: "Custom Bad Request",
                detail: "A specific override detail.");
            var metadata = new TransportMetadata().WithTag(HttpMetadataKeys.ProblemDetailsOverride, customProblem);
            var outcome = EndpointOutcome.FromError(ErrorInfo.General(code: "Code", message: "Error"), transportMetadata: metadata);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status400BadRequest, title: "Bad Request"));

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem.Should().NotBeNull();
            deserializedProblem!.Status.Should().Be(StatusCodes.Status400BadRequest);
            deserializedProblem.Title.Should().Be("Custom Bad Request");
            deserializedProblem.Detail.Should().Be("A specific override detail.");

            _mockProblemDetailsMapper.Verify(m => m.Map(It.IsAny<ErrorInfo>(), It.IsAny<HttpContext>()), Times.Never(), "Mapper should not be called when override exists.");
        }

        [Fact]
        public async Task Map_Failure_MapsErrorInfoToProblemDetails_WhenNoOverride()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeErrorCodeInExtensions = true;
            // FIX: Disable the option that appends error messages to the detail.
            // This test focuses on mapping ErrorInfo.Detail directly.
            testOptions.ProblemDetails.IncludeErrorInfoMessagesInDetail = false; // <--- ADD THIS LINE
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var errorInfo = TestErrorInfoFactory.Validation(code: "VLD-001", message: "Validation failed.", detail: "Specific fields were invalid.");
            var outcome = EndpointOutcome.FromError(errorInfo);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(errorInfo, httpContext))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                    Detail = null // Ensure Detail is null initially for SetProblemDetailsDetail to populate
                });

            _mockProblemTypeUriGenerator.Setup(g => g.Generate(errorInfo.Code, httpContext))
                .ReturnsAsync("https://example.com/problems/VLD-001");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem.Should().NotBeNull();
            deserializedProblem!.Status.Should().Be(StatusCodes.Status400BadRequest);
            deserializedProblem.Title.Should().Be("Bad Request");
            deserializedProblem.Detail.Should().Be("Specific fields were invalid."); // This assertion should now pass
            deserializedProblem.Type.Should().Be("https://example.com/problems/VLD-001");
            deserializedProblem.Instance.Should().Be("/mocked-test-path");
            deserializedProblem.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            var errorCodeElement = (JsonElement)deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.ErrorCode]!;
            errorCodeElement.Should().NotBeNull();
            errorCodeElement.ValueKind.Should().Be(JsonValueKind.String);
            errorCodeElement.GetString().Should().Be("VLD-001");

            deserializedProblem.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.TraceId);
            var traceIdElement = (JsonElement)deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.TraceId]!;
            traceIdElement.Should().NotBeNull();
            traceIdElement.ValueKind.Should().Be(JsonValueKind.String);
            traceIdElement.GetString().Should().Be(httpContext.TraceIdentifier);

            deserializedProblem.Extensions.Should().NotContainKey(ProblemDetailsConstants.Detail);
        }

        [Fact]
        public async Task Map_Failure_UsesDefaultInternalServerError_WhenNoErrors()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeErrorCodeInExtensions = true;
            // FIX 1: Enable inclusion of error info messages in detail for this test.
            // This allows the message from ErrorInfo.General() to populate the ProblemDetails.Detail.
            testOptions.ProblemDetails.IncludeErrorInfoMessagesInDetail = true; // <--- ADD OR ENSURE THIS IS TRUE
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }

            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);
            var errorInfoFromGeneral = ErrorInfo.General();
            var outcome = EndpointOutcome.FromError(errorInfoFromGeneral);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            // FIX 2: Ensure the mocked ProblemDetails has its Detail property initially null.
            // This allows SetProblemDetailsDetail to correctly set the detail from outcome.Messages.
            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails // Create a new ProblemDetails object directly
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = ResultStatuses.InternalServerError.Description,
                    Detail = null // Ensure Detail is null so it can be populated by messages
                });

            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/INTERNAL_SERVER_ERROR");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem.Should().NotBeNull();
            deserializedProblem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
            deserializedProblem.Title.Should().Be(ResultStatuses.InternalServerError.Description);
            deserializedProblem.Detail.Should().Be("An unspecified error occurred."); // This should now pass
            deserializedProblem.Type.Should().Be("https://example.com/problems/INTERNAL_SERVER_ERROR");

            deserializedProblem.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            var errorCodeElement = (JsonElement)deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.ErrorCode]!;
            errorCodeElement.ValueKind.Should().Be(JsonValueKind.String);
            errorCodeElement.GetString().Should().Be("GENERAL_ERROR");

            deserializedProblem.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.TraceId);
            var traceIdElement = (JsonElement)deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.TraceId]!;
            traceIdElement.ValueKind.Should().Be(JsonValueKind.String);
            traceIdElement.GetString().Should().Be(httpContext.TraceIdentifier);
        }

        [Fact]
        public async Task Map_Failure_StatusCodeHintOverridesProblemDetailsStatus()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var errorInfo = TestErrorInfoFactory.Authentication("AUTH-001");
            var metadata = new TransportMetadata().WithTag(HttpMetadataKeys.HttpStatusCodeHint, StatusCodes.Status403Forbidden);
            var outcome = EndpointOutcome.FromError(errorInfo, transportMetadata: metadata);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(errorInfo, httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status401Unauthorized, title: "Unauthorized"));

            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/UNAUTHORIZED");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);
            deserializedProblem!.Status.Should().Be(StatusCodes.Status403Forbidden);
            deserializedProblem.Title.Should().Be(ResultStatuses.Forbidden.Description);
        }

        [Fact]
        public async Task Map_Failure_IncludesStackTrace_InDevelopmentEnvironment_WhenConfigured()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Development");

            var testOptions = (EndpointsHttpOptions)_defaultOptions.Clone();
            testOptions.ProblemDetails.IncludeStackTrace = true;

            Exception actualException;
            try
            {
                throw new InvalidOperationException("Internal Error Test Message");
            }
            catch (InvalidOperationException ex)
            {
                actualException = ex;
            }

            var errorInfo = TestErrorInfoFactory.InternalErrorFromException(actualException);
            var outcome = EndpointOutcome.FromError(errorInfo);

            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync((ErrorInfo mappedErrorInfo, HttpContext ctx) =>
                {
                    var problem = TestProblemDetailsFactory.CreateBase(StatusCodes.Status500InternalServerError);
                    if (mappedErrorInfo.Metadata.TryGetValue(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, out var st) && st is string stString)
                    {
                        problem.Extensions.Add(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, stString);
                    }
                    return problem;
                });

            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/INTERNAL_SERVER_ERROR");

            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().ContainKey(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);
            var stackTraceElement = deserializedProblem.Extensions[Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace] as JsonElement?;
            stackTraceElement.Should().NotBeNull();
            stackTraceElement.Value.ValueKind.Should().Be(JsonValueKind.String);
            stackTraceElement.Value.GetString().Should().NotBeNullOrEmpty();

            stackTraceElement.Value.GetString().Should().Contain(
                nameof(Map_Failure_IncludesStackTrace_InDevelopmentEnvironment_WhenConfigured));
        }

        [Fact]
        public async Task Map_Failure_DoesNotIncludeStackTrace_InProductionEnvironment()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");

            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeStackTrace = true;
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var stackTrace = "at MyMethod() in MyFile.cs:line 10";
            var errorInfo = TestErrorInfoFactory.InternalServerError(detail: "Internal Error");
            var metadata = new TransportMetadata().WithTag(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, stackTrace);
            var outcome = EndpointOutcome.FromError(errorInfo, transportMetadata: metadata);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status500InternalServerError));
            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/INTERNAL_SERVER_ERROR");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().NotContainKey(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);
        }

        [Fact]
        public async Task Map_Failure_DoesNotIncludeStackTrace_WhenNotConfigured()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Development");

            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeStackTrace = false;
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var stackTrace = "at MyMethod() in MyFile.cs:line 10";
            var errorInfo = TestErrorInfoFactory.InternalServerError(detail: "Internal Error");
            var metadata = new TransportMetadata().WithTag(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace, stackTrace);
            var outcome = EndpointOutcome.FromError(errorInfo, transportMetadata: metadata);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status500InternalServerError));
            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/INTERNAL_SERVER_ERROR");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().NotContainKey(Zentient.Results.Constants.MetadataKeys.ExceptionStackTrace);
        }

        [Fact]
        public async Task Map_Failure_IncludesErrorCodeInExtensions_WhenConfigured()
        {
            const string errorCode = "RES-001";

            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");

            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeErrorCodeInExtensions = true;
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var errorInfo = TestErrorInfoFactory.Conflict(code: errorCode, message: "Resource conflict.");
            var outcome = EndpointOutcome.FromError(errorInfo);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(errorInfo, httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status409Conflict));
            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/RES-001");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
            var errorCodeElement = deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.ErrorCode] as JsonElement?;

            errorCodeElement.Should().NotBeNull();
            errorCodeElement.Value.ValueKind.Should().Be(JsonValueKind.String);
            errorCodeElement.Value.GetString().Should().Be(errorCode);
        }

        [Fact]
        public async Task Map_Failure_DoesNotIncludeErrorCodeInExtensions_WhenNotConfigured()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");

            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            testOptions.ProblemDetails.IncludeErrorCodeInExtensions = false;
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var errorInfo = TestErrorInfoFactory.Conflict(code: "RES-001", message: "Resource conflict.");
            var outcome = EndpointOutcome.FromError(errorInfo);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(errorInfo, httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status409Conflict));
            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/RES-001");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().NotContainKey(ProblemDetailsConstants.Extensions.ErrorCode);
        }

        [Fact]
        public async Task Map_Failure_IncludesInnerErrorsInExtensions()
        {
            // Arrange
            _mockEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");
            var testOptions = TestOptionsFactory.CreateDefault().Value;
            testOptions.ProblemDetails.BaseTypeUri = new Uri("https://example.com/problems/");
            if (!testOptions.JsonSerializerOptions.Converters.OfType<JsonStringEnumConverter>().Any())
            {
                testOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            }
            var mapper = CreateMapper(testOptions, _mockEnvironment.Object);

            var innerError1 = TestErrorInfoFactory.Validation("fieldA", "Field A is invalid", "Detail for field A");
            var innerError2 = TestErrorInfoFactory.Custom(ErrorCategory.BusinessLogic, "ruleB", "Rule B violated");

            var errorInfo = new ErrorInfo(
                ErrorCategory.General,
                code: "CMP-001",
                message: "Composite error occurred with multiple underlying issues.",
                innerErrors: ImmutableList.Create(innerError1, innerError2));

            var outcome = EndpointOutcome.FromError(errorInfo);
            var (httpContext, responseStream) = HttpContextHelper.CreateHttpContext();

            _mockProblemDetailsMapper.Setup(m => m.Map(It.IsAny<ErrorInfo>(), httpContext))
                .ReturnsAsync(TestProblemDetailsFactory.CreateBase(StatusCodes.Status400BadRequest));
            _mockProblemTypeUriGenerator.Setup(g => g.Generate(It.IsAny<string>(), httpContext))
                .ReturnsAsync("https://example.com/problems/COMPOSITE_ERROR");

            // Act
            var result = await mapper.Map(outcome, httpContext);

            // Assert
            result.Should().BeOfType<ContentHttpResult>();
            var unwrappedResult = UnwrapResult<ContentHttpResult>(result);
            unwrappedResult.Should().BeOfType<ContentHttpResult>();
            unwrappedResult.Should().NotBeNull();

            var deserializedProblem = await HttpContextHelper.DeserializeProblemDetailsFromContentResultAsync(unwrappedResult!, httpContext, testOptions.JsonSerializerOptions);

            deserializedProblem!.Extensions.Should().ContainKey(ProblemDetailsConstants.Extensions.InnerErrors);
            var innerErrors = deserializedProblem.Extensions[ProblemDetailsConstants.Extensions.InnerErrors] as JsonElement?;
            innerErrors.Should().NotBeNull();
            innerErrors.Value.ValueKind.Should().Be(JsonValueKind.Array);
            innerErrors.Value.EnumerateArray().Should().HaveCount(2);

            var firstInner = innerErrors.Value.EnumerateArray().First();
            firstInner.GetProperty(JsonConstants.ErrorInfo.Category).GetString().Should().Be(innerError1.Category.ToString().ToUpperInvariant());
            firstInner.GetProperty(JsonConstants.ErrorInfo.Code).GetString().Should().Be(innerError1.Code);
            firstInner.GetProperty(JsonConstants.ErrorInfo.Message).GetString().Should().Be(innerError1.Message);
            firstInner.GetProperty(JsonConstants.ErrorInfo.Detail).GetString().Should().Be(innerError1.Detail);

            var secondInner = innerErrors.Value.EnumerateArray().Skip(1).First();
            secondInner.GetProperty(JsonConstants.ErrorInfo.Category).GetString().Should().Be(innerError2.Category.ToString().ToUpperInvariant());
            secondInner.GetProperty(JsonConstants.ErrorInfo.Code).GetString().Should().Be(innerError2.Code);
            secondInner.GetProperty(JsonConstants.ErrorInfo.Message).GetString().Should().Be(innerError2.Message);
            secondInner.GetProperty(JsonConstants.ErrorInfo.Detail).ValueKind.Should().Be(JsonValueKind.Null);
        }
    }
}
#pragma warning restore CS1591
