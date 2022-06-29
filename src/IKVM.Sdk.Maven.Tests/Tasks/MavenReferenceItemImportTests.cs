using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using IKVM.Sdk.Maven.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NuGet.Common;

namespace IKVM.Sdk.Maven.Tests.Tasks
{

    [TestClass]
    public class MavenReferenceItemImportTests
    {

        /// <summary>
        /// Forces an exception on errors reading the lock file.
        /// </summary>
        sealed class TestContextLogger : LoggerBase
        {

            readonly TestContext context;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="context"></param>
            public TestContextLogger(TestContext context)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public override void Log(ILogMessage message)
            {
                context.WriteLine(message.Message);
            }

            public override System.Threading.Tasks.Task LogAsync(ILogMessage message)
            {
                context.WriteLine(message.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            }

        }

        readonly string binPath = Path.GetDirectoryName(typeof(MavenReferenceItemImportTests).Assembly.Location);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Can_load_assets_file()
        {
            var l = MavenReferenceItemImport.LoadLockFile(Path.Combine(binPath, "Tasks", "Test.project.assets.json"), new TestContextLogger(TestContext));
            l.Should().NotBeNull();
            l.Targets.Should().NotBeEmpty();
            l.Libraries.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Can_discover_POM()
        {
            var l = MavenReferenceItemImport.LoadLockFile(Path.Combine(binPath, "Tasks", "Test.project.assets.json"), new TestContextLogger(TestContext));
            l.Should().NotBeNull();
            var f = MavenReferenceItemImport.GetProjectObjectModelFiles(l, "netcoreapp3.1", "win7-x64").ToList();
            f.Should().NotBeNull();
        }

    }

}