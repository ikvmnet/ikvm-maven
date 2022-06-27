using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectModel;

using org.apache.maven.model;
using org.apache.maven.model.io;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Imports <see cref="MavenReferenceItem"/> from package assets.
    /// </summary>
    public class MavenReferenceItemImport : Task
    {
        /// <summary>
        /// Forces an exception on errors reading the lock file.
        /// </summary>
        sealed class ThrowOnLockFileLoadError : LoggerBase
        {

            readonly TaskLoggingHelper _log;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="log"></param>
            public ThrowOnLockFileLoadError(TaskLoggingHelper log)
            {
                _log = log ?? throw new ArgumentNullException(nameof(log));
            }

            public override void Log(ILogMessage message)
            {
                if (message.Level == LogLevel.Error)
                    _log.LogWarning(message.FormatWithCode(), null, null, message.ProjectPath);
                else
                    _log.LogMessage(message.FormatWithCode(), null, null, message.ProjectPath);
            }

            public override System.Threading.Tasks.Task LogAsync(ILogMessage message)
            {
                Log(message);
                return System.Threading.Tasks.Task.CompletedTask;
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
                var lockFile = LoadLockFile(AssetsFilePath, new ThrowOnLockFileLoadError(Log));
                if (lockFile == null)
                    throw new MavenTaskException("Unable to open assets file.");

                // integrate each discovered POM
                foreach (var pom in GetProjectObjectModelFiles(lockFile, TargetFramework, RuntimeIdentifier))
                    foreach (var dependency in GetProjectObjectModelFileDependencies(pom))
                        items.Add(GetMavenReferenceItem(dependency));

                // output final list of new dependencies
                Items = items.Select(i => i.Item).ToArray();
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
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
            var item = new MavenReferenceItem(new TaskItem(itemSpec));
            item.ItemSpec = itemSpec;
            item.GroupId = dependency.getGroupId();
            item.ArtifactId = dependency.getArtifactId();
            item.Classifier = dependency.getClassifier();
            item.Version = dependency.getVersion();
            item.Save();
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

            // read POM file
            var reader = new DefaultModelReader();
            var model = reader.read(new java.io.File(pom), null);

            // extract dependencies from model
            foreach (Dependency dependency in (IEnumerable)model.getDependencies())
                yield return dependency;
        }

        /// <summary>
        /// Probes the lock file for POM files given a target framework and runtime identifier.
        /// </summary>
        /// <param name="lockFile"></param>
        /// <param name="targetFramework"></param>
        /// <param name="runtimeIdentifier"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        internal static IEnumerable<string> GetProjectObjectModelFiles(LockFile lockFile, string targetFramework, string runtimeIdentifier)
        {
            // start with the target being built
            var target = lockFile.GetTarget(targetFramework, null);
            if (target == null && lockFile.PackageSpec.TargetFrameworks.All(tfi => string.IsNullOrEmpty(tfi.TargetAlias)))
                target = lockFile.GetTarget(NuGetFramework.Parse(targetFramework), null);
            if (target == null)
                yield break;

            // for each in-scope library
            foreach (var library in target.Libraries)
            {
                // must be a dependency library
                if (library.Type != "package")
                    continue;

                // target library should exist in libraries
                var lib = lockFile.GetLibrary(library.Name, library.Version);
                if (lib == null)
                    continue;

                // group the available POM files by TFM
                var pomPathsByTfm = lib.Files
                    .Where(i => i.StartsWith("maven/") && i.EndsWith($"/{library.Name}.pom"))
                    .Select(i => new { Segments = i.Split('/'), File = i }).Where(i => i.Segments.Length == 3)
                    .Select(i => new { Folder = i.Segments[1], Path = i.File.Replace('/', Path.DirectorySeparatorChar) })
                    .GroupBy(i => i.Folder)
                    .Select(i => new FrameworkSpecificGroup(NuGetFramework.ParseFolder(i.Key), i.Select(j => j.Path).ToList()))
                    .ToList();

                // find the group of POMs for the TFM
                var compatibleGroup = NuGetFrameworkUtility.GetNearest(pomPathsByTfm, NuGetFramework.Parse(targetFramework));
                if (compatibleGroup == null)
                    continue;

                // integrate each discovered POM
                foreach (var pom in compatibleGroup.Items)
                    foreach (var pkgDir in lockFile.PackageFolders)
                        if (Path.Combine(pkgDir.Path, lib.Path.Replace('/', Path.DirectorySeparatorChar), pom) is string pomPath && File.Exists(pomPath))
                            yield return pomPath;
            }
        }

        /// <summary>
        /// Attempts to load the given lock file path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        internal static LockFile LoadLockFile(string path, NuGet.Common.ILogger logger)
        {
            LockFile lockFile;

            try
            {
                lockFile = LockFileUtilities.GetLockFile(path, logger);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new MavenTaskException("Error reading assets file.", ex);
            }

            if (lockFile == null)
                throw new MavenTaskException("Assets file not found.");

            return lockFile;
        }

    }

}
