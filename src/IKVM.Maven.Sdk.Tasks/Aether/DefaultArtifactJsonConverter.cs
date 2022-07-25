using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.artifact;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="DefaultArtifact"/> to and from JSON.
    /// </summary>
    class DefaultArtifactJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DefaultArtifact);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JToken.ReadFrom(reader) as JObject;
            if (o == null)
                return null;

            return new DefaultArtifact(
                serializer.Deserialize<string>(o["groupId"].CreateReader()),
                serializer.Deserialize<string>(o["artifactId"].CreateReader()),
                serializer.Deserialize<string>(o["classifier"].CreateReader()),
                serializer.Deserialize<string>(o["extension"].CreateReader()),
                serializer.Deserialize<string>(o["version"].CreateReader()),
                ReadProperties(o, reader, serializer),
                ReadFile(o, reader, serializer));
        }

        java.util.Map ReadProperties(JObject json, JsonReader reader, JsonSerializer serializer)
        {
            var m = new java.util.HashMap();
            foreach (var kvp in json["properties"].ToObject<Dictionary<string, string>>(serializer))
                m.put(kvp.Key, kvp.Value);

            return m;
        }

        java.io.File ReadFile(JObject json, JsonReader reader, JsonSerializer serializer)
        {
            return json.Value<string>("file") is string f ? new java.io.File(f) : null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as DefaultArtifact;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("groupId");
            writer.WriteValue(o.getGroupId());
            writer.WritePropertyName("artifactId");
            writer.WriteValue(o.getArtifactId());
            writer.WritePropertyName("classifier");
            writer.WriteValue(o.getClassifier());
            writer.WritePropertyName("extension");
            writer.WriteValue(o.getExtension());
            writer.WritePropertyName("version");
            writer.WriteValue(o.getVersion());
            writer.WritePropertyName("properties");
            WriteProperties(o, writer, serializer);
            writer.WritePropertyName("file");
            writer.WriteValue(o.getFile()?.toString());
            writer.WriteEndObject();
        }

        void WriteProperties(DefaultArtifact o, JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (java.util.Map.Entry e in (IEnumerable)o.getProperties().entrySet())
            {
                writer.WritePropertyName((string)e.getKey());
                writer.WriteValue((string)e.getValue());
            }

            writer.WriteEndObject();
        }

    }

}
