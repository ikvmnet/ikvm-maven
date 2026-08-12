using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tests
{

    /// <summary>
    /// Forwards MSBuild events to the test context.
    /// </summary>
    class MSBuildTestLogger : Logger
    {

        readonly TestContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MSBuildTestLogger(TestContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += (sender, evt) => context.WriteLine(evt.Message);
            eventSource.WarningRaised += (sender, evt) => context.WriteLine(evt.Message);
            eventSource.MessageRaised += (sender, evt) =>
            {
                if (IsEnabled(evt.Importance))
                    context.WriteLine(evt.Message);
            };
        }

        /// <summary>
        /// Returns <c>true</c> if a message of the given importance should be forwarded. Every event used to be
        /// forwarded regardless of verbosity, which for a run that drives a full build per test case produced
        /// gigabytes of captured output in the test results.
        /// </summary>
        /// <param name="importance"></param>
        /// <returns></returns>
        bool IsEnabled(MessageImportance importance) => Verbosity switch
        {
            LoggerVerbosity.Quiet => false,
            LoggerVerbosity.Minimal => importance == MessageImportance.High,
            LoggerVerbosity.Normal => importance != MessageImportance.Low,
            _ => true,
        };

    }

}
