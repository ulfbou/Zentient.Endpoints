// <copyright file="EndpointsHttpOptions.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace Zentient.Endpoints.Http.Options
{
    /// <summary>
    /// Provides comprehensive options for configuring the behavior of Zentient.Endpoints.Http.
    /// This includes settings for global filters, Problem Details generation, success response
    /// serialization, and logging defaults.
    /// </summary>
    [SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily", Justification = "<Pending>")]
    public class EndpointsHttpOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the
        /// <see cref="Zentient.Endpoints.Http.Filters.NormalizeEndpointOutcomeFilter"/>
        /// should be automatically registered globally for all endpoints.
        /// <para>Defaults to <see langword="false" />. Set to <see langword="true" /> if you intend
        /// to explicitly apply the filter using
        /// <see cref="ServiceCollectionExtensions.WithNormalizeEndpointOutcomeFilter(Microsoft.AspNetCore.Builder.RouteHandlerBuilder)"/>
        /// on specific endpoints or groups.</para>
        /// </summary>
        /// <value><see langword="true" /> if the filter is registered globally; otherwise, <see langword="false" />.</value>
        public bool AddNormalizeEndpointOutcomeFilterGlobally { get; set; } = false;

        /// <summary>
        /// Gets or sets the default category name (logger name) used by internal loggers
        /// when an endpoint's display name is not available (e.g., for global filters).
        /// <para>Defaults to "Zentient.Endpoints.Http".</para>
        /// </summary>
        /// <value>The default logger category name used by internal loggers.</value>
        public string DefaultLoggerCategory { get; set; } = "Zentient.Endpoints.Http";

        /// <summary>
        /// Gets or sets options specifically for Problem Details (RFC 9457) generation.
        /// </summary>
        /// <value>
        /// The <see cref="ProblemDetailsOptions"/> for Problem Details type URIs, or <see langword="null"/> if not set.
        /// </value>
        public ProblemDetailsOptions ProblemDetails { get; set; } = new ProblemDetailsOptions();

        /// <summary>
        /// Gets or sets options specifically for successful API response serialization.
        /// </summary>
        /// <value>The <see cref="SuccessResponseOptions"/> for successful API response serialization.</value>
        public SuccessResponseOptions SuccessResponse { get; set; } = new SuccessResponseOptions();

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerOptions"/> used for serializing
        /// Problem Details and successful API responses.
        /// <para>
        /// Defaults to a pre-configured set (e.g., camelCase naming, ignore null values).
        /// </para>
        /// </summary>
        /// <remarks>
        /// This allows developers to control the JSON serialization behavior for
        /// all outbound responses managed by Zentient.Endpoints.Http, ensuring consistency
        /// with their overall API design.
        /// </remarks>
        /// <value>The <see cref="JsonSerializerOptions"/> used for serialization.</value>
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = CreateDefaultJsonSerializerOptions();

        /// <summary>
        /// Creates a deep copy of the current <see cref="EndpointsHttpOptions"/> instance.
        /// </summary>
        /// <returns>A new <see cref="EndpointsHttpOptions"/> instance with the same settings as the original.</returns>
        public EndpointsHttpOptions Clone()
        {
            return new EndpointsHttpOptions
            {
                AddNormalizeEndpointOutcomeFilterGlobally = this.AddNormalizeEndpointOutcomeFilterGlobally,
                DefaultLoggerCategory = this.DefaultLoggerCategory,
                ProblemDetails = this.ProblemDetails.Clone(),
                SuccessResponse = this.SuccessResponse.Clone(),
                JsonSerializerOptions = new JsonSerializerOptions(this.JsonSerializerOptions)
            };
        }

        /// <summary>
        /// Creates a default set of <see cref="JsonSerializerOptions"/> for API responses.
        /// </summary>
        /// <returns>A new <see cref="JsonSerializerOptions"/> instance.</returns>
        private static JsonSerializerOptions CreateDefaultJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
            };

            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            return options;
        }
    }
}
