using System.Collections.Generic;
using System.IO;
using System.Text;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.apache.maven.model.io;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Accepts information about the current project and builds a POM file.
    /// </summary>
    public class MavenWriteProjectObjectModelFile : Task
    {

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
                var wrt = new java.io.StringWriter();
                var pom = new Model();
                pom.setGroupId(GroupId);
                pom.setArtifactId(ArtifactId);
                pom.setVersion(Version);

                // add dependencies
                foreach (var item in MavenReferenceItemUtil.Import(References))
                    foreach (var dependency in ItemToDependencies(item))
                        pom.addDependency(dependency);

                // output to string
                new DefaultModelWriter().write(wrt, null, pom);
                var txt = wrt.ToString();

                // if the file already exists and matches, just return success
                if (File.Exists(ProjectFile))
                    if (File.ReadAllText(ProjectFile, Encoding.UTF8) == txt)
                        return true;

                // replace file
                File.WriteAllText(ProjectFile, wrt.toString(), Encoding.UTF8);
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

    }

}
