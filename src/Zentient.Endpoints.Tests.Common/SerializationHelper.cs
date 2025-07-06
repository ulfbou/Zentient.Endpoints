// <copyright file="SerializationHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using System;
using System.Text.Json;

namespace Zentient.Endpoints.Tests.Common
{
    /// <summary>
    /// Provides helper methods for serializing and deserializing objects to and from JSON using configured options.
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// Serializes an object to a JSON string using the provided options.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use for serialization.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeToJson<T>(T obj, JsonSerializerOptions jsonSerializerOptions) =>
            JsonSerializer.Serialize(obj, jsonSerializerOptions);

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type using the provided options.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
        /// <returns>The deserialized object, or <c>null</c> if the JSON is invalid or cannot be deserialized to the specified type.</returns>
        public static T? DeserializeFromJson<T>(string json, JsonSerializerOptions jsonSerializerOptions) =>
            JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }
}
