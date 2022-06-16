using System;

using IKVM.Sdk.Maven.Tasks.Resources;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Maven task exception with error message.
    /// </summary>
    class MavenTaskMessageException : MavenTaskException
    {

        readonly string messageResourceName;
        readonly object[] messageArgs;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="messageResourceName"></param>
        /// <param name="messageArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MavenTaskMessageException(string messageResourceName, params object[] messageArgs) :
            base(string.Format(SR.ResourceManager.GetString(messageResourceName), messageArgs))
        {
            this.messageResourceName = messageResourceName ?? throw new ArgumentNullException(nameof(messageResourceName));
            this.messageArgs = messageArgs ?? throw new ArgumentNullException(nameof(messageArgs));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="innerException"></param>
        /// <param name="messageResourceName"></param>
        /// <param name="messageArgs"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MavenTaskMessageException(Exception innerException, string messageResourceName, params object[] messageArgs) :
            base(string.Format(SR.ResourceManager.GetString(messageResourceName), messageArgs), innerException)
        {
            this.messageResourceName = messageResourceName ?? throw new ArgumentNullException(nameof(messageResourceName));
            this.messageArgs = messageArgs ?? throw new ArgumentNullException(nameof(messageArgs));
        }

        /// <summary>
        /// Gets the resource name of the message.
        /// </summary>
        public string MessageResourceName => messageResourceName;

        /// <summary>
        /// Gets the arguments of the message.
        /// </summary>
        public object[] MessageArgs => messageArgs;

    }

}