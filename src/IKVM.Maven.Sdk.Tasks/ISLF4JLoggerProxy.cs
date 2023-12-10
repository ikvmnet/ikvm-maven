using System;

using org.slf4j;

namespace IKVM.Maven.Sdk.Tasks
{

    interface ISLF4JLoggerProxy
    {

        void trace(SLF4JContextLogger logger, string str);

        void trace(SLF4JContextLogger logger, string str, object obj);

        void trace(SLF4JContextLogger logger, string str, object obj1, object obj2);

        void trace(SLF4JContextLogger logger, string str, params object[] objarr);

        void trace(SLF4JContextLogger logger, string str, Exception t);

        void trace(SLF4JContextLogger logger, Marker m, string str);

        void trace(SLF4JContextLogger logger, Marker m, string str, object obj);

        void trace(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2);

        void trace(SLF4JContextLogger logger, Marker m, string str, params object[] objarr);

        void trace(SLF4JContextLogger logger, Marker m, string str, Exception t);

        void debug(SLF4JContextLogger logger, string str);

        void debug(SLF4JContextLogger logger, string str, object obj);

        void debug(SLF4JContextLogger logger, string str, object obj1, object obj2);

        void debug(SLF4JContextLogger logger, string str, params object[] objarr);

        void debug(SLF4JContextLogger logger, string str, Exception t);

        void debug(SLF4JContextLogger logger, Marker m, string str);

        void debug(SLF4JContextLogger logger, Marker m, string str, object obj);

        void debug(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2);

        void debug(SLF4JContextLogger logger, Marker m, string str, params object[] objarr);

        void debug(SLF4JContextLogger logger, Marker m, string str, Exception t);

        void info(SLF4JContextLogger logger, string str);

        void info(SLF4JContextLogger logger, string str, object obj);

        void info(SLF4JContextLogger logger, string str, object obj1, object obj2);

        void info(SLF4JContextLogger logger, string str, params object[] objarr);

        void info(SLF4JContextLogger logger, string str, Exception t);

        void info(SLF4JContextLogger logger, Marker m, string str);

        void info(SLF4JContextLogger logger, Marker m, string str, object obj);

        void info(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2);

        void info(SLF4JContextLogger logger, Marker m, string str, params object[] objarr);

        void info(SLF4JContextLogger logger, Marker m, string str, Exception t);

        bool isDebugEnabled(SLF4JContextLogger logger);

        bool isDebugEnabled(SLF4JContextLogger logger, Marker m);

        bool isErrorEnabled(SLF4JContextLogger logger);

        bool isErrorEnabled(SLF4JContextLogger logger, Marker m);

        bool isInfoEnabled(SLF4JContextLogger logger);

        bool isInfoEnabled(SLF4JContextLogger logger, Marker m);

        bool isTraceEnabled(SLF4JContextLogger logger);

        bool isTraceEnabled(SLF4JContextLogger logger, Marker m);

        bool isWarnEnabled(SLF4JContextLogger logger);

        bool isWarnEnabled(SLF4JContextLogger logger, Marker m);

        void warn(SLF4JContextLogger logger, string str);

        void warn(SLF4JContextLogger logger, string str, object obj);

        void warn(SLF4JContextLogger logger, string str, params object[] objarr);

        void warn(SLF4JContextLogger logger, string str, object obj1, object obj2);

        void warn(SLF4JContextLogger logger, string str, Exception t);

        void warn(SLF4JContextLogger logger, Marker m, string str);

        void warn(SLF4JContextLogger logger, Marker m, string str, object obj);

        void warn(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2);

        void warn(SLF4JContextLogger logger, Marker m, string str, params object[] objarr);

        void warn(SLF4JContextLogger logger, Marker m, string str, Exception t);

        void error(SLF4JContextLogger logger, string str);

        void error(SLF4JContextLogger logger, string str, object obj);

        void error(SLF4JContextLogger logger, string str, object obj1, object obj2);

        void error(SLF4JContextLogger logger, string str, params object[] objarr);

        void error(SLF4JContextLogger logger, string str, Exception t);

        void error(SLF4JContextLogger logger, Marker m, string str);

        void error(SLF4JContextLogger logger, Marker m, string str, object obj);

        void error(SLF4JContextLogger logger, Marker m, string str, object obj1, object obj2);

        void error(SLF4JContextLogger logger, Marker m, string str, params object[] objarr);

        void error(SLF4JContextLogger logger, Marker m, string str, Exception t);

    }

}