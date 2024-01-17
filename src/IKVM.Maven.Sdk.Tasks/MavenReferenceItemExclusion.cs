using System;

namespace IKVM.Maven.Sdk.Tasks
{

    public class MavenReferenceItemExclusion : IEquatable<MavenReferenceItemExclusion>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="artifactId"></param>
        /// <param name="classifier"></param>
        /// <param name="extension"></param>
        public MavenReferenceItemExclusion(string groupId, string artifactId, string classifier, string extension)
        {
            GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            ArtifactId = artifactId ?? throw new ArgumentNullException(nameof(artifactId));
            Classifier = classifier;
            Extension = extension;
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
        /// Gets the classifier of the exclusion.
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// Gets the extension ID of the exclusion.
        /// </summary>
        public string Extension { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as MavenReferenceItemExclusion);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(GroupId, ArtifactId, Classifier, Extension);
        }

        /// <summary>
        /// Returns <c>true</c> if the this item is equal to the other item.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(MavenReferenceItemExclusion other)
        {
            return other is not null && GroupId == other.GroupId && ArtifactId == other.ArtifactId && Classifier == other.Classifier && Extension == other.Extension;
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GroupId}:{ArtifactId}";
        }

    }

}