using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="DependencyNode"/> to and from JSON.
    /// </summary>
    class DependencyNodeJsonConverter : JsonConverter<DependencyNode>
    {

        public override DependencyNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new Exception("Unknown dependency node type during deserialization.");
        }

        public override void Write(Utf8JsonWriter writer, DependencyNode value, JsonSerializerOptions options)
        {
            if (value is DefaultDependencyNode n)
                JsonSerializer.Serialize(writer, n, options);
            else
                throw new Exception("Unknown dependency node type during serialization.");
        }

    }

}
