using System;
using System.Linq;
using System.Text;

using org.eclipse.aether.artifact;
using org.eclipse.aether.util.artifact;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Describes a dependency declared upon a <see cref="MavenReferenceItem"/>, or upon an artifact within the
    /// dependency tree of a <see cref="MavenReferenceItem"/>, addressed by <see cref="Path"/>.
    /// </summary>
    public class MavenReferenceItemDependency : IEquatable<MavenReferenceItemDependency>
    {

        /// <summary>
        /// Parses a compressed dependency string. Path segments are separated by '/', with each segment in the form
        /// 'groupId:artifactId[:version]'. The final segment is an artifact coordinate in the form
        /// 'groupId:artifactId[:extension[:classifier]]:version', optionally followed by comma-separated 'key=value'
        /// qualifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        public static MavenReferenceItemDependency Parse(string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var segments = value.Split('/').Select(i => i.Trim()).ToArray();
            if (segments.Any(string.IsNullOrEmpty))
                throw new MavenTaskException($"Invalid dependency specification '{value}'.");

            // leading segments form the path to the target artifact
            var path = new MavenReferenceItemDependencySelector[segments.Length - 1];
            for (int i = 0; i < segments.Length - 1; i++)
            {
                var a = segments[i].Split(':');
                if (a.Length is 2 or 3)
                    path[i] = new MavenReferenceItemDependencySelector(a[0], a[1], a.Length >= 3 ? a[2] : null);
                else
                    throw new MavenTaskException($"Invalid dependency path segment '{segments[i]}' in '{value}'.");
            }

            // final segment is an artifact coordinate, optionally followed by comma-separated qualifiers
            var tokens = segments[segments.Length - 1].Split(',').Select(i => i.Trim()).ToArray();
            var coords = tokens[0];

            var item = new MavenReferenceItemDependency();
            item.Path = path;

            try
            {
                var artifact = new DefaultArtifact(coords);
                item.GroupId = artifact.getGroupId();
                item.ArtifactId = artifact.getArtifactId();
                item.Extension = artifact.getExtension();
                item.Classifier = artifact.getClassifier();
                item.Version = artifact.getVersion();
            }
            catch (java.lang.IllegalArgumentException e)
            {
                throw new MavenTaskException($"Invalid dependency coordinate '{coords}' in '{value}': {e.getMessage()}");
            }

            // remaining tokens assign the non-coordinate attributes of the dependency
            foreach (var pair in tokens.Skip(1))
            {
                var kv = pair.Split(new[] { '=' }, 2);
                if (kv.Length != 2)
                    throw new MavenTaskException($"Invalid dependency qualifier '{pair}' in '{value}'.");

                var key = kv[0].Trim();
                var val = kv[1].Trim();
                switch (key)
                {
                    case "scope":
                        item.Scope = val;
                        break;
                    case "optional":
                        if (string.Equals(val, "true", StringComparison.OrdinalIgnoreCase))
                            item.Optional = true;
                        else if (string.Equals(val, "false", StringComparison.OrdinalIgnoreCase))
                            item.Optional = false;
                        else
                            throw new MavenTaskException($"Invalid dependency qualifier '{pair}' in '{value}'.");
                        break;
                    default:
                        throw new MavenTaskException($"Unknown dependency qualifier '{key}' in '{value}'.");
                }
            }

            return item;
        }

        /// <summary>
        /// Path from the reference to the artifact upon which the dependency is declared. An empty path declares the
        /// dependency upon the reference itself.
        /// </summary>
        public MavenReferenceItemDependencySelector[] Path { get; set; } = Array.Empty<MavenReferenceItemDependencySelector>();

        /// <summary>
        /// The Maven group ID of the dependency. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The Maven artifact ID of the dependency. Required.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The extension of the dependency.
        /// </summary>
        public string Extension { get; set; } = "jar";

        /// <summary>
        /// The Maven classifier of the dependency. Optional.
        /// </summary>
        public string Classifier { get; set; } = "";

        /// <summary>
        /// The version of the dependency. Required.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The scope of the dependency.
        /// </summary>
        public string Scope { get; set; } = JavaScopes.COMPILE;

        /// <summary>
        /// Whether the dependency is optional.
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as MavenReferenceItemDependency);
        }

        /// <summary>
        /// Returns <c>true</c> if the this item is equal to the other item.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MavenReferenceItemDependency other)
        {
            return other is not null &&
                Path.SequenceEqual(other.Path) &&
                GroupId == other.GroupId &&
                ArtifactId == other.ArtifactId &&
                Extension == other.Extension &&
                Classifier == other.Classifier &&
                Version == other.Version &&
                Scope == other.Scope &&
                Optional == other.Optional;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var h = new HashCode();
            foreach (var i in Path)
                h.Add(i);
            h.Add(GroupId);
            h.Add(ArtifactId);
            h.Add(Extension);
            h.Add(Classifier);
            h.Add(Version);
            h.Add(Scope);
            h.Add(Optional);
            return h.ToHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var b = new StringBuilder();

            foreach (var i in Path)
                b.Append(i).Append('/');

            b.Append(GroupId).Append(':').Append(ArtifactId);
            var classifier = string.IsNullOrEmpty(Classifier) == false;
            if (classifier || Extension != "jar")
            {
                b.Append(':').Append(Extension);
                if (classifier)
                    b.Append(':').Append(Classifier);
            }
            b.Append(':').Append(Version);

            if (Scope != JavaScopes.COMPILE)
                b.Append(",scope=").Append(Scope);
            if (Optional)
                b.Append(",optional=true");

            return b.ToString();
        }

    }

}
