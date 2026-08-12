using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Accepts information about the current project and builds a POM file.
    /// </summary>
    public class MavenWriteProjectObjectModelFile : Task
    {

        /// <summary>
        /// Namespace of the IKVM POM extensions.
        /// </summary>
        internal static readonly XNamespace PomExtNamespace = "http://ikvm.org/POM-EXT/1.0.0";

        /// <summary>
        /// <see cref="StringWriter"/> that reports UTF-8 as its encoding.
        /// </summary>
        class Utf8StringWriter : StringWriter
        {

            /// <inheritdoc />
            public override Encoding Encoding => Encoding.UTF8;

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenWriteProjectObjectModelFile() :
            base(SR.ResourceManager, "MAVEN:")
        {

        }

        /// <summary>
        /// Group ID of the project.
        /// </summary>
        [Required]
        public string GroupId { get; set; }

        /// <summary>
        /// Artifact ID of the project.
        /// </summary>
        [Required]
        public string ArtifactId { get; set; }

        /// <summary>
        /// Version of the project.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// Set of MavenReferenceItem that form the dependencies.
        /// </summary>
        [Required]
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// File to write the output to.
        /// </summary>
        [Required]
        public string ProjectFile { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                var items = MavenReferenceItemMetadata.Import(References);

                var wrt = new java.io.StringWriter();
                var pom = new Model();
                pom.setGroupId(GroupId);
                pom.setArtifactId(ArtifactId);
                pom.setVersion(Version);

                // add dependencies
                foreach (var item in items)
                    foreach (var dependency in ItemToDependencies(item))
                        pom.addDependency(dependency);

                // output to string
                new DefaultModelWriter().write(wrt, null, pom);
                var txt = wrt.toString();

                // nest dependencies declared upon references within their dependency nodes
                if (items.Any(i => i.Dependencies != null && i.Dependencies.Length > 0))
                    txt = AddItemDependencies(txt, items);

                // if the file already exists and matches, just return success
                if (File.Exists(ProjectFile))
                    if (File.ReadAllText(ProjectFile, Encoding.UTF8) == txt)
                        return true;

                // replace file
                File.WriteAllText(ProjectFile, txt, Encoding.UTF8);
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        /// <summary>
        /// Generates a <see cref="Dependency"/> record for the given <see cref="MavenReferenceItem"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<Dependency> ItemToDependencies(MavenReferenceItem item)
        {
            var dependency = new Dependency();
            dependency.setGroupId(item.GroupId);
            dependency.setArtifactId(item.ArtifactId);
            dependency.setClassifier(item.Classifier);
            dependency.setVersion(item.Version);
            dependency.setOptional(item.Optional);
            dependency.setScope(item.Scope);
            yield return dependency;
        }

        /// <summary>
        /// Nests the dependencies declared upon each item within the corresponding dependency node of the given POM
        /// document.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        internal static string AddItemDependencies(string xml, IEnumerable<MavenReferenceItem> items)
        {
            var doc = XDocument.Parse(xml);
            var pom = doc.Root.Name.Namespace;

            // declare the extension namespace on the root so serialization uses the prefix
            if (doc.Root.Attribute(XNamespace.Xmlns + "ikvm") == null)
                doc.Root.Add(new XAttribute(XNamespace.Xmlns + "ikvm", PomExtNamespace));

            foreach (var item in items)
            {
                if (item.Dependencies == null || item.Dependencies.Length == 0)
                    continue;

                // locate the dependency node of the item
                var element = doc.Root.Element(pom + "dependencies")?.Elements(pom + "dependency")
                    .FirstOrDefault(i => (string)i.Element(pom + "groupId") == item.GroupId && (string)i.Element(pom + "artifactId") == item.ArtifactId);
                if (element == null)
                    continue;

                foreach (var dependency in item.Dependencies)
                    AddItemDependency(element, dependency);
            }

            using var wrt = new Utf8StringWriter();
            doc.Save(wrt);
            return wrt.ToString();
        }

        /// <summary>
        /// Nests the given dependency within the given dependency node, descending through the path.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="dependency"></param>
        static void AddItemDependency(XElement element, MavenReferenceItemDependency dependency)
        {
            var pom = element.Name.Namespace;

            // descend through the path, merging with existing selector nodes where possible
            foreach (var selector in dependency.Path)
            {
                var container = GetOrCreateDependenciesElement(element);
                var child = container.Elements(pom + "dependency").FirstOrDefault(i =>
                    (string)i.Element(pom + "groupId") == selector.GroupId &&
                    (string)i.Element(pom + "artifactId") == selector.ArtifactId &&
                    (string)i.Element(pom + "version") == selector.Version);
                if (child == null)
                {
                    child = new XElement(pom + "dependency",
                        new XElement(pom + "groupId", selector.GroupId),
                        new XElement(pom + "artifactId", selector.ArtifactId));
                    if (selector.Version != null)
                        child.Add(new XElement(pom + "version", selector.Version));
                    container.Add(child);
                }

                element = child;
            }

            // append the declared dependency itself
            var target = GetOrCreateDependenciesElement(element);
            var d = new XElement(pom + "dependency",
                new XElement(pom + "groupId", dependency.GroupId),
                new XElement(pom + "artifactId", dependency.ArtifactId),
                new XElement(pom + "version", dependency.Version));
            if (dependency.Extension != "jar")
                d.Add(new XElement(pom + "type", dependency.Extension));
            if (string.IsNullOrEmpty(dependency.Classifier) == false)
                d.Add(new XElement(pom + "classifier", dependency.Classifier));
            if (dependency.Scope != JavaScopes.COMPILE)
                d.Add(new XElement(pom + "scope", dependency.Scope));
            if (dependency.Optional)
                d.Add(new XElement(pom + "optional", "true"));
            target.Add(d);
        }

        /// <summary>
        /// Gets or creates the extension dependencies element of the given dependency node.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        static XElement GetOrCreateDependenciesElement(XElement element)
        {
            var container = element.Element(PomExtNamespace + "dependencies");
            if (container == null)
            {
                container = new XElement(PomExtNamespace + "dependencies");
                element.Add(container);
            }

            return container;
        }

    }

}
