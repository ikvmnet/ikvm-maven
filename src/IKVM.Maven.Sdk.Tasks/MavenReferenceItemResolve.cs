﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using IKVM.Maven.Sdk.Tasks.Json;
using IKVM.Maven.Sdk.Tasks.Resources;

using java.util;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.artifact.resolver;
using org.apache.maven.artifact.versioning;
using org.eclipse.aether;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.graph;
using org.eclipse.aether.repository;
using org.eclipse.aether.resolution;
using org.eclipse.aether.util.artifact;
using org.eclipse.aether.util.filter;
using org.eclipse.aether.util.graph.transformer;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/>, resolves the full set of MavenReferenceItem's that should be generated.
    /// </summary>
    public class MavenReferenceItemResolve : Task
    {

        const int CACHE_FILE_VERSION = 3;

        static readonly JsonSerializerOptions serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new BooleanConverter(),
                new ArtifactJsonConverter(),
                new DefaultArtifactJsonConverter(),
                new DependencyNodeJsonConverter(),
                new DefaultDependencyNodeJsonConverter(),
                new DependencyJsonConverter(),
                new ExclusionJsonConverter(),
                new RemoteRepositoryJsonConverter(),
                new VersionJsonConverter(),
                new VersionConstraintJsonConverter(),
            },
            MaxDepth = 1024,
        };

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemResolve() :
            base(SR.ResourceManager, "MAVEN:")
        {

        }

        /// <summary>
        /// Path to the cache file.
        /// </summary>
        [Required]
        public string CacheFile { get; set; }

        /// <summary>
        /// Set of Maven repostories to initialize.
        /// </summary>
        [Required]
        public ITaskItem[] Repositories { get; set; }

        /// <summary>
        /// Set of MavenReferenceItem.
        /// </summary>
        [Required]
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Set of output IkvmReferenceItem instances.
        /// </summary>
        [Output]
        public ITaskItem[] ResolvedReferences { get; set; }

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
        /// Attempts to read the cache file.
        /// </summary>
        /// <returns></returns>
        MavenResolveCacheFile TryReadCacheFile()
        {
            if (CacheFile != null && File.Exists(CacheFile))
            {
                try
                {
                    using var stm = File.OpenRead(CacheFile);
                    if (stm.Length > 0)
                        return JsonSerializer.Deserialize<MavenResolveCacheFile>(stm, new JsonSerializerOptions(serializerOptions) { ReferenceHandler = new PreserveReferenceHandler() });
                }
                catch (JsonException)
                {
                    // ignore, consider file missing
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to write the cache file.
        /// </summary>
        /// <returns></returns>
        void TryWriteCacheFile(MavenResolveCacheFile cacheFile)
        {
            if (CacheFile != null)
            {
                using var stm = File.Create(CacheFile);
                JsonSerializer.Serialize(stm, cacheFile, new JsonSerializerOptions(serializerOptions) { ReferenceHandler = new PreserveReferenceHandler() });
            }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
#if DEBUG
            using var d = SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(Log, org.slf4j.@event.Level.TRACE));
#else
            using var d = SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(Log, org.slf4j.@event.Level.INFO));
#endif

            try
            {
                var repositories = MavenRepositoryItemMetadata.Load(Repositories);
                var items = MavenReferenceItemMetadata.Import(References);
                ResolvedReferences = ResolveReferences(repositories, items).Select(ToTaskItem).ToArray();
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
        /// Persists the item to a task item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ITaskItem ToTaskItem(IkvmReferenceItem item)
        {
            var task = new TaskItem();
            IkvmReferenceItemMetadata.Save(item, task);
            return task;
        }

        /// <summary>
        /// Resolves the set of dependencies given by the set of items.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IkvmReferenceItem> ResolveReferences(IList<MavenRepositoryItem> repositories, IList<MavenReferenceItem> items)
        {
            if (repositories == null)
                throw new ArgumentNullException(nameof(repositories));
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var maven = new IkvmMavenEnvironment(repositories, Log);
            var session = maven.CreateRepositorySystemSession(true);

            // root of the runtime dependency graph
            var graph = ResolveCompileDependencyGraph(maven, session, repositories, items);
            if (graph == null)
                throw new NullReferenceException("Null result obtaining dependency graph.");

            // walk the full dependency graph to generate items and their references
            var output = new Dictionary<string, IkvmReferenceItem>();
            CollectIkvmReferenceItems(output, graph, new HashSet<DependencyNode>());
            RemoveCircularReferences(output.Values);

            // resolve compile and runtime items and ensure they are copied
            var privateScopes = new List<string>() { JavaScopes.RUNTIME };
            if (IncludeTestScope)
                privateScopes.Add(JavaScopes.TEST);
            foreach (var ikvmItem in ResolveIkvmReferenceItemsForScopes(output, maven, session, graph, privateScopes))
                ikvmItem.Private = true;

            // resolve compile and provided items and ensure they are referenced
            var referenceOutputAssemblyScopes = new List<string>() { JavaScopes.COMPILE, JavaScopes.PROVIDED };
            if (IncludeTestScope)
                referenceOutputAssemblyScopes.Add(JavaScopes.TEST);
            foreach (var ikvmItem in ResolveIkvmReferenceItemsForScopes(output, maven, session, graph, referenceOutputAssemblyScopes))
                ikvmItem.ReferenceOutputAssembly = true;

            return output.Values;
        }

        /// <summary>
        /// Removes circular dependencies and outputs a warning. IKVM should still build these since it supports dynamic lookup.
        /// </summary>
        /// <param name="items"></param>
        void RemoveCircularReferences(IReadOnlyCollection<IkvmReferenceItem> items)
        {
            RemoveCircularReferences(items, new HashSet<IkvmReferenceItem>());
        }

        /// <summary>
        /// Removes circular dependencies and outputs a warning. IKVM should still build these since it supports dynamic lookup.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="stack"></param>
        void RemoveCircularReferences(IReadOnlyCollection<IkvmReferenceItem> items, HashSet<IkvmReferenceItem> stack)
        {
            foreach (var item in items)
            {
                // register this item as encountered
                stack.Add(item);

                // remove any items we have already encountered higher in the tree
                foreach (var i in stack)
                    if (item.References.Remove(i))
                        Log.LogWarningFromResources("Warning.MavenIgnoreCyclicReference", item.ItemSpec, i.ItemSpec);

                // recurse into references
                RemoveCircularReferences(item.References, stack);

                // unregister this item as encountered
                stack.Remove(item);
            }
        }

        /// <summary>
        /// Resolves the dependency graph for any items that may be relevant to the code of the application.
        /// </summary>
        /// <param name="maven"></param>
        /// <param name="session"></param>
        /// <param name="repositories"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        DependencyNode ResolveCompileDependencyGraph(IkvmMavenEnvironment maven, RepositorySystemSession session, IList<MavenRepositoryItem> repositories, IList<MavenReferenceItem> items)
        {
            if (maven is null)
                throw new ArgumentNullException(nameof(maven));
            if (session is null)
                throw new ArgumentNullException(nameof(session));
            if (repositories is null)
                throw new ArgumentNullException(nameof(repositories));
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            // convert set of incoming items into a dependency list
            var dependencies = new Dependency[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var exclusions = Arrays.asList(items[i].Exclusions.Select(j => new Exclusion(j.GroupId, j.ArtifactId, j.Classifier, j.Extension)).ToArray());
                dependencies[i] = new Dependency(new DefaultArtifact(items[i].GroupId, items[i].ArtifactId, items[i].Classifier, "jar", items[i].Version), items[i].Scope, items[i].Optional ? java.lang.Boolean.TRUE : java.lang.Boolean.FALSE, exclusions);
            }

            // check the cache
            var root = ResolveCompileDependencyGraphFromCache(maven, dependencies);
            if (root != null)
            {
                Log.LogMessageFromText("Resolved Maven dependency graph from project cache.", MessageImportance.Low);
                return root;
            }

            // collect the full dependency graph
            var filter = DependencyFilterUtils.classpathFilter(JavaScopes.COMPILE, JavaScopes.RUNTIME, JavaScopes.PROVIDED);
            if (IncludeTestScope)
                filter = DependencyFilterUtils.orFilter(DependencyFilterUtils.classpathFilter(JavaScopes.TEST));
            var result = maven.RepositorySystem.resolveDependencies(
                session,
                new DependencyRequest(
                    new CollectRequest(Arrays.asList(dependencies), null, maven.Repositories),
                    filter));

            root = (DefaultDependencyNode)result.getRoot();
            if (root == null)
                throw new MavenTaskException("Null dependency graph.");

            TryWriteCacheFile(new MavenResolveCacheFile()
            {
                Version = CACHE_FILE_VERSION,
                Dependencies = dependencies,
                Repositories = repositories.ToArray(),
                Graph = root,
            });

            return root;
        }

        /// <summary>
        /// Attempts to resolve the dependency graph from the cache file.
        /// </summary>
        /// <param name="maven"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        DefaultDependencyNode ResolveCompileDependencyGraphFromCache(IkvmMavenEnvironment maven, IList<Dependency> dependencies)
        {
            if (maven is null)
                throw new ArgumentNullException(nameof(maven));
            if (dependencies is null)
                throw new ArgumentNullException(nameof(dependencies));

            var cacheFile = TryReadCacheFile();
            if (cacheFile == null)
                return null;

            // current version
            if (cacheFile.Version != CACHE_FILE_VERSION)
                return null;

            // nothing was cached
            if (cacheFile.Graph == null)
                return null;

            // check that the same set of repositories are involved
            if (cacheFile.Repositories == null || UnorderedSequenceEquals(((IEnumerable)maven.Repositories).Cast<RemoteRepository>().ToArray(), cacheFile.Repositories) == false)
                return null;

            // check that the same set of dependencies are involved
            if (cacheFile.Dependencies == null || UnorderedSequenceEquals(dependencies, cacheFile.Dependencies) == false)
                return null;

            // return previously resolved graph
            return cacheFile.Graph;
        }

        /// <summary>
        /// Checks that each item in <paramref name="a"/> exists in <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool UnorderedSequenceEquals(IList<RemoteRepository> a, IList<MavenRepositoryItem> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            if (a.Count != b.Count)
                return false;

            foreach (var i in a)
                if (b.Any(j => j.Id == i.getId() && j.Url == i.getUrl()) == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Checks that each item in <paramref name="a"/> exists in <paramref name="b"/>.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool UnorderedSequenceEquals(IList<Dependency> a, IList<Dependency> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            if (a.Count != b.Count)
                return false;

            foreach (var i in a)
                if (b.Any(j => DependencyEqualityComparer.Default.Equals(i, j)) == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Attempts to resolve the source artifact for the specified <see cref="IkvmReferenceItem"/>.
        /// </summary>
        /// <param name="maven"></param>
        /// <param name="session"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Artifact ResolveSourceArtifact(IkvmMavenEnvironment maven, RepositorySystemSession session, IkvmReferenceItem item)
        {
            if (maven is null)
                throw new ArgumentNullException(nameof(maven));
            if (session is null)
                throw new ArgumentNullException(nameof(session));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            try
            {
                var result = maven.RepositorySystem.resolveArtifact(
                    session,
                    new ArtifactRequest(
                        new DefaultArtifact(
                            item.MavenGroupId,
                            item.MavenArtifactId,
                            string.IsNullOrWhiteSpace(item.MavenClassifier) ? "sources" : item.MavenClassifier + "-sources",
                            "jar",
                            item.MavenVersion),
                        maven.Repositories,
                        null));
                if (result.isResolved() == false)
                    return null;
                if (result.getArtifact() is not Artifact artifact)
                    return null;

                return artifact;
            }
            catch (org.eclipse.aether.resolution.ArtifactResolutionException)
            {
                return null;
            }
            catch (ArtifactNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Resolves the <see cref="IkvmReferenceItem"/>s that are applicable for the given dependency set and scopes.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="maven"></param>
        /// <param name="session"></param>
        /// <param name="root"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IEnumerable<IkvmReferenceItem> ResolveIkvmReferenceItemsForScopes(Dictionary<string, IkvmReferenceItem> output, IkvmMavenEnvironment maven, RepositorySystemSession session, DependencyNode root, List<string> scopes)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (maven is null)
                throw new ArgumentNullException(nameof(maven));
            if (session is null)
                throw new ArgumentNullException(nameof(session));
            if (root is null)
                throw new ArgumentNullException(nameof(root));
            if (scopes is null)
                throw new ArgumentNullException(nameof(scopes));

            var result = maven.RepositorySystem.resolveDependencies(
                session,
                new DependencyRequest(root, DependencyFilterUtils.classpathFilter(scopes.ToArray())));

            foreach (ArtifactResult resultItem in (IEnumerable)result.getArtifactResults())
                if (GetIkvmReferenceItemForArtifact(output, resultItem.getArtifact()) is IkvmReferenceItem ikvmItem)
                    yield return ikvmItem;
        }

        /// <summary>
        /// Collects any <see cref="IkvmReferenceItem"/>s from the given node.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="node"></param>
        void CollectIkvmReferenceItems(Dictionary<string, IkvmReferenceItem> output, DependencyNode node, HashSet<DependencyNode> processed)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            // resolve to winner of a conflict instead
            node = GetEffectiveNode(node);

            // check to see whether we've already processed the node, else continue
            if (processed.Add(node) == false)
                return;

            // walk tree and ensure IkvmReferenceItem exists for each child
            var scopes = new HashSet<string>() { JavaScopes.COMPILE, JavaScopes.RUNTIME, JavaScopes.PROVIDED };
            if (IncludeTestScope)
                scopes.Add(JavaScopes.TEST);
            foreach (DependencyNode child in GetEffectiveChildren(node))
                if (scopes.Contains(child.getDependency().getScope()))
                    CollectIkvmReferenceItems(output, child, processed);

            // if artifact, obtain IkvmReferenceItem from artifact
            var artifact = node.getArtifact();
            var ikvmItem = artifact != null ? GetOrCreateIkvmReferenceItemForArtifact(output, artifact) : null;

            // if we've got an actual item, traverse it's dependencies to assign references
            if (ikvmItem != null)
                foreach (var ikvmReference in CollectIkvmReferenceItemReferences(output, node, new HashSet<DependencyNode>()))
                    if (ikvmItem != ikvmReference && ikvmItem.References.Contains(ikvmReference) == false)
                        ikvmItem.References.Add(ikvmReference);
        }

        /// <summary>
        /// Recursively populates the output collection with IkvmReferenceItems.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        IEnumerable<IkvmReferenceItem> CollectIkvmReferenceItemReferences(Dictionary<string, IkvmReferenceItem> output, DependencyNode node, HashSet<DependencyNode> processed)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            // resolve to winner of a conflict instead
            node = GetEffectiveNode(node);

            // check to see whether we've already processed the node, else continue
            if (processed.Add(node) == false)
                yield break;

            // each child of node
            foreach (var child in GetEffectiveChildren(node))
            {
                // if the child node is a direct artifact
                if (child.getArtifact() is Artifact artifact)
                    if (GetOrCreateIkvmReferenceItemForArtifact(output, artifact) is IkvmReferenceItem reference)
                        yield return reference;

                // recurse into child
                foreach (var reference in CollectIkvmReferenceItemReferences(output, child, processed))
                    yield return reference;
            }
        }

        /// <summary>
        /// Gets the <see cref="IkvmReferenceItem"/> associated with the given artifact.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="artifact"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IkvmReferenceItem GetOrCreateIkvmReferenceItemForArtifact(Dictionary<string, IkvmReferenceItem> output, Artifact artifact)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (artifact is null)
                throw new ArgumentNullException(nameof(artifact));

            // we only process JAR artifacts
            var extension = artifact.getExtension();
            if (extension != "jar")
                return null;

            var ikvmItem = GetIkvmReferenceItemForArtifact(output, artifact);
            if (ikvmItem != null)
                return ikvmItem;

            // pull items out of artifact
            var groupId = artifact.getGroupId();
            var artifactId = artifact.getArtifactId();
            var classifier = artifact.getClassifier();
            var version = artifact.getVersion();
            var ikvmItemSpec = GetIkvmItemSpec(groupId, artifactId, classifier, version);

            // create a new item
            ikvmItem = new IkvmReferenceItem() { ItemSpec = ikvmItemSpec, ReferenceOutputAssembly = false, Private = false };
            output.Add(ikvmItemSpec, ikvmItem);

            // ensure output item has Maven information attached to it
            ikvmItem.MavenGroupId = groupId;
            ikvmItem.MavenArtifactId = artifactId;
            ikvmItem.MavenClassifier = classifier;
            ikvmItem.MavenVersion = version;

            // fallback to the Maven name and version if IKVM cannot detect otherwise
            ikvmItem.FallbackAssemblyName = artifactId;
            ikvmItem.FallbackAssemblyVersion = ToAssemblyVersion(version)?.ToString();

            // inherit global settings
            ikvmItem.Debug = Debug;
            ikvmItem.KeyFile = KeyFile;

            // setup the class loader
            ikvmItem.ClassLoader = ClassLoader;

            // if the artifact is a jar, we need to associate the path to the jar to the item
            var file = artifact.getFile();
            var filePath = file.getAbsolutePath();
            if (ikvmItem.Compile.Contains(filePath) == false)
                ikvmItem.Compile.Add(filePath);

            return ikvmItem;
        }

        /// <summary>
        /// Gets the <see cref="IkvmReferenceItem"/> associated with the given artifact.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="artifact"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IkvmReferenceItem GetIkvmReferenceItemForArtifact(IReadOnlyDictionary<string, IkvmReferenceItem> output, Artifact artifact)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));
            if (artifact is null)
                throw new ArgumentNullException(nameof(artifact));

            // we only process JAR artifacts
            var extension = artifact.getExtension();
            if (extension != "jar")
                return null;

            // pull items out of artifact
            var groupId = artifact.getGroupId();
            var artifactId = artifact.getArtifactId();
            var classifier = artifact.getClassifier();
            var version = artifact.getVersion();

            // find or create the IkvmReferenceItem for the artifact
            var ikvmItemSpec = GetIkvmItemSpec(groupId, artifactId, classifier, version);
            if (output.TryGetValue(ikvmItemSpec, out var ikvmItem))
                return ikvmItem;

            return null;
        }

        /// <summary>
        /// Gets the effective node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        DependencyNode GetEffectiveNode(DependencyNode node)
        {
            var n = (DependencyNode)node.getData().getOrDefault(ConflictResolver.NODE_DATA_WINNER, node);
            return n == node ? n : GetEffectiveNode(n);
        }

        /// <summary>
        /// Gets the effective children of a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        IEnumerable<DependencyNode> GetEffectiveChildren(DependencyNode node) => ((IEnumerable)GetEffectiveNode(node).getChildren()).Cast<DependencyNode>().Select(GetEffectiveNode);

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
                var major = Math.Min(v.getMajorVersion(), ushort.MaxValue);
                var minor = Math.Min(v.getMinorVersion(), ushort.MaxValue);
                return new Version(major, minor);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }

}
