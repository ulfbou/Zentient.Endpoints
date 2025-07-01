// <copyright file="SerializationHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Zentient.Endpoints.Tests.Shared
{
    /// <summary>
    /// Provides helper methods for serializing and deserializing objects to and from JSON using configured options.
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes an object to a JSON string using configured options.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions"/> to customize serialization. This parameter is currently ignored and <see cref="OptionsHelper.JsonSerializerOptions"/> is always used.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeToJson<T>(T obj, JsonSerializerOptions jsonSerializerOptions)
        {
            return JsonSerializer.Serialize(obj, OptionsHelper.JsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type using configured options.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="jsonSerializerOptions">Optional <see cref="JsonSerializerOptions"/> to customize deserialization. This parameter is currently ignored and <see cref="OptionsHelper.JsonSerializerOptions"/> is always used.</param>
        /// <returns>The deserialized object, or <c>null</c> if the JSON is invalid or cannot be deserialized to the specified type.</returns>
        public static T? DeserializeFromJson<T>(string json, JsonSerializerOptions jsonSerializerOptions)
        {
            return JsonSerializer.Deserialize<T>(json, OptionsHelper.JsonSerializerOptions);
        }
    }
}
