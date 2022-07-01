namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Interface that describes an interceptor for NuGet logging.
    /// </summary>
    interface INuGetLogger
    {

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogVerbose(string message);

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogDebug(string message);

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogMinimal(string message);

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogInformation(string message);

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogWarning(string message);

        /// <summary>
        /// Logs the given message.
        /// </summary>
        /// <param name="message"></param>
        void LogError(string message);

    }

}
