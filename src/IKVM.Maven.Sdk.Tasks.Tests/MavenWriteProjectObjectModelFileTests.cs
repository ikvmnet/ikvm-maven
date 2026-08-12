using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenWriteProjectObjectModelFileTests
    {

        static readonly XNamespace POM = "http://maven.apache.org/POM/4.0.0";
        static readonly XNamespace EXT = "http://ikvm.org/POM-EXT/1.0.0";

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

        [TestMethod]
        public void Should_write_pom_with_declared_dependencies()
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

            var i1 = new TaskItem("org.apache.calcite:calcite-core:1.43.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.calcite");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "calcite-core");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.43.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Dependencies, "com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2:optional");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            var x = XDocument.Load(t.ProjectFile);

            var dependency = x.Root.Element(POM + "dependencies").Element(POM + "dependency");
            dependency.Element(POM + "groupId").Should().HaveValue("org.apache.calcite");

            var selector = dependency.Element(EXT + "dependencies").Element(POM + "dependency");
            selector.Element(POM + "groupId").Should().HaveValue("com.jayway.jsonpath");
            selector.Element(POM + "artifactId").Should().HaveValue("json-path");
            selector.Element(POM + "version").Should().BeNull();

            var declared = selector.Element(EXT + "dependencies").Element(POM + "dependency");
            declared.Element(POM + "groupId").Should().HaveValue("com.fasterxml.jackson.core");
            declared.Element(POM + "artifactId").Should().HaveValue("jackson-databind");
            declared.Element(POM + "version").Should().HaveValue("2.18.2");
            declared.Element(POM + "optional").Should().HaveValue("true");
            declared.Element(POM + "scope").Should().BeNull();
            declared.Element(EXT + "dependencies").Should().BeNull();
        }

        [TestMethod]
        public void Should_merge_declared_dependency_paths()
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

            var i1 = new TaskItem("org.apache.calcite:calcite-core:1.43.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.calcite");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "calcite-core");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.43.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Dependencies, "com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2;com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-annotations:2.18.2");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            var x = XDocument.Load(t.ProjectFile);

            var dependency = x.Root.Element(POM + "dependencies").Element(POM + "dependency");
            dependency.Element(EXT + "dependencies").Elements(POM + "dependency").Should().HaveCount(1);

            var selector = dependency.Element(EXT + "dependencies").Element(POM + "dependency");
            selector.Element(EXT + "dependencies").Elements(POM + "dependency").Should().HaveCount(2);
        }

    }

}