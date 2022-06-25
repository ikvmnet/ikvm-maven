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
using NuGet.Packaging.Core;
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
        [Required]
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
                var tfm = NuGetFramework.Parse(TargetFramework);
                if (tfm == null)
                    throw new MavenTaskException("Unable to parse TargetFramework.");

                var lockFile = LoadLockFile(AssetsFilePath);
                if (lockFile == null)
                    throw new MavenTaskException("Unable to open assets file.");

                var target = lockFile.GetTarget(TargetFramework, RuntimeIdentifier);
                if (target == null)
                    throw new MavenTaskException("Unable to get targets for framework.");

                // only consider POMs from targets that are relevant
                foreach (var library in target.Libraries)
                {
                    // must be a dependency library
                    if (library.PackageType.Contains(PackageType.Dependency) == false)
                        continue;

                    var lib = lockFile.GetLibrary(library.Name, library.Version);
                    if (lib == null)
                        continue;

                    // group the available POM files by TFM
                    var groups = lib.Files
                        .Where(i => i.StartsWith("maven/"))
                        .Where(i => i.EndsWith($"/{library.Name}.pom"))
                        .Select(i => new { Segments = i.Split('/'), File = i })
                        .Where(i => i.Segments.Length == 3)
                        .Select(i => new { Folder = i.Segments[1], File = i.File })
                        .GroupBy(i => i.Folder)
                        .Select(i => new FrameworkSpecificGroup(NuGetFramework.ParseFolder(i.Key), i.Select(j => j.File).ToList()))
                        .ToList();

                    // find the group of POMs for the TFM
                    var compatibleGroup = NuGetFrameworkUtility.GetNearest(groups, tfm);
                    if (compatibleGroup == null)
                        continue;

                    // integrate each discovered POM
                    foreach (var pom in compatibleGroup.Items)
                    {
                        // read POM file
                        var reader = new DefaultModelReader();
                        var model = reader.read(new java.io.File(pom), null);

                        // extract dependencies from model
                        foreach (Dependency dependency in (IEnumerable)model.getDependencies())
                        {
                            var item = new MavenReferenceItem(new TaskItem($"{dependency.getGroupId()}:{dependency.getArtifactId()}"));
                            item.GroupId = dependency.getGroupId();
                            item.ArtifactId = dependency.getArtifactId();
                            item.Classifier = dependency.getClassifier();
                            item.Version = dependency.getVersion();
                            item.Save();
                            items.Add(item);
                        }
                    }
                }

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
        /// Attempts to load the given lock file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        LockFile LoadLockFile(string path)
        {
            LockFile lockFile;

            try
            {
                lockFile = LockFileUtilities.GetLockFile(path, new ThrowOnLockFileLoadError(Log));
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
