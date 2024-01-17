using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.util.version;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="org.eclipse.aether.version.Version"/> to and from JSON.
    /// </summary>
    class VersionJsonConverter : JsonConverter<org.eclipse.aether.version.Version>
    {

        static readonly MethodInfo parseVersionMethod = typeof(GenericVersionScheme)
            .GetMethods()
            .Where(i => i.Name == "parseVersion")
            .Where(i => i.GetParameters().Length == 1 && i.GetParameters()[0].ParameterType == typeof(string))
            .Where(i => i.ReturnType == typeof(GenericVersionScheme).Assembly.GetType("org.eclipse.aether.util.version.GenericVersion"))
            .First();

        public override org.eclipse.aether.version.Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;
            else
                return (org.eclipse.aether.version.Version)parseVersionMethod.Invoke(new GenericVersionScheme(), new[] { reader.GetString() });
        }

        public override void Write(Utf8JsonWriter writer, org.eclipse.aether.version.Version value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.toString());
        }

    }

}
