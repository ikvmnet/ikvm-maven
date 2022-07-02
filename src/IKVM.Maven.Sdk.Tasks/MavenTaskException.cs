
using System;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Generic Maven task exception.
    /// </summary>
    class MavenTaskException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        public MavenTaskException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MavenTaskException(string message, Exception innerException) :
            base(message, innerException)
        {

        }

    }

}