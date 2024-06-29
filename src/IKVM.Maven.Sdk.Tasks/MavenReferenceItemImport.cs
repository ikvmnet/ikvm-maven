using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IKVM.Maven.Sdk.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.apache.maven.model.io;

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
                    foreach (var dependency in GetProjectObjectModelFileDependencies(pom))
                        items.Add(GetMavenReferenceItem(dependency));

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
        /// Extracts the <see cref="Dependency"/> nodes from the given path to a POM file.
        /// </summary>
        /// <param name="pom"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static IEnumerable<Dependency> GetProjectObjectModelFileDependencies(string pom)
        {
            if (pom is null)
                throw new ArgumentNullException(nameof(pom));

            // file doesn't actually exist
            if (File.Exists(pom) == false)
                yield break;

            // read POM file
            var reader = new DefaultModelReader();
            var model = reader.read(new java.io.File(pom), null);

            // extract dependencies from model
            foreach (Dependency dependency in (IEnumerable)model.getDependencies())
                yield return dependency;
        }

    }

}
