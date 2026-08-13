using System;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.slf4j.@event;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// Resolution runs inside the Maven and NuGet stacks, both of which log through their own abstractions. These
    /// bridges are the only way a user ever sees what those stacks are doing.
    /// </summary>
    [TestClass]
    public class LoggingTests
    {

        /// <summary>
        /// A task exists only to give the bridges a <see cref="TaskLoggingHelper"/> bound to a build engine.
        /// </summary>
        class LoggingTask : Task
        {

            public override bool Execute() => true;

        }

        static (TestBuildEngine Engine, TaskLoggingHelper Log) CreateLog()
        {
            var engine = new TestBuildEngine();
            var task = new LoggingTask() { BuildEngine = engine };
            return (engine, task.Log);
        }

        #region NuGet

        [TestMethod]
        public void NuGetLogger_should_throw_on_null_log()
        {
            var f = () => new NuGetMSBuildLogger(null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void NuGetLogger_should_forward_messages()
        {
            var (engine, log) = CreateLog();
            var l = new NuGetMSBuildLogger(log);

            l.LogVerbose("verbose");
            l.LogDebug("debug");
            l.LogMinimal("minimal");
            l.LogInformation("information");

            engine.Messages.Select(i => i.Message).Should().Equal("verbose", "debug", "minimal", "information");
            engine.Warnings.Should().BeEmpty();
            engine.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void NuGetLogger_should_forward_warnings_and_errors()
        {
            var (engine, log) = CreateLog();
            var l = new NuGetMSBuildLogger(log);

            l.LogWarning("warning");
            l.LogError("error");

            engine.Warnings.Should().ContainSingle().Which.Message.Should().Be("warning");
            engine.Errors.Should().ContainSingle().Which.Message.Should().Be("error");
        }

        /// <summary>
        /// The lock file reader hands NuGet diagnostics to whatever logger it is given; errors must reach MSBuild so
        /// that a broken assets file is reported rather than swallowed.
        /// </summary>
        [TestMethod]
        public void ThrowOnLockFileLoadError_should_forward_each_level()
        {
            var (engine, log) = CreateLog();
            var l = new ThrowOnLockFileLoadError(new NuGetMSBuildLogger(log));

            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Verbose, "verbose"));
            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Debug, "debug"));
            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Minimal, "minimal"));
            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Information, "information"));
            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Warning, "warning"));
            l.Log(new NuGet.Common.LogMessage(NuGet.Common.LogLevel.Error, "error"));

            engine.Messages.Should().HaveCount(4);
            engine.Warnings.Should().ContainSingle();
            engine.Errors.Should().ContainSingle();
        }

        [TestMethod]
        public void ThrowOnLockFileLoadError_should_throw_on_null_log()
        {
            var f = () => new ThrowOnLockFileLoadError(null);
            f.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region SLF4J

        [TestMethod]
        public void Slf4jProxy_should_throw_on_null_log()
        {
            var f = () => new SLF4JMSBuildLoggerProxy(null, Level.INFO);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Slf4jLogger_should_discard_messages_without_a_context()
        {
            var logger = new SLF4JContextLogger("test");
            logger.info("dropped");
            logger.isInfoEnabled().Should().BeFalse();
        }

        [TestMethod]
        public void Slf4jLogger_should_route_messages_to_the_active_context()
        {
            var (engine, log) = CreateLog();
            var logger = new SLF4JContextLogger("test");

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(log, Level.INFO)))
            {
                logger.isInfoEnabled().Should().BeTrue();
                logger.info("hello");
                logger.warn("careful");
                logger.error("broken");
            }

            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("hello"));
            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("careful"));
            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("broken"));
        }

        /// <summary>
        /// Maven logs heavily at debug and trace; those must stay off unless the task asked for them or every build
        /// drowns in output.
        /// </summary>
        [TestMethod]
        public void Slf4jLogger_should_honour_the_configured_level()
        {
            var (engine, log) = CreateLog();
            var logger = new SLF4JContextLogger("test");

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(log, Level.INFO)))
            {
                logger.isTraceEnabled().Should().BeFalse();
                logger.isDebugEnabled().Should().BeFalse();
                logger.isInfoEnabled().Should().BeTrue();
                logger.isWarnEnabled().Should().BeTrue();
                logger.isErrorEnabled().Should().BeTrue();

                logger.trace("trace");
                logger.debug("debug");
            }

            engine.Messages.Should().NotContain(i => i.Message.Contains("trace"));
            engine.Messages.Should().NotContain(i => i.Message.Contains("debug"));
        }

        [TestMethod]
        public void Slf4jLogger_should_enable_everything_at_trace_level()
        {
            var (engine, log) = CreateLog();
            var logger = new SLF4JContextLogger("test");

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(log, Level.TRACE)))
            {
                logger.isTraceEnabled().Should().BeTrue();
                logger.isDebugEnabled().Should().BeTrue();
                logger.trace("traced");
                logger.debug("debugged");
            }

            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("traced"));
            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("debugged"));
        }

        [TestMethod]
        public void Slf4jLogger_should_format_placeholders()
        {
            var (engine, log) = CreateLog();
            var logger = new SLF4JContextLogger("test");

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(log, Level.INFO)))
                logger.info("resolving {} from {}", "foo", "central");

            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("resolving foo from central"));
        }

        /// <summary>
        /// The resolve task enters a context per invocation; leaving one has to restore whatever was there before so
        /// that nested or sequential task runs do not leak a disposed task's logger.
        /// </summary>
        [TestMethod]
        public void Slf4jLogger_should_restore_the_previous_context_on_exit()
        {
            var outer = CreateLog();
            var inner = CreateLog();
            var logger = new SLF4JContextLogger("test");

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(outer.Log, Level.INFO)))
            {
                using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(inner.Log, Level.INFO)))
                    logger.info("inner");

                logger.info("outer");
            }

            logger.info("dropped");

            inner.Engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("inner"));
            inner.Engine.Messages.Select(i => i.Message).Should().NotContain(i => i.Contains("outer"));
            outer.Engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("outer"));
            outer.Engine.Messages.Select(i => i.Message).Should().NotContain(i => i.Contains("dropped"));
        }

        [TestMethod]
        public void Slf4jLoggerFactory_should_create_a_named_context_logger()
        {
            var logger = new SLF4JContextLoggerFactory().getLogger("org.eclipse.aether");
            logger.Should().BeOfType<SLF4JContextLogger>();
            logger.getName().Should().Be("org.eclipse.aether");
        }

        /// <summary>
        /// Maven code obtains its loggers through the SLF4J static factory, so the bridge is only wired up if that
        /// factory hands back our implementation.
        /// </summary>
        [TestMethod]
        public void Slf4jLoggerFactory_should_be_installed_for_maven_code()
        {
            var (engine, log) = CreateLog();

            using (SLF4JContextLogger.Enter(new SLF4JMSBuildLoggerProxy(log, Level.INFO)))
                org.slf4j.LoggerFactory.getLogger("org.eclipse.aether").info("from the factory");

            engine.Messages.Select(i => i.Message).Should().Contain(i => i.Contains("from the factory"));
        }

        #endregion

    }

}
