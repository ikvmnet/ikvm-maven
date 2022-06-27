using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Framework;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="IkvmReferenceItem"/>.
    /// </summary>
    class IkvmReferenceItem
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public IkvmReferenceItem(ITaskItem item)
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
        /// Assembly name.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Assembly version.
        /// </summary>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// Disables the automatic detection of the assembly name.
        /// </summary>
        public bool DisableAutoAssemblyName { get; set; } = false;

        /// <summary>
        /// Disables the automatic detection of the assembly version.
        /// </summary>
        public bool DisableAutoAssemblyVersion { get; set; } = false;

        /// <summary>
        /// Fallback assembly name if otherwise not provided.
        /// </summary>
        public string FallbackAssemblyName { get; set; }

        /// <summary>
        /// Fallback assembly version if otherwise not provided.
        /// </summary>
        public string FallbackAssemblyVersion { get; set; }

        /// <summary>
        /// Compile in debug mode.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Set of sources to compile.
        /// </summary>
        public List<string> Compile { get; set; } = new List<string>();

        /// <summary>
        /// Set of Java sources which can be used to generate documentation.
        /// </summary>
        public List<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// References required to compile.
        /// </summary>
        public List<IkvmReferenceItem> References { get; set; } = new List<IkvmReferenceItem>();

        /// <summary>
        /// Whether the item will be copied along with the build output.
        /// </summary>
        public bool Private { get; set; } = true;

        /// <summary>
        /// Whether a reference should be added to this item.
        /// </summary>
        public bool ReferenceOutputAssembly { get; set; } = true;

        /// <summary>
        /// Unique IKVM identity of the reference.
        /// </summary>
        public string IkvmIdentity { get; set; }

        /// <summary>
        /// Metadata attached by the Maven process.
        /// </summary>
        public string MavenGroupId { get; set; }

        /// <summary>
        /// Metadata attached by the Maven process.
        /// </summary>
        public string MavenArtifactId { get; set; }

        /// <summary>
        /// Metadata attached by the Maven process.
        /// </summary>
        public string MavenClassifier { get; set; }

        /// <summary>
        /// Metadata attached by the Maven process.
        /// </summary>
        public string MavenVersion { get; set; }

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public void Save()
        {
            Item.ItemSpec = ItemSpec;
            Item.SetMetadata(IkvmReferenceItemMetadata.AssemblyName, AssemblyName);
            Item.SetMetadata(IkvmReferenceItemMetadata.AssemblyVersion, AssemblyVersion);
            Item.SetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyName, DisableAutoAssemblyName ? "true" : "false");
            Item.SetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyVersion, DisableAutoAssemblyVersion ? "true" : "false");
            Item.SetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyName, FallbackAssemblyName);
            Item.SetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion, FallbackAssemblyVersion);
            Item.SetMetadata(IkvmReferenceItemMetadata.Debug, Debug ? "true" : "false");
            Item.SetMetadata(IkvmReferenceItemMetadata.Compile, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, Compile));
            Item.SetMetadata(IkvmReferenceItemMetadata.Sources, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, Sources));
            Item.SetMetadata(IkvmReferenceItemMetadata.References, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, References.Select(i => i.ItemSpec)));
            Item.SetMetadata(IkvmReferenceItemMetadata.Private, Private ? "true" : "false");
            Item.SetMetadata(IkvmReferenceItemMetadata.ReferenceOutputAssembly, ReferenceOutputAssembly ? "true" : "false");
            Item.SetMetadata(IkvmReferenceItemMetadata.IkvmIdentity, IkvmIdentity);
            Item.SetMetadata(IkvmReferenceItemMetadata.MavenGroupId, MavenGroupId);
            Item.SetMetadata(IkvmReferenceItemMetadata.MavenArtifactId, MavenArtifactId);
            Item.SetMetadata(IkvmReferenceItemMetadata.MavenClassifier, MavenClassifier);
            Item.SetMetadata(IkvmReferenceItemMetadata.MavenVersion, MavenVersion);
        }

    }

}
