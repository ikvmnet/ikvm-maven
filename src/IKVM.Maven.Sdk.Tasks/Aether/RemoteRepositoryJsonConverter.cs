using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.repository;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="RemoteRepository"/> to and from JSON.
    /// </summary>
    class RemoteRepositoryJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RemoteRepository);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JToken.ReadFrom(reader) as JObject;
            if (o == null)
                return null;

            return new RemoteRepository.Builder(
                o.Value<string>("id"),
                o.Value<string>("type"),
                o.Value<string>("url")
            ).build();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as RemoteRepository;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(o.getId());
            writer.WritePropertyName("type");
            writer.WriteValue(o.getContentType());
            writer.WritePropertyName("url");
            writer.WriteValue(o.getUrl());
            writer.WriteEndObject();
        }

    }

}
