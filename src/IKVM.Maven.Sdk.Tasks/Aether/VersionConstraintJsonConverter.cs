using System;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using org.eclipse.aether.util.version;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    /// <summary>
    /// Serializes a <see cref="org.eclipse.aether.version.VersionConstraint"/> to and from JSON.
    /// </summary>
    class VersionConstraintJsonConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return typeof(org.eclipse.aether.version.VersionConstraint).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (JToken.ReadFrom(reader) is not JValue v || v.Type != JTokenType.String)
                return null;
            else
                return new GenericVersionScheme().parseVersionConstraint((string)v);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var o = value as org.eclipse.aether.version.VersionConstraint;
            if (o == null)
            {
                writer.WriteNull();
                return;
            }

            if (o.getVersion() is org.eclipse.aether.version.Version c)
                writer.WriteValue(c.toString());
            else if (o.getRange() is org.eclipse.aether.version.VersionRange r)
            {
                var l = r.getLowerBound();
                var u = r.getUpperBound();
                var s = new StringBuilder();
                if (l != null)
                    s.Append(l.isInclusive() ? "[" : "(").Append(l.getVersion().toString());
                if (l != null || r != null)
                    s.Append(",");
                if (r != null)
                    s.Append(u.getVersion().toString()).Append(u.isInclusive() ? "]" : ")");

                writer.WriteValue(s.ToString());
            }
            else
                writer.WriteNull();
        }

    }

}
