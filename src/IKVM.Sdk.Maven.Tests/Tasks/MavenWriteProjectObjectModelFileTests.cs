using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using FluentAssertions;

using IKVM.Sdk.Maven.Tasks;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Sdk.Maven.Tests.Tasks
{

    [TestClass]
    public class MavenWriteProjectObjectModelFileTests
    {

        static readonly XNamespace POM = "http://maven.apache.org/POM/4.0.0";

        [TestMethod]
        public void Should_write_simple_pom()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenWriteProjectObjectModelFile();
            t.BuildEngine = engine.Object;

            t.GroupId = "ikvm.test";
            t.ArtifactId = "ikvm-test";
            t.Version = "1.0";
            t.References = Array.Empty<TaskItem>();
            t.ProjectFile = Path.GetTempFileName();

            t.Execute().Should().BeTrue();
            File.Exists(t.ProjectFile).Should().BeTrue();
            var x = XDocument.Load(t.ProjectFile);

            x.Root.Element(POM + "groupId").Should().HaveValue("ikvm.test");
            x.Root.Element(POM + "artifactId").Should().HaveValue("ikvm-test");
            x.Root.Element(POM + "version").Should().HaveValue("1.0");
        }

        [TestMethod]
        public void Should_write_pom_with_dependencies()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenWriteProjectObjectModelFile();
            t.BuildEngine = engine.Object;

            t.GroupId = "ikvm.test";
            t.ArtifactId = "ikvm-test";
            t.Version = "1.0";
            t.ProjectFile = Path.GetTempFileName();

            var i1 = new TaskItem("ikvm.test:dep:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "dep");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            File.Exists(t.ProjectFile).Should().BeTrue();
            var x = XDocument.Load(t.ProjectFile);

            x.Root.Element(POM + "groupId").Should().HaveValue("ikvm.test");
            x.Root.Element(POM + "artifactId").Should().HaveValue("ikvm-test");
            x.Root.Element(POM + "version").Should().HaveValue("1.0");

            x.Root.Element(POM + "dependencies").Elements(POM + "dependency").Should().HaveCount(1);
            x.Root.Element(POM + "dependencies").Element(POM + "dependency").Element(POM + "groupId").Should().HaveValue("ikvm.test");
            x.Root.Element(POM + "dependencies").Element(POM + "dependency").Element(POM + "artifactId").Should().HaveValue("dep");
            x.Root.Element(POM + "dependencies").Element(POM + "dependency").Element(POM + "version").Should().HaveValue("1.0");
        }

    }

}