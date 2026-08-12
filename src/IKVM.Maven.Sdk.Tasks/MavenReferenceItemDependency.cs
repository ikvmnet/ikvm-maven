using System;
using System.Linq;
using System.Text;

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
        /// 'groupId:artifactId[:version]'. The final segment describes the dependency to be added in the form
        /// 'groupId:artifactId:version[:scope][:optional]'.
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

            // final segment describes the dependency to add
            var d = segments[segments.Length - 1].Split(':');
            if (d.Length is < 3 or > 5)
                throw new MavenTaskException($"Invalid dependency '{segments[segments.Length - 1]}' in '{value}'.");

            var item = new MavenReferenceItemDependency();
            item.Path = path;
            item.GroupId = d[0];
            item.ArtifactId = d[1];
            item.Version = d[2];

            // remaining tokens may be a scope followed by the literal 'optional'
            for (int i = 3; i < d.Length; i++)
            {
                if (string.Equals(d[i], "optional", StringComparison.OrdinalIgnoreCase))
                    item.Optional = true;
                else if (i == 3 && item.Optional == false)
                    item.Scope = d[i];
                else
                    throw new MavenTaskException($"Invalid dependency '{segments[segments.Length - 1]}' in '{value}'.");
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

            b.Append(GroupId).Append(':').Append(ArtifactId).Append(':').Append(Version);
            if (Scope != JavaScopes.COMPILE)
                b.Append(':').Append(Scope);
            if (Optional)
                b.Append(':').Append("optional");

            return b.ToString();
        }

    }

}
