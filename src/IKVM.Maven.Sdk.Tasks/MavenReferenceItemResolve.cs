using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IKVM.Maven.Sdk.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.artifact.versioning;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.graph;
using org.eclipse.aether.resolution;
using org.eclipse.aether.util.artifact;
using org.eclipse.aether.util.filter;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/>, resolves the full set of MavenReferenceItem's that should be generated.
    /// </summary>
    public class MavenReferenceItemResolve : Task
    {

        static readonly java.lang.Boolean TRUE = new java.lang.Boolean(true);
        static readonly java.lang.Boolean FALSE = new java.lang.Boolean(false);

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
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// Set of output IkvmReferenceItem instances.
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedItems { get; set; }

        /// <summary>
        /// Name of the classloader to use for the reference items.
        /// </summary>
        public string ClassLoader { get; set; }

        /// <summary>
        /// Value to set for Debug on generated references unless otherwise specified.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Path to the key file to use for signing Maven assemblies.
        /// </summary>
        public string KeyFile { get; set; }

        /// <summary>
        /// Indicates whether the resolution should include test items.
        /// </summary>
        public bool IncludeTestScope { get; set; }

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
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true, true, null);
                return false;
            }
        }

        /// <summary>
        /// Resolves the set of dependencies given by <paramref name="items"/> and augments their metadata.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        IEnumerable<IkvmReferenceItem> ResolveItems(MavenReferenceItem[] items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var maven = new IkvmMavenEnvironment(Log);

            // convert set of incoming items into a dependency list
            var dependencies = new java.util.ArrayList();
            foreach (var item in items)
                dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, item.Classifier, "jar", item.Version), item.Scope, item.Optional ? TRUE : FALSE, new java.util.ArrayList()));

            // filter for desired scopes to retrieve
            var filter = new List<string>();
            filter.Add(JavaScopes.COMPILE);
            filter.Add(JavaScopes.RUNTIME);
            filter.Add(JavaScopes.PROVIDED);
            if (IncludeTestScope)
                filter.Add(JavaScopes.TEST);

            // resolve the artifacts
            var result = maven.RepositorySystem.resolveDependencies(
                maven.RepositorySystemSession,
                new DependencyRequest(
                    new CollectRequest(dependencies, null, maven.Repositories),
                    DependencyFilterUtils.classpathFilter(filter.ToArray())));

            // merge tree twice, once for compile artifacts, and again for sources artifacts
            var output = new List<IkvmReferenceItem>();
            foreach (ArtifactResult artifact in (IEnumerable)result.getArtifactResults())
                MergeIkvmReferenceItemArtifacts(items, output, artifact.getRequest().getDependencyNode());

            // return results
            return output;
        }

        /// <summary>
        /// Merges the <see cref="DependencyNode"/> into the output dictionary.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="output"></param>
        /// <param name="node"></param>
        IkvmReferenceItem MergeIkvmReferenceItemArtifacts(MavenReferenceItem[] items, List<IkvmReferenceItem> output, DependencyNode node)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            var artifact = node.getArtifact();
            if (artifact == null)
                return null;

            // apply artifact as IkvmReferenceItem
            var outputItem = MergeIkvmReferenceItemFromCompileArtifact(output, node, artifact);
            if (outputItem == null)
                throw new MavenTaskException("Null result merging compile artifact.");

            // each dependency gets translated into a reference
            foreach (DependencyNode child in (IEnumerable)node.getChildren())
            {
                // recurse into dependency
                var dependency = MergeIkvmReferenceItemArtifacts(items, output, child);
                if (dependency == null) // might be a sources artifact
                    continue;

                // add reference if not already added
                if (outputItem.References.Contains(dependency) == false)
                    outputItem.References.Add(dependency);

                // ensure each dependency references the references from its own references
                foreach (var transitiveDependency in dependency.References)
                    if (outputItem.References.Contains(transitiveDependency) == false)
                        outputItem.References.Add(transitiveDependency);
            }

            // persist modified item
            outputItem.Save();
            return outputItem;
        }

        /// <summary>
        /// Merges a compile artifact into the IkvmReferenceItem.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="node"></param>
        /// <param name="artifact"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IkvmReferenceItem MergeIkvmReferenceItemFromCompileArtifact(List<IkvmReferenceItem> output, DependencyNode node, Artifact artifact)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (artifact is null)
                throw new ArgumentNullException(nameof(artifact));

            // pull items out of artifact
            var groupId = artifact.getGroupId();
            var artifactId = artifact.getArtifactId();
            var classifier = artifact.getClassifier();
            var version = artifact.getVersion();

            // find an existing IkvmReferenceItem that matches this artifact
            var outputItem = output.FirstOrDefault(i => i.MavenGroupId == groupId && i.MavenArtifactId == artifactId && i.MavenClassifier == classifier && i.MavenVersion == version);
            if (outputItem == null)
                outputItem = output.FirstOrDefault(i => i.MavenGroupId == groupId && i.MavenArtifactId == artifactId && i.MavenVersion == version);
            if (outputItem == null)
                outputItem = output.FirstOrDefault(i => i.MavenGroupId == groupId && i.MavenArtifactId == artifactId);
            if (outputItem == null)
            {
                // generate a new IkvmReferenceItem, prefixed so it doesn't conflict with others
                var outputItemSpec = GetIkvmItemSpec(groupId, artifactId, classifier, version);
                output.Add(outputItem = new IkvmReferenceItem(new TaskItem(outputItemSpec)) { ItemSpec = outputItemSpec, ReferenceOutputAssembly = false, Private = false });
            }

            // ensure output item has Maven information attached to it
            outputItem.MavenGroupId = groupId;
            outputItem.MavenArtifactId = artifactId;
            outputItem.MavenClassifier = classifier;
            outputItem.MavenVersion = version;

            // fallback to the Maven name and version if IKVM cannot detect otherwise
            outputItem.FallbackAssemblyName = artifactId;
            outputItem.FallbackAssemblyVersion = ToAssemblyVersion(version)?.ToString();

            // inherit global settings
            outputItem.Debug = Debug;
            outputItem.KeyFile = KeyFile;

            // setup the class loader
            outputItem.ClassLoader = ClassLoader;

            // artifact is required during compile, ensure we reference
            if (node.getDependency().getScope() == JavaScopes.COMPILE)
            {
                outputItem.Private = true;
                outputItem.ReferenceOutputAssembly = true;
            }

            // artifact is required during runtime, copy local
            if (node.getDependency().getScope() == JavaScopes.RUNTIME)
            {
                outputItem.Private = true;
            }

            // artifact is required during compile, but provided by runtime
            if (node.getDependency().getScope() == JavaScopes.PROVIDED)
            {
                outputItem.ReferenceOutputAssembly = true;
            }

            // artifact is required for a test project, and we are a test project
            if (node.getDependency().getScope() == JavaScopes.TEST && IncludeTestScope)
            {
                outputItem.Private = true;
                outputItem.ReferenceOutputAssembly = true;
            }

            // if the artifact is a jar, we need to associate the path to the jar to the item
            var file = artifact.getFile().getAbsolutePath();
            if (outputItem.Compile.Contains(file) == false)
                outputItem.Compile.Add(file);

            // persist modified item
            outputItem.Save();
            return outputItem;
        }

        /// <summary>
        /// Returns a normalized version of a <see cref="MavenReferenceItem"/> itemspec.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="artifactId"></param>
        /// <param name="classifier"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        string GetIkvmItemSpec(string groupId, string artifactId, string classifier, string version)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException($"'{nameof(groupId)}' cannot be null or whitespace.", nameof(groupId));
            if (string.IsNullOrWhiteSpace(artifactId))
                throw new ArgumentException($"'{nameof(artifactId)}' cannot be null or whitespace.", nameof(artifactId));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException($"'{nameof(version)}' cannot be null or whitespace.", nameof(version));

            var b = new StringBuilder("maven$");
            b.Append(groupId);
            b.Append(':').Append(artifactId);
            if (string.IsNullOrWhiteSpace(classifier) == false)
                b.Append(':').Append(classifier);
            b.Append(':').Append(version);

            return b.ToString();
        }

        /// <summary>
        /// Parses the given Maven version into an assembly version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        Version ToAssemblyVersion(string version)
        {
            try
            {
                var v = new DefaultArtifactVersion(version);
                return new Version(v.getMajorVersion(), v.getMinorVersion());
            }
            catch (Exception)
            {
                return null;
            }
        }

    }

}
