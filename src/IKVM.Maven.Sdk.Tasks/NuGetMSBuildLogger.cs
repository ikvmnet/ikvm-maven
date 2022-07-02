using System;

using Microsoft.Build.Utilities;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Implementation of <see cref="INuGetLogger"/> that sends logs to MSBuild.
    /// </summary>
    class NuGetMSBuildLogger : MarshalByRefObject, INuGetLogger
    {

        readonly TaskLoggingHelper log;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NuGetMSBuildLogger(TaskLoggingHelper log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void LogVerbose(string message)
        {
            log.LogMessage(message, null);
        }

        public void LogDebug(string message)
        {
            log.LogMessage(message, null);
        }

        public void LogMinimal(string message)
        {
            log.LogMessage(message, null);
        }

        public void LogInformation(string message)
        {
            log.LogMessage(message, null);
        }

        public void LogWarning(string message)
        {
            log.LogWarning(message, null);
        }

        public void LogError(string message)
        {
            log.LogError(message, null);
        }
    }

}
