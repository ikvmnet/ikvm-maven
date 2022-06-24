
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using NuGet.Common;
using NuGet.ProjectModel;

using org.apache.maven.model;
using org.apache.maven.project;

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
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                var lockFile = LoadLockFile(AssetsFilePath);
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

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
