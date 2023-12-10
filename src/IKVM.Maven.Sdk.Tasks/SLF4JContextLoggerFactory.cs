using IKVM.Maven.Sdk.Tasks.Java;

using org.slf4j;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// SLF4J LoggerFactory implementation.
    /// </summary>
    class SLF4JContextLoggerFactory : org.slf4j.ILoggerFactory
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static SLF4JContextLoggerFactory()
        {
            AdapterLoggerFactory.LoggerFactory = new SLF4JContextLoggerFactory();
        }

        public Logger getLogger(string name)
        {
            return new SLF4JContextLogger(name);
        }

    }

}
