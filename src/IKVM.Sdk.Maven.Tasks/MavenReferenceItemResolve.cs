using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.artifact.versioning;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.graph;
using org.eclipse.aether.resolution;

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
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// Set of output IkvmReferenceItem instances.
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedItems { get; set; }

        /// <summary>
        /// Value to set for Debug on generated references unless otherwise specified.
        /// </summary>
        public bool Debug { get; set; } = false;

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
                foreach (var scope in item.Scopes)
                    dependencies.add(new Dependency(new DefaultArtifact(item.GroupId, item.ArtifactId, item.Classifier, "jar", item.Version), scope, java.lang.Boolean.FALSE, new java.util.ArrayList()));

            // resolve the artifacts
            var result = maven.RepositorySystem.resolveDependencies(
                maven.RepositorySystemSession,
                new DependencyRequest(
                    new CollectRequest(dependencies, null, maven.Repositories),
                    null));

            // merge tree twice, once for compile artifacts, and again for sources artifacts
            var output = new List<IkvmReferenceItem>();
            foreach (ArtifactResult artifact in (IEnumerable)result.getArtifactResults())
                MergeIkvmReferenceItemCompileArtifacts(items, output, artifact.getRequest().getDependencyNode());

            // return results
            return output;
        }

        /// <summary>
        /// Merges the <see cref="DependencyNode"/> into the output dictionary.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="output"></param>
        /// <param name="node"></param>
        IkvmReferenceItem MergeIkvmReferenceItemCompileArtifacts(MavenReferenceItem[] items, List<IkvmReferenceItem> output, DependencyNode node)
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

            // find the original MavenReferenceItem that matches this artifact
            var item = items.FirstOrDefault(i => i.GroupId == artifact.getGroupId() && i.ArtifactId == artifact.getArtifactId() && i.Classifier == artifact.getClassifier() && i.Version == artifact.getVersion());
            if (item == null)
                item = items.FirstOrDefault(i => i.GroupId == artifact.getGroupId() && i.ArtifactId == artifact.getArtifactId() && i.Version == artifact.getVersion());
            if (item == null)
                item = items.FirstOrDefault(i => i.GroupId == artifact.getGroupId() && i.ArtifactId == artifact.getArtifactId());

            // apply artifact as IkvmReferenceItem
            var outputItem = MergeIkvmReferenceItemFromCompileArtifact(item, output, artifact);
            if (outputItem == null)
                throw new MavenTaskException("Null result merging compile artifact.");

            // each dependency gets translated into a reference
            foreach (DependencyNode child in (IEnumerable)node.getChildren())
            {
                // recurse into dependency
                var dependency = MergeIkvmReferenceItemCompileArtifacts(items, output, child);
                if (dependency == null) // might be a sources artifact
                    continue;

                outputItem.References.Add(dependency);
            }

            // persist modified item
            outputItem.Save();
            return outputItem;
        }

        /// <summary>
        /// Merges a compile artifact into the IkvmReferenceItem.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="output"></param>
        /// <param name="artifact"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IkvmReferenceItem MergeIkvmReferenceItemFromCompileArtifact(MavenReferenceItem item, List<IkvmReferenceItem> output, Artifact artifact)
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
                output.Add(outputItem = new IkvmReferenceItem(new TaskItem(outputItemSpec)) { ItemSpec = outputItemSpec });
            }

            // ensure output item has Maven information attached to it
            outputItem.MavenGroupId = groupId;
            outputItem.MavenArtifactId = artifactId;
            outputItem.MavenClassifier = classifier;
            outputItem.MavenVersion = version;

            // fallback to the Maven name and version if IKVM cannot detect otherwise
            outputItem.FallbackAssemblyName = artifactId;
            outputItem.FallbackAssemblyVersion = ToAssemblyVersion(version)?.ToString();

            // input item was matched, set new output based on input
            // user can override properties of transitive items by explicitely adding a dependency
            if (item != null)
            {
                // existing item specifies debug mode
                outputItem.Debug = item.Debug;

                // force the item's assembly name
                if (string.IsNullOrWhiteSpace(item.AssemblyName) == false)
                {
                    outputItem.DisableAutoAssemblyName = false;
                    outputItem.AssemblyName = item.AssemblyName;
                }

                // force the item's assembly name
                if (string.IsNullOrWhiteSpace(item.AssemblyVersion) == false)
                {
                    outputItem.DisableAutoAssemblyVersion = false;
                    outputItem.AssemblyName = item.AssemblyVersion;
                }
            }
            else
            {
                // default values
                outputItem.Debug = Debug;
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
