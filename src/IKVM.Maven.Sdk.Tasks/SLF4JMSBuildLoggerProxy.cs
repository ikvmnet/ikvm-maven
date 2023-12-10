using System;
using System.Runtime.CompilerServices;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.slf4j;
using org.slf4j.@event;
using org.slf4j.helpers;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Proxies SLF4J log events to MSBuild.
    /// </summary>
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
        readonly Level level;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public SLF4JMSBuildLoggerProxy(TaskLoggingHelper log, org.slf4j.@event.Level level)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.level = level;
        }

        public void trace(SLF4JContextLogger logger, string message)
        {
            if (isTraceEnabled(logger))
            log.LogMessageFromText("SLF4J [TRACE]: " + message, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, object obj)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, string str, Exception t)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + str, MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            if (isTraceEnabled(logger))
            log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void trace(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            if (isTraceEnabled(logger))
                log.LogMessageFromText("SLF4J [TRACE]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string message)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, object obj)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, string str, Exception t)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + str, MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void debug(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            if (isDebugEnabled(logger))
                log.LogMessageFromText("SLF4J [DEBUG]: " + str + ": " + t?.Message, MessageImportance.Low);
        }
        public void info(SLF4JContextLogger logger, string message)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + message, MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, string str, object obj)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, string str, Exception t)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + str, MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void info(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            if (isInfoEnabled(logger))
                log.LogMessageFromText("SLF4J [INFO]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, string message)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + message, MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, string str, object obj)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, string str, Exception t)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + str, MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void warn(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            if (isWarnEnabled(logger))
                log.LogMessageFromText("SLF4J [WARN]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, string message)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + message, MessageImportance.Normal);
        }

        public void error(SLF4JContextLogger logger, string str, object obj)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, string str, object obj1, object obj2)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, string str, params object[] objarr)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, string str, Exception t)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + str, MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, object obj)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.format(str, obj).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.format(str, obj1, obj2).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, params object[] objarr)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + MessageFormatter.arrayFormat(str, objarr).getMessage(), MessageImportance.Low);
        }

        public void error(SLF4JContextLogger logger, Marker m, string str, Exception t)
        {
            if (isErrorEnabled(logger))
                log.LogMessageFromText("SLF4J [ERROR]: " + str + ": " + t?.Message, MessageImportance.Low);
        }

        public bool isTraceEnabled(SLF4JContextLogger logger)
        {
            return level.toInt() <= Level.TRACE.toInt();
        }

        public bool isTraceEnabled(SLF4JContextLogger logger, Marker m)
        {
            return level.toInt() <= Level.TRACE.toInt();
        }

        public bool isDebugEnabled(SLF4JContextLogger logger)
        {
            return level.toInt() <= Level.DEBUG.toInt();
        }

        public bool isDebugEnabled(SLF4JContextLogger logger, Marker m)
        {
            return level.toInt() <= Level.DEBUG.toInt();
        }

        public bool isInfoEnabled(SLF4JContextLogger logger)
        {
            return level.toInt() <= Level.INFO.toInt();
        }

        public bool isInfoEnabled(SLF4JContextLogger logger, Marker m)
        {
            return level.toInt() <= Level.INFO.toInt();
        }

        public bool isWarnEnabled(SLF4JContextLogger logger)
        {
            return level.toInt() <= Level.WARN.toInt();
        }

        public bool isWarnEnabled(SLF4JContextLogger logger, Marker m)
        {
            return level.toInt() <= Level.WARN.toInt();
        }

        public bool isErrorEnabled(SLF4JContextLogger logger)
        {
            return level.toInt() <= Level.ERROR.toInt();
        }

        public bool isErrorEnabled(SLF4JContextLogger logger, Marker m)
        {
            return level.toInt() <= Level.ERROR.toInt();
        }

    }

}
