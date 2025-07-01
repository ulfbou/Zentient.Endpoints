// <copyright file="EndpointsHttpOptionsValidator.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Zentient.Endpoints.Http.Mapping;
using Zentient.Endpoints.Http.Options;

namespace Zentient.Endpoints.Http.Validation
{
    /// <summary>
    /// Validates <see cref="EndpointsHttpOptions"/> and its nested options to ensure semantic correctness.
    /// </summary>
    internal sealed class EndpointsHttpOptionsValidator : IValidateOptions<EndpointsHttpOptions>
    {
        /// <summary>
        /// Validates the provided <see cref="EndpointsHttpOptions"/> instance.
        /// </summary>
        /// <param name="name">The name of the options instance being validated, or <see langword="null"/> for unnamed options.</param>
        /// <param name="options">The <see cref="EndpointsHttpOptions"/> instance to validate.</param>
        /// <returns>
        /// A <see cref="ValidateOptionsResult"/> indicating whether the options are valid.
        /// If valid, returns <see cref="ValidateOptionsResult.Success"/>.
        /// If invalid, returns <c>ValidateOptionsResult.Fail(IEnumerable&lt;string&gt;)</c> with a list of error messages.
        /// </returns>
        public ValidateOptionsResult Validate(string? name, EndpointsHttpOptions options)
        {
            var failures = new List<string>();

            if (options.ProblemDetails.BaseTypeUri is { } uri)
            {
                if (!uri.IsAbsoluteUri)
                {
                    failures.Add("ProblemDetails.BaseTypeUri must be an absolute URI.");
                }

                if (!uri.AbsoluteUri.EndsWith('/'))
                {
                    failures.Add("ProblemDetails.BaseTypeUri should end with a '/' to allow proper concatenation with error codes.");
                }
            }

            foreach (var (category, statusCode) in options.ProblemDetails.CategoryToStatusCodeMap)
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    failures.Add("ProblemDetails.CategoryToStatusCodeMap contains a null or empty category key.");
                }

                if (statusCode < 100 || statusCode > 599)
                {
                    failures.Add($"ProblemDetails.CategoryToStatusCodeMap[{category}] has an invalid HTTP status code: {statusCode}.");
                }
            }

            if (options.SuccessResponse.DefaultOkStatusCode < 200 || options.SuccessResponse.DefaultOkStatusCode > 299)
            {
                failures.Add("SuccessResponse.DefaultOkStatusCode must be a 2xx status code (200–299).");
            }

            if (options.SuccessResponse.DefaultNoContentStatusCode != 204)
            {
                failures.Add("SuccessResponse.DefaultNoContentStatusCode must be 204 (No Content) as per HTTP specification.");
            }

            return failures.Count == 0
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(failures);
        }
    }
}
