using System;
using System.Collections.Generic;
using System.IO;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using NuGet.Common;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemImportTests
    {

        readonly string binPath = Path.GetDirectoryName(typeof(MavenReferenceItemImportTests).Assembly.Location);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Should_work_with_itemspec_and_version_as_metadata()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemImport();
            t.BuildEngine = engine.Object;

            t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), "net6.0", null).Should().HaveCount(4);
            t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), "net472", null).Should().HaveCount(4);
            t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), "net48", null).Should().HaveCount(4);
        }

    }

}