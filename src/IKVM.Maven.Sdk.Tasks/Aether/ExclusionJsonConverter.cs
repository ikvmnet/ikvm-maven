using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="Exclusion"/> to and from JSON.
    /// </summary>
    class ExclusionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Exclusion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JObject o)
                return null;
            else
                return new Exclusion(o["groupId"].Value<string>(), o["artifactId"].Value<string>(), o["classifier"].Value<string>(), o["extension"].Value<string>());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as Exclusion;
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
            writer.WriteEndObject();
        }

    }

}
