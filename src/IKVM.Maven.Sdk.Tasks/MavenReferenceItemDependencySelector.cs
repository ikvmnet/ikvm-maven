using System;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Selects an artifact within a dependency tree by its coordinates. A <c>null</c> version matches any version.
    /// </summary>
    public class MavenReferenceItemDependencySelector : IEquatable<MavenReferenceItemDependencySelector>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="artifactId"></param>
        /// <param name="version"></param>
        public MavenReferenceItemDependencySelector(string groupId, string artifactId, string version)
        {
            GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            ArtifactId = artifactId ?? throw new ArgumentNullException(nameof(artifactId));
            Version = version;
        }

        /// <summary>
        /// The Maven group ID. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The Maven artifact ID. Required.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The version to match. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as MavenReferenceItemDependencySelector);
        }

        /// <summary>
        /// Returns <c>true</c> if the this item is equal to the other item.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MavenReferenceItemDependencySelector other)
        {
            return other is not null && GroupId == other.GroupId && ArtifactId == other.ArtifactId && Version == other.Version;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(GroupId, ArtifactId, Version);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Version == null ? $"{GroupId}:{ArtifactId}" : $"{GroupId}:{ArtifactId}:{Version}";
        }

    }

}
