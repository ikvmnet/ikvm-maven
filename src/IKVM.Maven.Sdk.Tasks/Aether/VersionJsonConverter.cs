using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.util.version;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="org.eclipse.aether.version.Version"/> to and from JSON.
    /// </summary>
    class VersionJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return typeof(org.eclipse.aether.version.Version).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JValue v || v.Type != JTokenType.String)
                return null;
            else
                return new GenericVersionScheme().parseVersion((string)v);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as org.eclipse.aether.version.Version;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(o.toString());
        }

    }

}
