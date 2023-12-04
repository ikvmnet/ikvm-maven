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
            eventSource.AnyEventRaised += (sender, evt) => context.WriteLine(evt.Message);
        }

    }

}
