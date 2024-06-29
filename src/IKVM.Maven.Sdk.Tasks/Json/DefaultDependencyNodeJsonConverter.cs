using System;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

using java.util;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using org.eclipse.aether.repository;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="DefaultDependencyNode"/> to and from JSON.
    /// </summary>
    class DefaultDependencyNodeJsonConverter : JsonConverter<DefaultDependencyNode>
    {

        public override DefaultDependencyNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var o = JsonDocument.ParseValue(ref reader).RootElement;
            if (o.ValueKind != JsonValueKind.Object)
                return null;

            var resolver = options.ReferenceHandler?.CreateResolver();
            if (resolver != null)
                if (o.TryGetProperty("$ref", out var refId))
                    return (DefaultDependencyNode)resolver.ResolveReference(refId.GetString());

            DefaultDependencyNode node;
            if (o.TryGetProperty("dependency", out var dependency))
                node = new DefaultDependencyNode(JsonSerializer.Deserialize<Dependency>(dependency, options));
            else if (o.TryGetProperty("artifact", out var artifact))
                node = new DefaultDependencyNode(JsonSerializer.Deserialize<DefaultArtifact>(artifact, options));
            else
                node = new DefaultDependencyNode((Dependency)null);

            if (node == null)
                return null;

            if (resolver != null)
                if (o.TryGetProperty("$id", out var refId))
                    resolver.AddReference(refId.GetString(), node);

            ReadAliases(o, options, node);
            ReadChildren(o, options, node);
            ReadData(o, options, node);
            ReadManagedBits(o, options, node);
            ReadRepositories(o, options, node);
            ReadVersion(o, options, node);
            ReadVersionConstraint(o, options, node);

            return node;
        }

        void ReadAliases(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json.TryGetProperty("aliases", out var aliases) && aliases.ValueKind == JsonValueKind.Array)
                foreach (var i in JsonSerializer.Deserialize<DefaultArtifact[]>(aliases, options))
                    l.add(i);

            node.setAliases(l);
        }

        void ReadChildren(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
                foreach (var i in JsonSerializer.Deserialize<DefaultDependencyNode[]>(children, options))
                    l.add(i);

            node.setChildren(l);
        }

        void ReadData(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            if (json.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                foreach (var o in data.EnumerateArray())
                    ReadDataItem(json, options, node, o);
        }

        void ReadDataItem(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node, JsonElement item)
        {
            var key = item.TryGetProperty("key", out var k) ? JsonSerializer.Deserialize<object>(k, options) : null;
            var valueType = item.TryGetProperty("valueType", out var t) ? t.GetString() : null;
            var value = item.TryGetProperty("value", out var v) ? v : (JsonElement?)null;
            node.setData(key, ReadDataValue(options, valueType, value));
        }

        object ReadDataValue(JsonSerializerOptions options, string valueType, JsonElement? value)
        {
            if (valueType == "dependencyNode")
                return JsonSerializer.Deserialize<DefaultDependencyNode>(value.Value, options);
            else
                return JsonSerializer.Deserialize<object>(value.Value, options);
        }

        void ReadManagedBits(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            if (json.TryGetProperty("managedBits", out var b) && b.ValueKind == JsonValueKind.Number)
                node.setManagedBits(b.GetInt32());
        }

        void ReadRepositories(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json.TryGetProperty("repositories", out var repositories) && repositories.ValueKind == JsonValueKind.Array)
                foreach (var i in JsonSerializer.Deserialize<RemoteRepository[]>(repositories, options))
                    l.add(i);

            node.setRepositories(l);
        }

        void ReadVersion(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            if (json.TryGetProperty("version", out var version))
                node.setVersion(JsonSerializer.Deserialize<org.eclipse.aether.version.Version>(version, options));
        }

        void ReadVersionConstraint(JsonElement json, JsonSerializerOptions options, DefaultDependencyNode node)
        {
            if (json.TryGetProperty("versionConstraint", out var versionConstraint))
                node.setVersionConstraint(JsonSerializer.Deserialize<org.eclipse.aether.version.VersionConstraint>(versionConstraint, options));
        }

        public override void Write(Utf8JsonWriter writer, DefaultDependencyNode value, JsonSerializerOptions options)
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

            if (value.getDependency() != null)
            {
                writer.WritePropertyName("dependency");
                WriteDependency(writer, options, value);
            }
            else if (value.getArtifact() != null)
            {
                writer.WritePropertyName("artifact");
                WriteArtifact(writer, options, value);
            }

            writer.WritePropertyName("aliases");
            WriteAliases(writer, options, value);
            writer.WritePropertyName("children");
            WriteChildren(writer, options, value);
            writer.WritePropertyName("data");
            WriteData(writer, options, value);
            writer.WritePropertyName("managedBits");
            WriteManagedBits(writer, options, value);
            writer.WritePropertyName("repositories");
            WriteRepositories(writer, options, value);
            writer.WritePropertyName("version");
            WriteVersion(writer, options, value);
            writer.WritePropertyName("versionConstraint");
            WriteVersionConstraint(writer, options, value);
            writer.WriteEndObject();
        }

        void WriteArtifact(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            JsonSerializer.Serialize(writer, dependencyNode.getArtifact(), options);
        }

        void WriteDependency(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            JsonSerializer.Serialize(writer, dependencyNode.getDependency(), options);
        }

        void WriteAliases(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (Artifact a in (IEnumerable)dependencyNode.getAliases())
                JsonSerializer.Serialize(writer, a, options);

            writer.WriteEndArray();
        }

        void WriteChildren(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (DependencyNode n in (IEnumerable)dependencyNode.getChildren())
                JsonSerializer.Serialize(writer, n, options);

            writer.WriteEndArray();
        }

        void WriteData(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (Map.Entry n in (IEnumerable)dependencyNode.getData().entrySet())
            {
                writer.WriteStartObject();
                writer.WritePropertyName("key");
                JsonSerializer.Serialize(writer, n.getKey(), options);

                if (n.getValue() is DefaultDependencyNode)
                {
                    writer.WritePropertyName("valueType");
                    writer.WriteStringValue("dependencyNode");

                    writer.WritePropertyName("value");
                    JsonSerializer.Serialize(writer, n.getValue(), options);
                }
                else
                {
                    writer.WritePropertyName("value");
                    JsonSerializer.Serialize(writer, n.getValue(), options);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        void WriteManagedBits(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            JsonSerializer.Serialize(writer, dependencyNode.getManagedBits(), options);
        }

        void WriteRepositories(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (RemoteRepository r in (IEnumerable)dependencyNode.getRepositories())
                JsonSerializer.Serialize(writer, r, options);

            writer.WriteEndArray();
        }

        void WriteVersion(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            JsonSerializer.Serialize(writer, dependencyNode.getVersion(), options);
        }

        void WriteVersionConstraint(Utf8JsonWriter writer, JsonSerializerOptions options, DefaultDependencyNode dependencyNode)
        {
            JsonSerializer.Serialize(writer, dependencyNode.getVersionConstraint(), options);
        }

    }

}
