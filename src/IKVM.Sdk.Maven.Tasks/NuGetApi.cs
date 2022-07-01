using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectModel;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Provides statement methods for parsing the project assets file.
    /// </summary>
    class NuGetApi : MarshalByRefObject
    {

        /// <summary>
        /// Probes the asset file for POM files given a target framework and runtime identifier.
        /// </summary>
        /// <param name="assetsFilePath"></param>
        /// <param name="targetFramework"></param>
        /// <param name="runtimeIdentifier"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="MavenTaskException"></exception>
        public List<string> GetProjectObjectModelFiles(string assetsFilePath, string targetFramework, string runtimeIdentifier, INuGetLogger logger)
        {
            if (string.IsNullOrEmpty(assetsFilePath))
                throw new ArgumentException($"'{nameof(assetsFilePath)}' cannot be null or empty.", nameof(assetsFilePath));
            if (string.IsNullOrEmpty(targetFramework))
                throw new ArgumentException($"'{nameof(targetFramework)}' cannot be null or empty.", nameof(targetFramework));
            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            var lockFile = LoadLockFile(assetsFilePath, new ThrowOnLockFileLoadError(logger));
            if (lockFile == null)
                throw new MavenTaskException("Unable to open assets file.");

            return GetProjectObjectModelFiles(lockFile, targetFramework, runtimeIdentifier).ToList();
        }

        /// <summary>
        /// Probes the lock file for POM files given a target framework and runtime identifier.
        /// </summary>
        /// <param name="lockFile"></param>
        /// <param name="targetFramework"></param>
        /// <param name="runtimeIdentifier"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        internal IEnumerable<string> GetProjectObjectModelFiles(LockFile lockFile, string targetFramework, string runtimeIdentifier)
        {
            if (lockFile is null)
                throw new ArgumentNullException(nameof(lockFile));
            if (targetFramework is null)
                throw new ArgumentNullException(nameof(targetFramework));

            // start with the target being built
            var target = lockFile.GetTarget(NuGetFramework.Parse(targetFramework), null);
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
                        if (Path.Combine(pkgDir.Path, lib.Path.Replace('/', Path.DirectorySeparatorChar), pom) is string pomPath)
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
        internal LockFile LoadLockFile(string path, NuGet.Common.ILogger logger)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            if (File.Exists(path) == false)
                throw new FileNotFoundException("Could not find assets file.", path);

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
                throw new MavenTaskException("Could not read assets file.");

            return lockFile;
        }

    }

}
