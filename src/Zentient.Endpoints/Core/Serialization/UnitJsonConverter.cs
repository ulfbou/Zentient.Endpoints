// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitJsonConverter.cs" company="Zentient Framework Team">
// Copyright © 2025 Zentient Framework Team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Zentient.Endpoints.Core;

namespace Zentient.Endpoints.Core.Serialization
{
    /// <summary>A JSON converter for the <see cref="Unit"/> type.</summary>
    public class UnitJsonConverter : JsonConverter<Unit>
    {
        /// <inheritdoc />
        public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected end of object for Unit.");
                }
            }
            else if (reader.TokenType != JsonTokenType.Null)
            {
                throw new JsonException("Expected object or null for Unit.");
            }

            return Unit.Value;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
        {
            ArgumentNullException.ThrowIfNull(writer, nameof(writer));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            writer.WriteStartObject();
            writer.WriteEndObject();
        }
    }
}
