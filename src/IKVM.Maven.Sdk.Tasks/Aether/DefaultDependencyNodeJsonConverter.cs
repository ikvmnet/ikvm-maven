using System;
using System.Collections;

using java.util;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using org.eclipse.aether.repository;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="DefaultDependencyNode"/> to and from JSON.
    /// </summary>
    class DefaultDependencyNodeJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DefaultDependencyNode);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JObject o)
                return null;

            DefaultDependencyNode n;
            if (o["dependency"] is JToken d)
                n = new DefaultDependencyNode(d.ToObject<Dependency>(serializer));
            else if (o["artifact"] is JToken a)
                n = new DefaultDependencyNode(a.ToObject<DefaultArtifact>(serializer));
            else
                n = new DefaultDependencyNode((Dependency)null);

            if (n == null)
                return null;

            ReadAliases(o, serializer, n);
            ReadChildren(o, serializer, n);
            ReadData(o, serializer, n);
            ReadManagedBits(o, serializer, n);
            ReadRepositories(o, serializer, n);
            ReadVersion(o, serializer, n);
            ReadVersionConstraint(o, serializer, n);

            return n;
        }

        void ReadAliases(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json["aliases"] is JArray a)
                foreach (var i in a.ToObject<DefaultArtifact[]>(serializer))
                    l.add(i);

            node.setAliases(l);
        }

        void ReadChildren(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json["children"] is JArray a)
                foreach (var i in a.ToObject<DefaultDependencyNode[]>(serializer))
                    l.add(i);

            node.setChildren(l);
        }

        void ReadData(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            if (json["data"] is not JArray a)
                return;

            foreach (var o in a)
                if (o is JObject i)
                    node.setData(i["key"].ToObject<object>(serializer), i["value"].ToObject<object>(serializer));
        }

        void ReadManagedBits(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            if (json["managedBits"] is not JValue v)
                return;

            node.setManagedBits((int)v);
        }

        void ReadRepositories(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            var l = new java.util.ArrayList();

            if (json["repositories"] is JArray a)
                foreach (var i in a.ToObject<RemoteRepository[]>(serializer))
                    l.add(i);

            node.setRepositories(l);
        }

        void ReadVersion(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            if (json["version"] is not JToken v)
                return;

            node.setVersion(v.ToObject<org.eclipse.aether.version.Version>(serializer));
        }

        void ReadVersionConstraint(JObject json, JsonSerializer serializer, DefaultDependencyNode node)
        {
            node.setVersionConstraint(json["versionConstraint"].ToObject<org.eclipse.aether.version.VersionConstraint>(serializer));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as DefaultDependencyNode;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            if (o.getDependency() != null)
            {
                writer.WritePropertyName("dependency");
                WriteDependency(writer, serializer, o);
            }
            else if (o.getArtifact() != null)
            {
                writer.WritePropertyName("artifact");
                WriteArtifact(writer, serializer, o);
            }

            writer.WritePropertyName("aliases");
            WriteAliases(writer, serializer, o);
            writer.WritePropertyName("children");
            WriteChildren(writer, serializer, o);
            writer.WritePropertyName("data");
            WriteData(writer, serializer, o);
            writer.WritePropertyName("managedBits");
            WriteManagedBits(writer, serializer, o);
            writer.WritePropertyName("repositories");
            WriteRepositories(writer, serializer, o);
            writer.WritePropertyName("version");
            WriteVersion(writer, serializer, o);
            writer.WritePropertyName("versionConstraint");
            WriteVersionConstraint(writer, serializer, o);
            writer.WriteEndObject();
        }

        void WriteArtifact(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            serializer.Serialize(writer, dependencyNode.getArtifact());
        }

        void WriteDependency(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            serializer.Serialize(writer, dependencyNode.getDependency());
        }

        void WriteAliases(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (Artifact a in (IEnumerable)dependencyNode.getAliases())
                serializer.Serialize(writer, a);

            writer.WriteEndArray();
        }

        void WriteChildren(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (DependencyNode n in (IEnumerable)dependencyNode.getChildren())
                serializer.Serialize(writer, n);

            writer.WriteEndArray();
        }

        void WriteData(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (Map.Entry n in (IEnumerable)dependencyNode.getData().entrySet())
            {
                writer.WriteStartObject();
                writer.WritePropertyName("key");
                serializer.Serialize(writer, n.getKey());
                writer.WritePropertyName("value");
                serializer.Serialize(writer, n.getValue());
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        void WriteManagedBits(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            serializer.Serialize(writer, dependencyNode.getManagedBits());
        }

        void WriteRepositories(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            writer.WriteStartArray();

            foreach (RemoteRepository r in (IEnumerable)dependencyNode.getRepositories())
                serializer.Serialize(writer, r);

            writer.WriteEndArray();
        }

        void WriteVersion(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            serializer.Serialize(writer, dependencyNode.getVersion());
        }

        void WriteVersionConstraint(JsonWriter writer, JsonSerializer serializer, DefaultDependencyNode dependencyNode)
        {
            serializer.Serialize(writer, dependencyNode.getVersionConstraint());
        }

    }

}
