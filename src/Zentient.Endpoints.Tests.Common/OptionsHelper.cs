// <copyright file="OptionsHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides helper methods and properties for configuring <see cref="JsonSerializerOptions"/>
    /// used in Zentient.Results and Zentient.Endpoints tests.
    /// </summary>
    internal static class OptionsHelper
    {
        /// <summary>
        /// Gets default <see cref="JsonSerializerOptions"/> configured for Zentient.Results and Endpoints.
        /// </summary>
        /// <value>
        /// A <see cref="JsonSerializerOptions"/> instance with camel case property naming, indented output,
        /// and null value ignoring enabled.
        /// </value>
        public static JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        // Add any custom converters needed for Zentient.Results/Endpoints here
        // options.Converters.Add(new Zentient.Results.Serialization.ResultJsonConverter());
        // options.Converters.Add(new Zentient.Endpoints.Serialization.UnitJsonConverter());
    }
}