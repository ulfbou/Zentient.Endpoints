// <copyright file="SerializationHelper.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;

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

        /// <summary>
        /// Deserializes the value from a <see cref="JsonHttpResult{Object}"/> to an object of type <typeparamref name="TExpected"/>.
        /// </summary>
        /// <typeparam name="TExpected">The type to deserialize to.</typeparam>
        /// <param name="result">The <see cref="JsonHttpResult{Object}"/> containing the value to deserialize.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="TExpected"/>, or <c>default</c> if <paramref name="result"/> or its value is <c>null</c>.
        /// </returns>
        public static TExpected? DeserializeFromJsonHttpResult<TExpected>(JsonHttpResult<object> result)
            => (TExpected?)result?.Value;

        /// <summary>
        /// Deserializes the value from a <see cref="JsonHttpResult{TValue}"/> to an object of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize to.</typeparam>
        /// <param name="result">The <see cref="JsonHttpResult{TValue}"/> containing the value to deserialize.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="TValue"/>, or <c>default</c> if <paramref name="result"/> or its value is <c>null</c>.
        /// </returns>
        public static TValue? DeserializeFromJsonHttpResult<TValue>(JsonHttpResult<TValue> result)
            => result.Value;

        /// <summary>
        /// Deserializes the value from a <see cref="JsonHttpResult{TValue}"/> to an object of type <typeparamref name="TValue"/> using the provided <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="TValue">The type to deserialize to.</typeparam>
        /// <param name="result">The <see cref="JsonHttpResult{TValue}"/> containing the value to deserialize.</param>
        /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use for serialization and deserialization.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="TValue"/>, or <c>default</c> if <paramref name="result"/> or its value is <c>null</c>.
        /// </returns>
        public static TValue? DeserializeFromJsonHttpResult<TValue>(JsonHttpResult<TValue> result, JsonSerializerOptions jsonSerializerOptions)
        {
            if (result == null || result.Value == null)
            {
                return default;
            }

            var json = JsonSerializer.Serialize(result.Value, jsonSerializerOptions);
            return JsonSerializer.Deserialize<TValue>(json, jsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes the value from a <see cref="JsonHttpResult{Object}"/> to an object of type <typeparamref name="TExpected"/> using the provided <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <typeparam name="TExpected">The type to deserialize to.</typeparam>
        /// <param name="result">The <see cref="JsonHttpResult{Object}"/> containing the value to deserialize.</param>
        /// <param name="jsonSerializerOptions">The <see cref="JsonSerializerOptions"/> to use for serialization and deserialization.</param>
        /// <returns>
        /// The deserialized object of type <typeparamref name="TExpected"/>, or <c>default</c> if <paramref name="result"/> or its value is <c>null</c>.
        /// </returns>
        public static TExpected? DeserializeFromJsonHttpResult<TExpected>(JsonHttpResult<object> result, JsonSerializerOptions jsonSerializerOptions)
        {
            if (result == null || result.Value == null)
            {
                return default;
            }

            var json = JsonSerializer.Serialize(result.Value, jsonSerializerOptions);
            return JsonSerializer.Deserialize<TExpected>(json, jsonSerializerOptions);
        }
    }
}
