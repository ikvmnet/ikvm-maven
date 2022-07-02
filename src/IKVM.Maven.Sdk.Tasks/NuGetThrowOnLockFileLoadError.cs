using System;

using NuGet.Common;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Forces an exception on errors reading the lock file.
    /// </summary>
    sealed class ThrowOnLockFileLoadError : LoggerBase
    {

        readonly INuGetLogger log;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public ThrowOnLockFileLoadError(INuGetLogger log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override void Log(ILogMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Verbose:
                    log.LogVerbose(message.FormatWithCode());
                    break;
                case LogLevel.Debug:
                    log.LogDebug(message.FormatWithCode());
                    break;
                case LogLevel.Minimal:
                    log.LogMinimal(message.FormatWithCode());
                    break;
                case LogLevel.Information:
                    log.LogInformation(message.FormatWithCode());
                    break;
                case LogLevel.Error:
                    log.LogError(message.FormatWithCode());
                    break;
                case LogLevel.Warning:
                    log.LogWarning(message.FormatWithCode());
                    break;
            }
        }

        public override System.Threading.Tasks.Task LogAsync(ILogMessage message)
        {
            Log(message);
            return System.Threading.Tasks.Task.CompletedTask;
        }

    }

}
