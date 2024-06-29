using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.artifact;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="DefaultArtifact"/> to and from JSON.
    /// </summary>
    class ArtifactJsonConverter : JsonConverter<Artifact>
    {

        public override Artifact Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new Exception("Unknown artifact type during deserialization.");
        }

        public override void Write(Utf8JsonWriter writer, Artifact value, JsonSerializerOptions options)
        {
            if (value is DefaultArtifact a)
                JsonSerializer.Serialize(writer, a, options);
            else
                throw new Exception("Unknown artifact type during serialization.");
        }

    }

}
