using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.graph;
using org.eclipse.aether.resolution;
using org.eclipse.aether.util.artifact;
using org.eclipse.aether.util.filter;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/>, resolves the full set of MavenReferenceItem's that should be generated.
    /// </summary>
    public class MavenReferenceItemResolve : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemResolve() :
            base(SR.ResourceManager, "MAVEN:")
        {

        }

        /// <summary>
        /// Set of MavenReferenceItem.
        /// </summary>
        [Required]
        [Output]
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// Set of MavenReferenceItem that is the fully resolved set.
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedItems { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                var items = MavenReferenceItemUtil.Import(Items);
                ResolvedItems = ResolveItems(items).Select(i => i.Item).ToArray();
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        /// <summary>
        /// Resolves the set of dependencies given by <paramref name="items"/> and augments their metadata.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        List<MavenReferenceItem> ResolveItems(MavenReferenceItem[] items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var maven = new IkvmMavenEnvironment(Log);

            // convert set of incoming items into a dependency list
            var dependencies = new java.util.ArrayList();
            foreach (var item in items)
            {
                dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, "jar", item.Version), JavaScopes.COMPILE));
                dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, "jar", item.Version), JavaScopes.RUNTIME, new java.lang.Boolean(true), new java.util.ArrayList()));
                dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, "sources", "jar", item.Version), JavaScopes.COMPILE, new java.lang.Boolean(true), new java.util.ArrayList()));
                dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, "sources", "jar", item.Version), JavaScopes.RUNTIME, new java.lang.Boolean(true), new java.util.ArrayList()));
            }

            // resolve the artifacts
            var result = maven.RepositorySystem.resolveDependencies(
                maven.RepositorySystemSession,
                new DependencyRequest(
                    new CollectRequest(dependencies, null, maven.Repositories),
                    DependencyFilterUtils.classpathFilter(JavaScopes.COMPILE, JavaScopes.RUNTIME)));

            // assemble and merge the resulting artifacts
            var output = new Dictionary<string, MavenReferenceItem>();
            foreach (ArtifactResult artifact in (IEnumerable)result.getArtifactResults())
                MergeMavenReferenceItemFromArtifact(output, artifact.getRequest().getDependencyNode());

            // return results
            return output.Values.ToList();
        }

        /// <summary>
        /// Merges the <see cref="DependencyNode"/> into the output dictionary.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="node"></param>
        MavenReferenceItem MergeMavenReferenceItemFromArtifact(Dictionary<string, MavenReferenceItem> output, DependencyNode node)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            var artifact = node.getArtifact();
            if (artifact == null)
                return null;

            // obtain or create an existing item
            var itemSpec = $"{artifact.getGroupId()}:{artifact.getArtifactId()}:{artifact.getVersion()}";
            if (output.TryGetValue(itemSpec, out var item) == false)
                output[itemSpec] = item = new MavenReferenceItem(new TaskItem(itemSpec)) { ItemSpec = itemSpec };

            // configure the basic data
            item.ItemSpec = MavenReferenceItemUtil.GetItemSpec(artifact.getGroupId(), artifact.getArtifactId(), artifact.getVersion());
            item.GroupId = artifact.getGroupId();
            item.ArtifactId = artifact.getArtifactId();
            item.Version = artifact.getVersion();

            // process the dependencies of this reference
            foreach (DependencyNode child in (IEnumerable)node.getChildren())
            {
                var dependency = MergeMavenReferenceItemFromArtifact(output, child);
                if (dependency == null)
                    throw new NullReferenceException();

                item.Dependencies.Add(dependency);
            }

            // if the artifact is a jar, we need to associate the path to the jar to the item
            if (artifact.getExtension() == "jar" && artifact.getClassifier() == "")
            {
                var file = artifact.getFile().getAbsolutePath();
                if (item.Compile.Contains(file) == false)
                    item.Compile.Add(file);
            }

            // if the artifact is a sources.jar, we need to associate the path to the jar to the item
            if (artifact.getExtension() == "jar" && artifact.getClassifier() == "sources")
            {
                var file = artifact.getFile().getAbsolutePath();
                if (item.Sources.Contains(file) == false)
                    item.Sources.Add(file);
            }

            // write changed data to underlying TaskItem
            item.Save();
            return item;
        }

    }

}
