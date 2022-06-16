using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IKVM.Sdk.Maven.Tasks
{
    internal class MavenArtifactItem
    {
        private static readonly List<MavenArtifactItem> EmptyList = new();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="artifact">The artifact to copy properties from.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="dependencies">The dependency node containing the list of dependencies.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public MavenArtifactItem(Artifact artifact, string destinationFolder, DependencyNode dependencies = null)
        {
            if (artifact is null)
                throw new ArgumentNullException(nameof(artifact));
            if (string.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentNullException(nameof(destinationFolder));

            GroupId = artifact.getGroupId();
            ArtifactId = artifact.getArtifactId();
            Ext = artifact.getExtension();
            Version = artifact.getVersion();
            Classifier = artifact.getClassifier();
            FilePath = IkvmMavenArtifactItemUtil.GetDestinationFilePath(artifact, destinationFolder);
            References = dependencies is null ? EmptyList : IkvmMavenArtifactItemUtil.ConvertDependencies(dependencies, destinationFolder).ToList();
            ItemSpec = ToString(); // Must be executed last.
        }

        /// <summary>
        /// Referenced node.
        /// </summary>
        public ITaskItem Item { get; } = new TaskItem();

        /// <summary>
        /// Unique name of the item.
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// The Maven artifact ID.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The Maven group ID. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The extension of the artifact to download. Optional.
        /// </summary>
        public string Ext { get; set; } // NOTE: Extension is a reserved word in ITaskItem metadata, hence the abbreviation

        /// <summary>
        /// The version of the Maven artifact. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The classifier of the Maven artifact.
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// The path to the file where this artifact file resides.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// The list of direct dependencies.
        /// </summary>
        public List<MavenArtifactItem> References { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(GroupId);
            sb.Append(':');
            sb.Append(ArtifactId);
            sb.Append(':');
            sb.Append(Ext);
            sb.Append(':');
            sb.Append(Version);
            if (!string.IsNullOrEmpty(Classifier))
            {
                sb.Append(':');
                sb.Append(Classifier);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public MavenArtifactItem Save()
        {
            Item.ItemSpec = ItemSpec;
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.ArtifactId, ArtifactId);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.GroupId, GroupId);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.Extension, Ext);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.Version, Version);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.Classifier, Classifier);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.FilePath, FilePath);
            Item.SetMetadata(IkvmMavenArtifactItemMetadata.References, string.Join(";", References.Select(i => i.FilePath)));
            return this;
        }
    }
}
