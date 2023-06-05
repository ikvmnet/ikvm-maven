using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes various standard Java types to/from JSON.
    /// </summary>
    class BooleanConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(java.lang.Boolean);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JValue v || v.Type != JTokenType.Boolean)
                return null;
            else
                return java.lang.Boolean.valueOf((bool)v);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as java.lang.Boolean;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(o.booleanValue());
        }

    }

}
