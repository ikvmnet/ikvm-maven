using System;
using System.Runtime.CompilerServices;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.slf4j;
using org.slf4j.helpers;

namespace IKVM.Maven.Sdk.Tasks
{

    class SLF4JMSBuildLoggerProxy : ISLF4JLoggerProxy
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static SLF4JMSBuildLoggerProxy()
        {
            RuntimeHelpers.RunClassConstructor(typeof(SLF4JContextLogger).TypeHandle);
        }

        readonly TaskLoggingHelper log;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public SLF4JMSBuildLoggerProxy(TaskLoggingHelper log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void trace(SLF4JContextLogger logger, string message)
        {
            log.LogMessageFromText(message, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str)
        {
            log.LogMessageFromText(str, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string message)
        {
            log.LogMessageFromText(message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str)
        {
            log.LogMessageFromText(str, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Low);
        }
        public void info(SLF4JContextLogger logger, string message)
        {
            log.LogMessageFromText(message, MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str)
        {
            log.LogMessageFromText(str, MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            log.LogMessageFromText(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            log.LogMessageFromText(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            log.LogMessageFromText(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, string message)
        {
            log.LogWarning(message, MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, string str, object obj)
        {
            log.LogWarning(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            log.LogWarning(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            log.LogWarning(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, string str, Exception t)
        {
            log.LogWarning(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str)
        {
            log.LogWarning(str, MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            log.LogWarning(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            log.LogWarning(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            log.LogWarning(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            log.LogWarning(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string message)
        {
            log.LogError(message, MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string str, object obj)
        {
            log.LogError(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            log.LogError(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            log.LogError(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string str, Exception t)
        {
            log.LogError(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str)
        {
            log.LogError(str, MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            log.LogError(MessageFormatter.format(str, obj).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            log.LogError(MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            log.LogError(MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            log.LogError(str + ": " + t?.Message, MessageImportance.Normal);
        }

        public bool isDebugEnabled(SLF4JContextLogger logger)
        {
            return true;
        }

        public bool isDebugEnabled(SLF4JContextLogger logger, Marker m)
        {
            return true;
        }

        public bool isTraceEnabled(SLF4JContextLogger logger)
        {
            return true;
        }

        public bool isTraceEnabled(SLF4JContextLogger logger, Marker m)
        {
            return true;
        }

        public bool isInfoEnabled(SLF4JContextLogger logger)
        {
            return true;
        }

        public bool isInfoEnabled(SLF4JContextLogger logger, Marker m)
        {
            return true;
        }

        public bool isWarnEnabled(SLF4JContextLogger logger)
        {
            return true;
        }

        public bool isWarnEnabled(SLF4JContextLogger logger, Marker m)
        {
            return true;
        }

        public bool isErrorEnabled(SLF4JContextLogger logger)
        {
            return true;
        }

        public bool isErrorEnabled(SLF4JContextLogger logger, Marker m)
        {
            return true;
        }

    }

}
