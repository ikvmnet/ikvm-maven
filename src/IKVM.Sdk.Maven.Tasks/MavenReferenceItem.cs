using System;

using Microsoft.Build.Framework;

namespace IKVM.Sdk.Maven.Tasks
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
        /// Unique name of the item.
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// Group ID of the maven reference.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Artifact ID of the maven reference.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// Version of the maven reference.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public void Save()
        {
            Item.ItemSpec = ItemSpec;
            Item.SetMetadata(MavenReferenceItemMetadata.GroupId, GroupId);
            Item.SetMetadata(MavenReferenceItemMetadata.ArtifactId, ArtifactId);
            Item.SetMetadata(MavenReferenceItemMetadata.Version, Version);
        }

    }

}
