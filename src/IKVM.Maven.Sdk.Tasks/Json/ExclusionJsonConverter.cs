using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="Exclusion"/> to and from JSON.
    /// </summary>
    class ExclusionJsonConverter : JsonConverter<Exclusion>
    {

        public override Exclusion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var o = JsonDocument.ParseValue(ref reader).RootElement;
            if (o.ValueKind != JsonValueKind.Object)
                return null;
            else
                return new Exclusion(o.GetProperty("groupId").GetString(), o.GetProperty("artifactId").GetString(), o.GetProperty("classifier").GetString(), o.GetProperty("extension").GetString());
        }

        public override void Write(Utf8JsonWriter writer, Exclusion value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("groupId");
            writer.WriteStringValue(value.getGroupId());
            writer.WritePropertyName("artifactId");
            writer.WriteStringValue(value.getArtifactId());
            writer.WritePropertyName("classifier");
            writer.WriteStringValue(value.getClassifier());
            writer.WritePropertyName("extension");
            writer.WriteStringValue(value.getExtension());
            writer.WriteEndObject();
        }

    }

}
