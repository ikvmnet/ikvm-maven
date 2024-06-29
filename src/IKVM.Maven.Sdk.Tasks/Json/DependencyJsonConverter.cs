using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

using java.util;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="Dependency"/> to and from JSON.
    /// </summary>
    class DependencyJsonConverter : JsonConverter<Dependency>
    {

        public override Dependency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var o = JsonDocument.ParseValue(ref reader).RootElement;
            if (o.ValueKind != JsonValueKind.Object)
                return null;

            var resolver = options.ReferenceHandler?.CreateResolver();
            if (resolver != null)
                if (o.TryGetProperty("$ref", out var refId))
                    return (Dependency)resolver.ResolveReference(refId.GetString());

            var dependency = new Dependency(
                o.TryGetProperty("artifact", out var artifact) ? JsonSerializer.Deserialize<DefaultArtifact>(artifact, options) : null,
                o.TryGetProperty("scope", out var scope) ? scope.GetString() : null,
                ReadOptional(o, options),
                ReadExclusions(o, options));

            if (resolver != null)
                if (o.TryGetProperty("$id", out var refId))
                    resolver.AddReference(refId.GetString(), dependency);

            return dependency;
        }

        java.lang.Boolean ReadOptional(JsonElement o, JsonSerializerOptions options)
        {
            return (o.TryGetProperty("optional", out var optional) ? optional.GetBoolean() : (bool?)null) switch
            {
                true => java.lang.Boolean.TRUE,
                false => java.lang.Boolean.FALSE,
                _ => null,
            };
        }

        Collection ReadExclusions(JsonElement o, JsonSerializerOptions options)
        {
            var l = new java.util.ArrayList();
            if (o.TryGetProperty("exclusions", out var exclusions) && JsonSerializer.Deserialize<Exclusion[]>(exclusions, options) is Exclusion[] e)
                foreach (var i in e)
                    l.add(i);

            return l;
        }

        public override void Write(Utf8JsonWriter writer, Dependency value, JsonSerializerOptions options)
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

            writer.WritePropertyName("artifact");
            WriteArtifact(writer, value, options);
            writer.WritePropertyName("scope");
            WriteScope(writer, value, options);
            writer.WritePropertyName("optional");
            WriteOptional(writer, value, options);
            writer.WritePropertyName("exclusions");
            WriteExclusions(writer, value, options);
            writer.WriteEndObject();
        }

        void WriteArtifact(Utf8JsonWriter writer, Dependency dependency, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, dependency.getArtifact(), options);
        }

        void WriteScope(Utf8JsonWriter writer, Dependency dependency, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, dependency.getScope(), options);
        }

        void WriteOptional(Utf8JsonWriter writer, Dependency dependency, JsonSerializerOptions options)
        {
            var o = dependency.getOptional();
            if (o == null)
                writer.WriteNullValue();
            else if (o.booleanValue())
                writer.WriteBooleanValue(true);
            else
                writer.WriteBooleanValue(false);
        }

        void WriteExclusions(Utf8JsonWriter writer, Dependency dependency, JsonSerializerOptions options)
        {
            if (dependency.getExclusions() is not Collection c)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            foreach (Exclusion e in (IEnumerable)c)
                JsonSerializer.Serialize(writer, e, options);

            writer.WriteEndArray();
        }

    }

}
