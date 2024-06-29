using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.repository;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="RemoteRepository"/> to and from JSON.
    /// </summary>
    class RemoteRepositoryJsonConverter : JsonConverter<RemoteRepository>
    {

        public override RemoteRepository Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var o = JsonDocument.ParseValue(ref reader).RootElement;
            if (o.ValueKind != JsonValueKind.Object)
                return null;

            var resolver = options.ReferenceHandler?.CreateResolver();
            if (resolver != null)
                if (o.TryGetProperty("$ref", out var refId))
                    return (RemoteRepository)resolver.ResolveReference(refId.GetString());

            var repository = new RemoteRepository.Builder(o.GetProperty("id").GetString(), o.GetProperty("type").GetString(), o.GetProperty("url").GetString()).build();

            if (resolver != null)
                if (o.TryGetProperty("$id", out var refId))
                    resolver.AddReference(refId.GetString(), repository);

            return repository;
        }

        public override void Write(Utf8JsonWriter writer, RemoteRepository value, JsonSerializerOptions options)
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

            writer.WritePropertyName("id");
            writer.WriteStringValue(value.getId());
            writer.WritePropertyName("type");
            writer.WriteStringValue(value.getContentType());
            writer.WritePropertyName("url");
            writer.WriteStringValue(value.getUrl());
            writer.WriteEndObject();
        }

    }

}
