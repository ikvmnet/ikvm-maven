using System;

using Microsoft.Build.Framework;

using org.eclipse.aether.util.artifact;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="MavenReferenceItem"/>.
    /// </summary>
    class MavenReferenceItem
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MavenReferenceItem(ITaskItem item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>
        /// Referenced node.
        /// </summary>
        public ITaskItem Item { get; }

        /// <summary>
        /// The identity of the item.
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// The Maven group ID. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The Maven artifact ID. Required.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The Maven classifier. Optional.
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// The version of the Maven reference. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Whether the reference is optional.
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <summary>
        /// The scopes of this reference
        /// </summary>
        public string Scope { get; set; } = JavaScopes.COMPILE;

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public void Save()
        {
            Item.ItemSpec = ItemSpec;
            Item.SetMetadata(MavenReferenceItemMetadata.GroupId, GroupId);
            Item.SetMetadata(MavenReferenceItemMetadata.ArtifactId, ArtifactId);
            Item.SetMetadata(MavenReferenceItemMetadata.Classifier, Classifier);
            Item.SetMetadata(MavenReferenceItemMetadata.Version, Version);
            Item.SetMetadata(MavenReferenceItemMetadata.Optional, Optional ? "true" : "false");
            Item.SetMetadata(MavenReferenceItemMetadata.Scope, Scope);
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GroupId}:{ArtifactId}";
        }

    }

}
