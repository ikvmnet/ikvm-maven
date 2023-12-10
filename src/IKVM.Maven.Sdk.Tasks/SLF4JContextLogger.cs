using System;
using System.Runtime.CompilerServices;

using org.slf4j;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// SLF4J logger implementation which passes messages to the active proxy for the thread.
    /// </summary>
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
        static ISLF4JLoggerProxy proxy;

        /// <summary>
        /// Holds the previous context to restore upon exit.
        /// </summary>
        class ProxyContext : IDisposable
        {

            readonly ISLF4JLoggerProxy previous;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="previous"></param>
            public ProxyContext(ISLF4JLoggerProxy previous)
            {
                this.previous = previous;
            }

            /// <summary>
            /// Disposes of the instance.
            /// </summary>
            public void Dispose()
            {
                proxy = previous;
            }

        }

        /// <summary>
        /// Enters a new log proxy.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static IDisposable Enter<TProxy>(TProxy proxy)
            where TProxy : ISLF4JLoggerProxy
        {
            var previous = SLF4JContextLogger.proxy;
            SLF4JContextLogger.proxy = proxy;
            return new ProxyContext(previous);
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
            proxy?.trace(this, str);
        }

        public void trace(string str, object obj)
        {
            proxy?.trace(this, str, obj);
        }

        public void trace(string str, object obj1, object obj2)
        {
            proxy?.trace(this, str, obj1, obj2);
        }

        public void trace(string str, params object[] objarr)
        {
            proxy?.trace(this, str, objarr);
        }

        public void trace(string str, Exception t)
        {
            proxy?.trace(this, str, t);
        }

        public void trace(Marker m, string str)
        {
            proxy?.trace(this, m, str);
        }

        public void trace(Marker m, string str, object obj)
        {
            proxy?.trace(this, m, str, obj);
        }

        public void trace(Marker m, string str, object obj1, object obj2)
        {
            proxy?.trace(this, m, str, obj1, obj2);
        }

        public void trace(Marker m, string str, params object[] objarr)
        {
            proxy?.trace(this, m, str, objarr);
        }

        public void trace(Marker m, string str, Exception t)
        {
            proxy?.trace(this, m, str, t);
        }

        public void debug(string str)
        {
            proxy?.debug(this, str);
        }

        public void debug(string str, object obj)
        {
            proxy?.debug(this, str, obj);
        }

        public void debug(string str, object obj1, object obj2)
        {
            proxy?.debug(this, str, obj1, obj2);
        }

        public void debug(string str, params object[] objarr)
        {
            proxy?.debug(this, str, objarr);
        }

        public void debug(string str, Exception t)
        {
            proxy?.debug(this, str, t);
        }

        public void debug(Marker m, string str)
        {
            proxy?.debug(this, m, str);
        }

        public void debug(Marker m, string str, object obj)
        {
            proxy?.debug(this, m, str, obj);
        }

        public void debug(Marker m, string str, object obj1, object obj2)
        {
            proxy?.debug(this, m, str, obj1, obj2);
        }

        public void debug(Marker m, string str, params object[] objarr)
        {
            proxy?.debug(this, m, str, objarr);
        }

        public void debug(Marker m, string str, Exception t)
        {
            proxy?.debug(this, m, str, t);
        }

        public void info(string str)
        {
            proxy?.info(this, str);
        }

        public void info(string str, object obj)
        {
            proxy?.info(this, str, obj);
        }

        public void info(string str, object obj1, object obj2)
        {
            proxy?.info(this, str, obj1, obj2);
        }

        public void info(string str, params object[] objarr)
        {
            proxy?.info(this, str, objarr);
        }

        public void info(string str, Exception t)
        {
            proxy?.info(this, str, t);
        }

        public void info(Marker m, string str)
        {
            proxy?.info(this, m, str);
        }

        public void info(Marker m, string str, object obj)
        {
            proxy?.info(this, m, str, obj);
        }

        public void info(Marker m, string str, object obj1, object obj2)
        {
            proxy?.info(this, m, str, obj1, obj2);
        }

        public void info(Marker m, string str, params object[] objarr)
        {
            proxy?.info(this, m, str, objarr);
        }

        public void info(Marker m, string str, Exception t)
        {
            proxy?.info(this, m, str, t);
        }

        public bool isDebugEnabled()
        {
            return proxy?.isDebugEnabled(this) ?? false;
        }

        public bool isDebugEnabled(Marker m)
        {
            return proxy?.isDebugEnabled(this, m) ?? false;
        }

        public bool isErrorEnabled()
        {
            return proxy?.isErrorEnabled(this) ?? false;
        }

        public bool isErrorEnabled(Marker m)
        {
            return proxy?.isErrorEnabled(this, m) ?? false;
        }

        public bool isInfoEnabled()
        {
            return proxy?.isInfoEnabled(this) ?? false;
        }

        public bool isInfoEnabled(Marker m)
        {
            return proxy?.isInfoEnabled(this, m) ?? false;
        }

        public bool isTraceEnabled()
        {
            return proxy?.isTraceEnabled(this) ?? false;
        }

        public bool isTraceEnabled(Marker m)
        {
            return proxy?.isTraceEnabled(this, m) ?? false;
        }

        public bool isWarnEnabled()
        {
            return proxy?.isWarnEnabled(this) ?? false;
        }

        public bool isWarnEnabled(Marker m)
        {
            return proxy?.isWarnEnabled(this, m) ?? false;
        }

        public void warn(string str)
        {
            proxy?.warn(this, str);
        }

        public void warn(string str, object obj)
        {
            proxy?.warn(this, str, obj);
        }

        public void warn(string str, params object[] objarr)
        {
            proxy?.warn(this, str, objarr);
        }

        public void warn(string str, object obj1, object obj2)
        {
            proxy?.warn(this, str, obj1, obj2);
        }

        public void warn(string str, Exception t)
        {
            proxy?.warn(this, str, t);
        }

        public void warn(Marker m, string str)
        {
            proxy?.warn(this, m, str);
        }

        public void warn(Marker m, string str, object obj)
        {
            proxy?.warn(this, m, str, obj);
        }

        public void warn(Marker m, string str, object obj1, object obj2)
        {
            proxy?.warn(this, m, str, obj1, obj2);
        }

        public void warn(Marker m, string str, params object[] objarr)
        {
            proxy?.warn(this, m, str, objarr);
        }

        public void warn(Marker m, string str, Exception t)
        {
            proxy?.warn(this, m, str, t);
        }

        public void error(string str)
        {
            proxy?.error(this, str);
        }

        public void error(string str, object obj)
        {
            proxy?.error(this, str, obj);
        }

        public void error(string str, object obj1, object obj2)
        {
            proxy?.error(this, str, obj1, obj2);
        }

        public void error(string str, params object[] objarr)
        {
            proxy?.error(this, str, objarr);
        }

        public void error(string str, Exception t)
        {
            proxy?.error(this, str, t);
        }

        public void error(Marker m, string str)
        {
            proxy?.error(this, m, str);
        }

        public void error(Marker m, string str, object obj)
        {

            proxy?.error(this, m, str, obj);
        }

        public void error(Marker m, string str, object obj1, object obj2)
        {

            proxy?.error(this, m, str, obj1, obj2);
        }

        public void error(Marker m, string str, params object[] objarr)
        {

            proxy?.error(this, m, str, objarr);
        }

        public void error(Marker m, string str, Exception t)
        {
            proxy?.error(this, m, str, t);
        }

    }

}