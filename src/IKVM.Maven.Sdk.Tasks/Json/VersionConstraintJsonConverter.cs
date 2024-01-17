using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using org.eclipse.aether.util.version;

namespace IKVM.Maven.Sdk.Tasks.Json
{

    /// <summary>
    /// Serializes a <see cref="org.eclipse.aether.version.VersionConstraint"/> to and from JSON.
    /// </summary>
    class VersionConstraintJsonConverter : JsonConverter<org.eclipse.aether.version.VersionConstraint>
    {

        static readonly MethodInfo parseVersionConstraintMethod = typeof(GenericVersionScheme)
            .GetMethods()
            .Where(i => i.Name == "parseVersionConstraint")
            .Where(i => i.GetParameters().Length == 1 && i.GetParameters()[0].ParameterType == typeof(string))
            .Where(i => i.ReturnType == typeof(GenericVersionScheme).Assembly.GetType("org.eclipse.aether.util.version.GenericVersionConstraint"))
            .First();

        public override org.eclipse.aether.version.VersionConstraint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;
            else
                return (org.eclipse.aether.version.VersionConstraint)parseVersionConstraintMethod.Invoke(new GenericVersionScheme(), new[] { reader.GetString() });
        }

        public override void Write(Utf8JsonWriter writer, org.eclipse.aether.version.VersionConstraint value, JsonSerializerOptions options)
        {
            // type must be a VersionConstrant
            if (value is not org.eclipse.aether.version.VersionConstraint o)
            {
                writer.WriteNullValue();
                return;
            }

            // constraint can be a Version
            if (o.getVersion() is org.eclipse.aether.version.Version c)
            {
                writer.WriteStringValue(c.toString());
                return;
            }

            // constraint can be a VersionRange
            if (o.getRange() is org.eclipse.aether.version.VersionRange r)
            {
                var s = new StringBuilder();

                var l = r.getLowerBound();
                var u = r.getUpperBound();

                if (l != null)
                    s.Append(l.isInclusive() ? "[" : "(").Append(l.getVersion().toString());
                else
                    s.Append("(");

                s.Append(",");

                if (u != null)
                    s.Append(u.getVersion().toString()).Append(u.isInclusive() ? "]" : ")");
                else
                    s.Append(")");

                writer.WriteStringValue(s.ToString());
                return;
            }

            // fallback
            writer.WriteNullValue();
        }

    }

}
