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
        sealed class NullLogger : LoggerBase
        {

            public override void Log(ILogMessage message)
            {

            }

            public override System.Threading.Tasks.Task LogAsync(ILogMessage message)
            {
                return System.Threading.Tasks.Task.CompletedTask;
            }

        }

        [TestMethod]
        public void Can_load_assets_file()
        {
            var l = MavenReferenceItemImport.LoadLockFile(Path.Combine(Environment.CurrentDirectory, "Tasks", "Test.project.assets.json"), new NullLogger());
            l.Should().NotBeNull();
            l.Targets.Should().NotBeEmpty();
            l.Libraries.Should().NotBeEmpty();
        }

        [TestMethod]
        public void Can_discover_POM()
        {
            var l = MavenReferenceItemImport.LoadLockFile(Path.Combine(Environment.CurrentDirectory, "Tasks", "Test.project.assets.json"), new NullLogger());
            var f = MavenReferenceItemImport.GetProjectObjectModelFiles(l, "netcoreapp3.1", "win7-x64").ToList();
            f.Should().NotBeNull();
        }

    }

}