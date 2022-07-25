using System;
using System.Collections;

using java.util;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="Dependency"/> to and from JSON.
    /// </summary>
    class DependencyJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dependency);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JObject o)
                return null;

            return new Dependency(
                o["artifact"]?.ToObject<DefaultArtifact>(serializer),
                o.Value<string>("scope"),
                ReadOptional(serializer, o),
                ReadExclusions(o, serializer));
        }

        java.lang.Boolean ReadOptional(JsonSerializer serializer, JObject o)
        {
            return o?.Value<bool?>("optional") switch
            {
                true => java.lang.Boolean.TRUE,
                false => java.lang.Boolean.FALSE,
                _ => null,
            };
        }

        Collection ReadExclusions(JObject json, JsonSerializer serializer)
        {
            var l = new java.util.ArrayList();
            foreach (var i in json["exclusions"].ToObject<Exclusion[]>(serializer))
                l.add(i);

            return l;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as Dependency;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("artifact");
            WriteArtifact(writer, serializer, o);
            writer.WritePropertyName("scope");
            WriteScope(writer, serializer, o);
            writer.WritePropertyName("optional");
            WriteOptional(writer, serializer, o);
            writer.WritePropertyName("exclusions");
            WriteExclusions(writer, serializer, o);
            writer.WriteEndObject();
        }

        void WriteArtifact(JsonWriter writer, JsonSerializer serializer, Dependency dependency)
        {
            serializer.Serialize(writer, dependency.getArtifact());
        }

        void WriteScope(JsonWriter writer, JsonSerializer serializer, Dependency dependency)
        {
            serializer.Serialize(writer, dependency.getScope());
        }

        void WriteOptional(JsonWriter writer, JsonSerializer serializer, Dependency dependency)
        {
            var o = dependency.getOptional();
            if (o == null)
                writer.WriteNull();
            else if (o.booleanValue())
                writer.WriteValue(true);
            else
                writer.WriteValue(false);
        }

        void WriteExclusions(JsonWriter writer, JsonSerializer serializer, Dependency dependency)
        {
            if (dependency.getExclusions() is not java.util.Collection c)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();

            foreach (Exclusion e in (IEnumerable)c)
                serializer.Serialize(writer, e);

            writer.WriteEndArray();
        }

    }

}
