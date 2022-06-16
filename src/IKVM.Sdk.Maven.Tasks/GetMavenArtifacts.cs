using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using java.lang;
using java.util;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.repository.@internal;
using org.apache.maven.settings;
using org.apache.maven.settings.building;
using org.apache.maven.settings.crypto;
using org.eclipse.aether;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.connector.basic;
using org.eclipse.aether.graph;
using org.eclipse.aether.impl;
using org.eclipse.aether.repository;
using org.eclipse.aether.resolution;
using org.eclipse.aether.spi.connector;
using org.eclipse.aether.spi.connector.transport;
using org.eclipse.aether.transport.file;
using org.eclipse.aether.transport.http;
using org.eclipse.aether.util.artifact;
using org.eclipse.aether.util.filter;
using org.eclipse.aether.util.graph.transformer;
using org.eclipse.aether.util.repository;
using org.sonatype.plexus.components.cipher;
using org.sonatype.plexus.components.sec.dispatcher;

namespace IKVM.Sdk.Maven.Tasks
{
    /// <summary>
    /// Retrieves Maven artifacts from a repository, downloads them, and returns a list of the downloaded artifacts.
    /// </summary>
    public class GetMavenArtifacts : Task
    {
        /// <summary>
        /// The MavenReference items to process.
        /// </summary>
        [Required]
        public ITaskItem[] MavenReferences { get; set; }

        /// <summary>
        /// The output directory to move the artifacts to.
        /// </summary>
        [Required]
        public string DestinationFolder { get; set; }

        /// <summary>
        /// Overwrite the file if it doesn't exist. If left <c>false</c> (the default),
        /// the 
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets the scope to use, which will be set on the classpath. Defaults to "compile" if not provided.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// If set to <c>true</c>, fetch only the metadata not the actual artifacts.
        /// </summary>
        public bool NoDownload { get; set; }

        /// <summary>
        /// The list of artifacts that were requested.
        /// </summary>
        [Output]
        public ITaskItem[] MavenArtifacts { get; set;}

        /// <summary>
        /// Executes this task.
        /// </summary>
        /// <returns><c>true</c> if the task was successful; otherwise <c>false</c>.</returns>
        public override bool Execute()
        {
            try
            {
                // Setup Maven configuration
                var container = new IkvmMavenContainer(
                    systemProperties: new System.Collections.Generic.Dictionary<string, string>(),
                    userProperties: new System.Collections.Generic.Dictionary<string, string>());

                var scope = string.IsNullOrWhiteSpace(Scope) ? JavaScopes.COMPILE : Scope;

                // Get dependencies list
                List depedencies = new java.util.ArrayList();
                foreach (MavenReferenceItem mavenReference in IkvmMavenReferenceItemUtil.Import(MavenReferences))
                {
                    depedencies.add(new Dependency(new DefaultArtifact(
                        mavenReference.GroupId,
                        mavenReference.ArtifactId,
                        mavenReference.Classifier,
                        mavenReference.Extension,
                        mavenReference.Version),
                        scope));
                }

                var classpathFilter = DependencyFilterUtils.classpathFilter(scope);
                var collectRequest = new CollectRequest()
                    .setRepositories(container.Repositories)
                    .setDependencies(depedencies);
                var dependencyRequest = new DependencyRequest(collectRequest, classpathFilter);
                var artifactResults = container.System.resolveDependencies(container.Session, dependencyRequest).getArtifactResults();

                var mavenArtifactItems = new System.Collections.Generic.List<MavenArtifactItem>();

                foreach (ArtifactResult artifactResult in (IEnumerable)artifactResults)
                {
                    var artifact = artifactResult.getArtifact();
                    var mavenArtifact = new MavenArtifactItem(artifact, DestinationFolder, artifactResult.getRequest().getDependencyNode());
                    mavenArtifact.Save();
                    mavenArtifactItems.Add(mavenArtifact);

                    string source = artifact.getFile().getCanonicalPath();
                    string destination = mavenArtifact.FilePath;

                    Trace.WriteLine(string.Format("{0} resolved to {1}", artifact, destination));

                    if (!NoDownload)
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destination));
                        if (System.IO.File.Exists(destination))
                        {
                            Trace.WriteLine($"Skipping copying artifact '{source}' to '{destination}' because it already exists.");
                        }
                        else
                        {
                            Trace.WriteLine($"Copying artifact '{source}' to '{destination}'.");
                            System.IO.File.Copy(source, destination);
                        }
                    }
                }

                MavenArtifacts = mavenArtifactItems.Select(i => i.Item).ToArray();

                return true;
            }
            catch (Throwable e)
            {
                Log.LogErrorFromException(e);
                return false;
            }
        }
    }
}
