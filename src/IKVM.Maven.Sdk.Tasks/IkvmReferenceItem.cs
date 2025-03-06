using System.Collections.Generic;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="IkvmReferenceItem"/>.
    /// </summary>
    class IkvmReferenceItem
    {

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
        /// Name of the classloader to use.
        /// </summary>
        public string ClassLoader { get; set; }

        /// <summary>
        /// Compile in debug mode.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Path to the file to sign the assembly.
        /// </summary>
        public string KeyFile { get; set; }

        /// <summary>
        /// Whether to delay sign the produced assembly.
        /// </summary>
        public bool DelaySign { get; set; }

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

        /// <inheritdoc />
        public override string ToString() => ItemSpec;

    }

}
