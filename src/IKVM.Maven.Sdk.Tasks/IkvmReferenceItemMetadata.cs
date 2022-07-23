using System;
using System.Linq;

using Microsoft.Build.Framework;

namespace IKVM.Maven.Sdk.Tasks
{

    static class IkvmReferenceItemMetadata
    {

        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string AssemblyName = "AssemblyName";
        public static readonly string AssemblyVersion = "AssemblyVersion";
        public static readonly string DisableAutoAssemblyName = "DisableAutoAssemblyName";
        public static readonly string DisableAutoAssemblyVersion = "DisableAutoAssemblyVersion";
        public static readonly string FallbackAssemblyName = "FallbackAssemblyName";
        public static readonly string FallbackAssemblyVersion = "FallbackAssemblyVersion";
        public static readonly string Compile = "Compile";
        public static readonly string Sources = "Sources";
        public static readonly string References = "References";
        public static readonly string ClassLoader = "ClassLoader";
        public static readonly string Debug = "Debug";
        public static readonly string KeyFile = "KeyFile";
        public static readonly string DelaySign = "DelaySign";
        public static readonly string Private = "Private";
        public static readonly string ReferenceOutputAssembly = "ReferenceOutputAssembly";

        public static readonly string IkvmIdentity = "IkvmIdentity";
        public static readonly string MavenGroupId = "MavenGroupId";
        public static readonly string MavenArtifactId = "MavenArtifactId";
        public static readonly string MavenClassifier = "MavenClassifier";
        public static readonly string MavenVersion = "MavenVersion";

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public static void Save(IkvmReferenceItem item, ITaskItem task)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            task.ItemSpec = item.ItemSpec;
            task.SetMetadata(IkvmReferenceItemMetadata.AssemblyName, item.AssemblyName);
            task.SetMetadata(IkvmReferenceItemMetadata.AssemblyVersion, item.AssemblyVersion);
            task.SetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyName, item.DisableAutoAssemblyName ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyVersion, item.DisableAutoAssemblyVersion ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyName, item.FallbackAssemblyName);
            task.SetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion, item.FallbackAssemblyVersion);
            task.SetMetadata(IkvmReferenceItemMetadata.Compile, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, item.Compile));
            task.SetMetadata(IkvmReferenceItemMetadata.Sources, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, item.Sources));
            task.SetMetadata(IkvmReferenceItemMetadata.References, string.Join(IkvmReferenceItemMetadata.PropertySeperatorString, item.References.Select(i => i.ItemSpec)));
            task.SetMetadata(IkvmReferenceItemMetadata.ClassLoader, item.ClassLoader);
            task.SetMetadata(IkvmReferenceItemMetadata.Debug, item.Debug ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.KeyFile, item.KeyFile);
            task.SetMetadata(IkvmReferenceItemMetadata.DelaySign, item.DelaySign ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.Private, item.Private ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.ReferenceOutputAssembly, item.ReferenceOutputAssembly ? "true" : "false");
            task.SetMetadata(IkvmReferenceItemMetadata.IkvmIdentity, item.IkvmIdentity);
            task.SetMetadata(IkvmReferenceItemMetadata.MavenGroupId, item.MavenGroupId);
            task.SetMetadata(IkvmReferenceItemMetadata.MavenArtifactId, item.MavenArtifactId);
            task.SetMetadata(IkvmReferenceItemMetadata.MavenClassifier, item.MavenClassifier);
            task.SetMetadata(IkvmReferenceItemMetadata.MavenVersion, item.MavenVersion);
        }

    }

}
