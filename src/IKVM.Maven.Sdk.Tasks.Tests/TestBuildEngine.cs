using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// Minimal <see cref="IBuildEngine"/> implementation which records the events raised by the task under test.
    /// </summary>
    class TestBuildEngine : IBuildEngine
    {

        readonly TestContext context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context">optional test context to mirror events to</param>
        public TestBuildEngine(TestContext context = null)
        {
            this.context = context;
        }

        /// <summary>
        /// Errors logged by the task.
        /// </summary>
        public List<BuildErrorEventArgs> Errors { get; } = new List<BuildErrorEventArgs>();

        /// <summary>
        /// Warnings logged by the task.
        /// </summary>
        public List<BuildWarningEventArgs> Warnings { get; } = new List<BuildWarningEventArgs>();

        /// <summary>
        /// Messages logged by the task.
        /// </summary>
        public List<BuildMessageEventArgs> Messages { get; } = new List<BuildMessageEventArgs>();

        public bool ContinueOnError => false;

        public int LineNumberOfTaskNode => 0;

        public int ColumnNumberOfTaskNode => 0;

        public string ProjectFileOfTaskNode => "test.proj";

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Errors.Add(e);
            context?.WriteLine("ERROR: " + e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Warnings.Add(e);
            context?.WriteLine("WARNING: " + e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Messages.Add(e);
            context?.WriteLine(e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            context?.WriteLine(e.Message);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotSupportedException();
        }

    }

}
