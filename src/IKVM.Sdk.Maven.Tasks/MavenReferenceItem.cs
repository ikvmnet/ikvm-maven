using System;

using Microsoft.Build.Framework;

using org.eclipse.aether.util.artifact;

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
        /// Force the assembly name to the given value.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Force the assembly version to the given value.
        /// </summary>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// Generate debug information.
        /// </summary>
        public bool Debug { get; set; } = false;

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
            Item.SetMetadata(MavenReferenceItemMetadata.AssemblyName, AssemblyName);
            Item.SetMetadata(MavenReferenceItemMetadata.AssemblyVersion, AssemblyVersion);
            Item.SetMetadata(MavenReferenceItemMetadata.Debug, Debug ? "true" : "false");
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
