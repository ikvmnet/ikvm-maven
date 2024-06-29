using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.artifact;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="DefaultArtifact"/> to and from JSON.
    /// </summary>
    class DefaultArtifactJsonConverter : JsonConverter<DefaultArtifact>
    {

        public override DefaultArtifact Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var o = JsonDocument.ParseValue(ref reader).RootElement;
            if (o.ValueKind != JsonValueKind.Object)
                return null;

            var resolver = options.ReferenceHandler?.CreateResolver();
            if (resolver != null)
                if (o.TryGetProperty("$ref", out var refId))
                    return (DefaultArtifact)resolver.ResolveReference(refId.GetString());

            var artifact = new DefaultArtifact(
                o.TryGetProperty("groupId", out var groupId) ? groupId.GetString() : null,
                o.TryGetProperty("artifactId", out var artifactId) ? artifactId.GetString() : null,
                o.TryGetProperty("classifier", out var classifier) ? classifier.GetString() : null,
                o.TryGetProperty("extension", out var extension) ? extension.GetString() : null,
                o.TryGetProperty("version", out var version) ? version.GetString() : null,
                ReadProperties(o, ref reader, options),
                ReadFile(o, ref reader, options));

            if (resolver != null)
                if (o.TryGetProperty("$id", out var refId))
                    resolver.AddReference(refId.GetString(), artifact);

            return artifact;
        }

        java.util.Map ReadProperties(JsonElement json, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var m = new java.util.HashMap();
            var d = json.TryGetProperty("properties", out var properties) ? properties.Deserialize<Dictionary<string, string>>(options) : Enumerable.Empty<KeyValuePair<string, string>>();
            foreach (var kvp in d)
                m.put(kvp.Key, kvp.Value);

            return m;
        }

        java.io.File ReadFile(JsonElement json, ref Utf8JsonReader reader, JsonSerializerOptions serializer)
        {
            return json.TryGetProperty("file", out var f) && f.ValueKind == JsonValueKind.String ? new java.io.File(f.GetString()) : null;
        }

        public override void Write(Utf8JsonWriter writer, DefaultArtifact value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            var resolver = options.ReferenceHandler?.CreateResolver();
            if (resolver != null)
            {
                var refId = resolver.GetReference(value, out bool alreadyExists);
                if (alreadyExists)
                {
                    writer.WriteString("$ref", refId);
                    writer.WriteEndObject();
                    return;
                }
                else
                {
                    writer.WriteString("$id", refId);
                }
            }

            writer.WritePropertyName("groupId");
            writer.WriteStringValue(value.getGroupId());
            writer.WritePropertyName("artifactId");
            writer.WriteStringValue(value.getArtifactId());
            writer.WritePropertyName("classifier");
            writer.WriteStringValue(value.getClassifier());
            writer.WritePropertyName("extension");
            writer.WriteStringValue(value.getExtension());
            writer.WritePropertyName("version");
            writer.WriteStringValue(value.getVersion());
            writer.WritePropertyName("properties");
            WriteProperties(value, writer, options);
            writer.WritePropertyName("file");
            writer.WriteStringValue(value.getFile()?.toString());
            writer.WriteEndObject();
        }

        void WriteProperties(DefaultArtifact value, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (java.util.Map.Entry e in (IEnumerable)value.getProperties().entrySet())
            {
                writer.WritePropertyName((string)e.getKey());
                writer.WriteStringValue((string)e.getValue());
            }

            writer.WriteEndObject();
        }

    }

}
