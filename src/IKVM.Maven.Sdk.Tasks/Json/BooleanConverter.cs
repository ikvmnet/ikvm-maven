using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes various standard Java types to/from JSON.
    /// </summary>
    class BooleanConverter : JsonConverter<java.lang.Boolean>
    {
        public override java.lang.Boolean Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.True && reader.TokenType != JsonTokenType.False)
                return null;
            else
                return java.lang.Boolean.valueOf(reader.GetBoolean());
        }

        public override void Write(Utf8JsonWriter writer, java.lang.Boolean value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteBooleanValue(value.booleanValue());
        }

    }

}
