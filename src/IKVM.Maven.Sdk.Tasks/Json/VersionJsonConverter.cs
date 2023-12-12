using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.util.version;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="org.eclipse.aether.version.Version"/> to and from JSON.
    /// </summary>
    class VersionJsonConverter : JsonConverter<org.eclipse.aether.version.Version>
    {

        public override org.eclipse.aether.version.Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;
            else
                return new GenericVersionScheme().parseVersion(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, org.eclipse.aether.version.Version value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.toString());
        }

    }

}
