using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKVM.Sdk.Maven.Tasks
{
    internal class IkvmMavenArtifactItemUtil
    {
        public static string GetDestinationFilePath(Artifact artifact, string destinationFolder)
        {
            return Path.Combine(destinationFolder, Path.GetFileName(artifact.getFile().getCanonicalPath()));
        }


        public static IEnumerable<MavenArtifactItem> ConvertDependencies(DependencyNode dependencies, string destinationFolder)
        {
            foreach (DependencyNode child in (System.Collections.IEnumerable)dependencies.getChildren())
            {
                var artifact = child.getArtifact();
                yield return new MavenArtifactItem(artifact, GetDestinationFilePath(artifact, destinationFolder)).Save();
            }
        }
    }
}
