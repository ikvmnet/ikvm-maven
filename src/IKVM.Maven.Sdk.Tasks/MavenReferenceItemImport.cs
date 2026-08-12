using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using IKVM.Maven.Sdk.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.apache.maven.model.io;
using org.eclipse.aether.util.artifact;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Imports <see cref="MavenReferenceItem"/> from package assets.
    /// </summary>
    public class MavenReferenceItemImport : Task
    {

        /// <summary>
        /// Disposable wrapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class DisposableValue<T> : IDisposable
        {

            readonly T value;
            readonly Action dispose;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="dispose"></param>
            public DisposableValue(T value, Action dispose)
            {
                this.value = value;
                this.dispose = dispose;
            }

            /// <summary>
            /// Gets the disposable value.
            /// </summary>
            public T Value => value;

            /// <summary>
            /// Disposes of the instance.
            /// </summary>
            public void Dispose()
            {
                dispose?.Invoke();
            }

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemImport() :
            base(SR.ResourceManager, "MAVEN:")
        {

        }

        /// <summary>
        /// Set of MavenReferenceItem
        /// </summary>
        [Required]
        public string AssetsFilePath { get; set; }

        /// <summary>
        /// Target framework of current build.
        /// </summary>
        [Required]
        public string TargetFramework { get; set; }

        /// <summary>
        /// Runtime identifier of current build.
        /// </summary>
        public string RuntimeIdentifier { get; set; }

        /// <summary>
        /// <see cref="MavenReferenceItem"/> instances imported from packages.
        /// </summary>
        [Output]
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            var items = new List<MavenReferenceItem>(8);

            try
            {
                // integrate each discovered POM
                foreach (var pom in GetProjectObjectModelFiles(AssetsFilePath, TargetFramework, RuntimeIdentifier))
                    foreach (var item in GetProjectObjectModelFileReferences(pom))
                        items.Add(item);

                // output final list of new dependencies
                Items = items.Select(ToTaskItem).ToArray();
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        /// <summary>
        /// Persists the item to a task item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ITaskItem ToTaskItem(MavenReferenceItem item)
        {
            var task = new TaskItem();
            MavenReferenceItemMetadata.Save(item, task);
            return task;
        }

        /// <summary>
        /// Gets the available Maven project model files.
        /// </summary>
        /// <param name="assetsFilePath"></param>
        /// <param name="targetFramework"></param>
        /// <param name="runtimeIdentifier"></param>
        /// <returns></returns>
        internal List<string> GetProjectObjectModelFiles(string assetsFilePath, string targetFramework, string runtimeIdentifier)
        {
            using var api = GetNuGetApi();
            return api.Value.GetProjectObjectModelFiles(assetsFilePath, targetFramework, runtimeIdentifier, new NuGetMSBuildLogger(Log));
        }

        /// <summary>
        /// Gets an instance of the <see cref="NuGetApi"/>. On Framework this method returns a remote reference to an
        /// isolated AppDomain so as to not conflict with locally loaded versions of NuGet.
        /// </summary>
        /// <returns></returns>
        static DisposableValue<NuGetApi> GetNuGetApi()
        {
            return new DisposableValue<NuGetApi>(new NuGetApi(), null);
        }

        /// <summary>
        /// Converts a <see cref="Dependency"/> to a <see cref="MavenReferenceItem"/>.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        internal static MavenReferenceItem GetMavenReferenceItem(Dependency dependency)
        {
            if (dependency is null)
                throw new ArgumentNullException(nameof(dependency));

            var itemSpec = $"{dependency.getGroupId()}:{dependency.getArtifactId()}";
            var item = new MavenReferenceItem();
            item.ItemSpec = itemSpec;
            item.GroupId = dependency.getGroupId();
            item.ArtifactId = dependency.getArtifactId();
            item.Classifier = dependency.getClassifier();
            item.Version = dependency.getVersion();
            item.Scope = dependency.getScope();
            return item;
        }

        /// <summary>
        /// Extracts the <see cref="MavenReferenceItem"/> instances described by the given path to a POM file,
        /// including any dependencies declared through the IKVM POM extensions.
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static IEnumerable<MavenReferenceItem> GetProjectObjectModelFileReferences(string pom)
        {
            if (pom is null)
                throw new ArgumentNullException(nameof(pom));

            // file doesn't actually exist
            if (File.Exists(pom) == false)
                yield break;

            // load the POM file and separate the extension content from the model content
            var doc = XDocument.Load(pom);
            var additions = ExtractItemDependencies(doc);

            // read the cleaned POM file
            var reader = new DefaultModelReader();
            var model = reader.read(new java.io.StringReader(doc.ToString()), null);

            // extract dependencies from model
            foreach (Dependency dependency in (IEnumerable)model.getDependencies())
            {
                var item = GetMavenReferenceItem(dependency);
                if (additions.TryGetValue((item.GroupId, item.ArtifactId), out var l))
                    item.Dependencies = l.ToArray();

                yield return item;
            }
        }

        /// <summary>
        /// Extracts the dependencies declared through the IKVM POM extensions from the given document, keyed by the
        /// coordinates of the dependency node upon which they are declared, and removes the extension content so the
        /// remaining document forms a standard POM.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        internal static Dictionary<(string GroupId, string ArtifactId), List<MavenReferenceItemDependency>> ExtractItemDependencies(XDocument doc)
        {
            var ext = MavenWriteProjectObjectModelFile.PomExtNamespace;
            var pom = doc.Root.Name.Namespace;
            var additions = new Dictionary<(string, string), List<MavenReferenceItemDependency>>();

            // walk each dependency node carrying extension content
            foreach (var dependency in doc.Root.Element(pom + "dependencies")?.Elements(pom + "dependency") ?? Enumerable.Empty<XElement>())
            {
                var container = dependency.Element(ext + "dependencies");
                if (container == null)
                    continue;

                var groupId = (string)dependency.Element(pom + "groupId");
                var artifactId = (string)dependency.Element(pom + "artifactId");
                if (additions.TryGetValue((groupId, artifactId), out var l) == false)
                    additions[(groupId, artifactId)] = l = new List<MavenReferenceItemDependency>();

                CollectItemDependencies(container, pom, new List<MavenReferenceItemDependencySelector>(), l);
            }

            // remove all extension content so the model parses strictly
            doc.Descendants().Where(i => i.Name.Namespace == ext).Remove();
            doc.Root.Attributes().Where(i => i.IsNamespaceDeclaration && i.Value == ext.NamespaceName).Remove();

            return additions;
        }

        /// <summary>
        /// Recursively collects the dependencies declared within the given extension container. A dependency node
        /// itself containing extension content acts as a path selector; a leaf dependency node is a declared
        /// dependency.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pom"></param>
        /// <param name="path"></param>
        /// <param name="output"></param>
        static void CollectItemDependencies(XElement container, XNamespace pom, List<MavenReferenceItemDependencySelector> path, List<MavenReferenceItemDependency> output)
        {
            var ext = MavenWriteProjectObjectModelFile.PomExtNamespace;

            foreach (var element in container.Elements(pom + "dependency"))
            {
                var groupId = (string)element.Element(pom + "groupId");
                var artifactId = (string)element.Element(pom + "artifactId");
                var version = (string)element.Element(pom + "version");

                var nested = element.Element(ext + "dependencies");
                if (nested != null)
                {
                    path.Add(new MavenReferenceItemDependencySelector(groupId, artifactId, version));
                    CollectItemDependencies(nested, pom, path, output);
                    path.RemoveAt(path.Count - 1);
                }
                else
                {
                    output.Add(new MavenReferenceItemDependency()
                    {
                        Path = path.ToArray(),
                        GroupId = groupId,
                        ArtifactId = artifactId,
                        Version = version,
                        Scope = (string)element.Element(pom + "scope") ?? JavaScopes.COMPILE,
                        Optional = string.Equals((string)element.Element(pom + "optional"), "true", StringComparison.OrdinalIgnoreCase),
                    });
                }
            }
        }

    }

}
