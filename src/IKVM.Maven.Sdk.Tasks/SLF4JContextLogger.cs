using System;
using System.Runtime.CompilerServices;

using org.slf4j;

namespace IKVM.Maven.Sdk.Tasks
{

    class SLF4JContextLogger : org.slf4j.Logger
    {

        /// <summary>
        /// Initializes the static instance.
        /// </summary>
        static SLF4JContextLogger()
        {
            RuntimeHelpers.RunClassConstructor(typeof(SLF4JContextLoggerFactory).TypeHandle);
        }

        [ThreadStatic]
        static ISLF4JLoggerProxy context;

        /// <summary>
        /// Holds the previous context to restore upon exit.
        /// </summary>
        class ExitContext : IDisposable
        {

            readonly ISLF4JLoggerProxy previous;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="previous"></param>
            public ExitContext(ISLF4JLoggerProxy previous)
            {
                this.previous = previous;
            }

            /// <summary>
            /// Disposes of the instance.
            /// </summary>
            public void Dispose()
            {
                context = previous;
            }

        }

        /// <summary>
        /// Enters a new log context.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static IDisposable Enter<TProxy>(TProxy proxy)
            where TProxy : ISLF4JLoggerProxy
        {
            var previous = context;
            context = proxy;
            return new ExitContext(previous);
        }

        readonly string name;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SLF4JContextLogger(string name)
        {
            this.name = name;
        }

        public string getName()
        {
            return name;
        }

        public void trace(string str)
        {
            context?.trace(this, str);
        }

        public void trace(string str, object obj)
        {
            context?.trace(this, str, obj);
        }

        public void trace(string str, object obj1, object obj2)
        {
            context?.trace(this, str, obj1, obj2);
        }

        public void trace(string str, params object[] objarr)
        {
            context?.trace(this, str, objarr);
        }

        public void trace(string str, Exception t)
        {
            context?.trace(this, str, t);
        }

        public void trace(Marker m, string str)
        {
            context?.trace(this, m, str);
        }

        public void trace(Marker m, string str, object obj)
        {
            context?.trace(this, m, str, obj);
        }

        public void trace(Marker m, string str, object obj1, object obj2)
        {
            context?.trace(this, m, str, obj1, obj2);
        }

        public void trace(Marker m, string str, params object[] objarr)
        {
            context?.trace(this, m, str, objarr);
        }

        public void trace(Marker m, string str, Exception t)
        {
            context?.trace(this, m, str, t);
        }

        public void debug(string str)
        {
            context?.debug(this, str);
        }

        public void debug(string str, object obj)
        {
            context?.debug(this, str, obj);
        }

        public void debug(string str, object obj1, object obj2)
        {
            context?.debug(this, str, obj1, obj2);
        }

        public void debug(string str, params object[] objarr)
        {
            context?.debug(this, str, objarr);
        }

        public void debug(string str, Exception t)
        {
            context?.debug(this, str, t);
        }

        public void debug(Marker m, string str)
        {
            context?.debug(this, m, str);
        }

        public void debug(Marker m, string str, object obj)
        {
            context?.debug(this, m, str, obj);
        }

        public void debug(Marker m, string str, object obj1, object obj2)
        {
            context?.debug(this, m, str, obj1, obj2);
        }

        public void debug(Marker m, string str, params object[] objarr)
        {
            context?.debug(this, m, str, objarr);
        }

        public void debug(Marker m, string str, Exception t)
        {
            context?.debug(this, m, str, t);
        }

        public void info(string str)
        {
            context?.info(this, str);
        }

        public void info(string str, object obj)
        {
            context?.info(this, str, obj);
        }

        public void info(string str, object obj1, object obj2)
        {
            context?.info(this, str, obj1, obj2);
        }

        public void info(string str, params object[] objarr)
        {
            context?.info(this, str, objarr);
        }

        public void info(string str, Exception t)
        {
            context?.info(this, str, t);
        }

        public void info(Marker m, string str)
        {
            context?.info(this, m, str);
        }

        public void info(Marker m, string str, object obj)
        {
            context?.info(this, m, str, obj);
        }

        public void info(Marker m, string str, object obj1, object obj2)
        {
            context?.info(this, m, str, obj1, obj2);
        }

        public void info(Marker m, string str, params object[] objarr)
        {
            context?.info(this, m, str, objarr);
        }

        public void info(Marker m, string str, Exception t)
        {
            context?.info(this, m, str, t);
        }

        public bool isDebugEnabled()
        {
            return context?.isDebugEnabled(this) ?? false;
        }

        public bool isDebugEnabled(Marker m)
        {
            return context?.isDebugEnabled(this, m) ?? false;
        }

        public bool isErrorEnabled()
        {
            return context?.isErrorEnabled(this) ?? false;
        }

        public bool isErrorEnabled(Marker m)
        {
            return context?.isErrorEnabled(this, m) ?? false;
        }

        public bool isInfoEnabled()
        {
            return context?.isInfoEnabled(this) ?? false;
        }

        public bool isInfoEnabled(Marker m)
        {
            return context?.isInfoEnabled(this, m) ?? false;
        }

        public bool isTraceEnabled()
        {
            return context?.isTraceEnabled(this) ?? false;
        }

        public bool isTraceEnabled(Marker m)
        {
            return context?.isTraceEnabled(this, m) ?? false;
        }

        public bool isWarnEnabled()
        {
            return context?.isWarnEnabled(this) ?? false;
        }

        public bool isWarnEnabled(Marker m)
        {
            return context?.isWarnEnabled(this, m) ?? false;
        }

        public void warn(string str)
        {
            context?.warn(this, str);
        }

        public void warn(string str, object obj)
        {
            context?.warn(this, str, obj);
        }

        public void warn(string str, params object[] objarr)
        {
            context?.warn(this, str, objarr);
        }

        public void warn(string str, object obj1, object obj2)
        {
            context?.warn(this, str, obj1, obj2);
        }

        public void warn(string str, Exception t)
        {
            context?.warn(this, str, t);
        }

        public void warn(Marker m, string str)
        {
            context?.warn(this, m, str);
        }

        public void warn(Marker m, string str, object obj)
        {
            context?.warn(this, m, str, obj);
        }

        public void warn(Marker m, string str, object obj1, object obj2)
        {
            context?.warn(this, m, str, obj1, obj2);
        }

        public void warn(Marker m, string str, params object[] objarr)
        {
            context?.warn(this, m, str, objarr);
        }

        public void warn(Marker m, string str, Exception t)
        {
            context?.warn(this, m, str, t);
        }

        public void error(string str)
        {
            context?.error(this, str);
        }

        public void error(string str, object obj)
        {
            context?.error(this, str, obj);
        }

        public void error(string str, object obj1, object obj2)
        {
            context?.error(this, str, obj1, obj2);
        }

        public void error(string str, params object[] objarr)
        {
            context?.error(this, str, objarr);
        }

        public void error(string str, Exception t)
        {
            context?.error(this, str, t);
        }

        public void error(Marker m, string str)
        {
            context?.error(this, m, str);
        }

        public void error(Marker m, string str, object obj)
        {

            context?.error(this, m, str, obj);
        }

        public void error(Marker m, string str, object obj1, object obj2)
        {

            context?.error(this, m, str, obj1, obj2);
        }

        public void error(Marker m, string str, params object[] objarr)
        {

            context?.error(this, m, str, objarr);
        }

        public void error(Marker m, string str, Exception t)
        {
            context?.error(this, m, str, t);
        }

    }

}