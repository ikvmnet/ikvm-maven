using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Framework;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="MavenReferenceItem"/>.
    /// </summary>
    internal class MavenReferenceItem
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
        /// The version of the Maven reference. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets the dependencies of this maven reference.
        /// </summary>
        public List<MavenReferenceItem> Dependencies { get; set; } = new List<MavenReferenceItem>();

        /// <summary>
        /// Gets the path to the compile items.
        /// </summary>
        public List<string> Compile { get; set; } = new List<string>();

        /// <summary>
        /// Gets the path to the sources items.
        /// </summary>
        public List<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public void Save()
        {
            Item.ItemSpec = ItemSpec;
            Item.SetMetadata(MavenReferenceItemMetadata.GroupId, GroupId);
            Item.SetMetadata(MavenReferenceItemMetadata.ArtifactId, ArtifactId);
            Item.SetMetadata(MavenReferenceItemMetadata.Version, Version);
            Item.SetMetadata(MavenReferenceItemMetadata.Dependencies, string.Join(MavenReferenceItemMetadata.PropertySeperatorString, Dependencies.Select(i => i.ItemSpec)));
            Item.SetMetadata(MavenReferenceItemMetadata.Compile, string.Join(MavenReferenceItemMetadata.PropertySeperatorString, Compile));
            Item.SetMetadata(MavenReferenceItemMetadata.Sources, string.Join(MavenReferenceItemMetadata.PropertySeperatorString, Sources));
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Version) == false)
                return $"{GroupId}:{ArtifactId}:{Version}";
            else
                return $"{GroupId}:{ArtifactId}";
        }

    }

}
