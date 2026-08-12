using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void Should_write_dependency_classifier_scope_and_optional()
        {
            var engine = new TestBuildEngine();
            var t = new MavenWriteProjectObjectModelFile();
            t.BuildEngine = engine;

            t.GroupId = "ikvm.test";
            t.ArtifactId = "ikvm-test";
            t.Version = "1.0";
            t.ProjectFile = Path.GetTempFileName();

            var i1 = new TaskItem("ikvm.test:dep:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "dep");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Classifier, "linux-x86_64");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "runtime");
            i1.SetMetadata(MavenReferenceItemMetadata.Optional, "true");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();

            var d = XDocument.Load(t.ProjectFile).Root.Element(POM + "dependencies").Element(POM + "dependency");
            d.Element(POM + "classifier").Should().HaveValue("linux-x86_64");
            d.Element(POM + "scope").Should().HaveValue("runtime");
            d.Element(POM + "optional").Should().HaveValue("true");
        }

        [TestMethod]
        public void Should_write_multiple_dependencies_in_order()
        {
            var engine = new TestBuildEngine();
            var t = new MavenWriteProjectObjectModelFile();
            t.BuildEngine = engine;

            t.GroupId = "ikvm.test";
            t.ArtifactId = "ikvm-test";
            t.Version = "1.0";
            t.ProjectFile = Path.GetTempFileName();
            t.References = new[] { CreateReference("dep1", "1.0"), CreateReference("dep2", "2.0") };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();

            var d = XDocument.Load(t.ProjectFile).Root.Element(POM + "dependencies").Elements(POM + "dependency").ToList();
            d.Should().HaveCount(2);
            d[0].Element(POM + "artifactId").Should().HaveValue("dep1");
            d[0].Element(POM + "version").Should().HaveValue("1.0");
            d[1].Element(POM + "artifactId").Should().HaveValue("dep2");
            d[1].Element(POM + "version").Should().HaveValue("2.0");
        }

        /// <summary>
        /// The POM file participates in incremental builds, so an unchanged POM must not be rewritten.
        /// </summary>
        [TestMethod]
        public void Should_not_rewrite_unchanged_file()
        {
            var engine = new TestBuildEngine();
            var projectFile = Path.GetTempFileName();

            var t1 = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = new[] { CreateReference("dep1", "1.0") } };
            t1.Execute().Should().BeTrue();

            // reset the timestamp to a known value so we can detect a rewrite
            var stamp = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(projectFile, stamp);

            var t2 = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = new[] { CreateReference("dep1", "1.0") } };
            t2.Execute().Should().BeTrue();

            File.GetLastWriteTimeUtc(projectFile).Should().Be(stamp);
        }

        [TestMethod]
        public void Should_rewrite_changed_file()
        {
            var engine = new TestBuildEngine();
            var projectFile = Path.GetTempFileName();

            var t1 = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = new[] { CreateReference("dep1", "1.0") } };
            t1.Execute().Should().BeTrue();

            var stamp = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(projectFile, stamp);

            var t2 = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = new[] { CreateReference("dep1", "2.0") } };
            t2.Execute().Should().BeTrue();

            File.GetLastWriteTimeUtc(projectFile).Should().NotBe(stamp);
            XDocument.Load(projectFile).Root.Element(POM + "dependencies").Element(POM + "dependency").Element(POM + "version").Should().HaveValue("2.0");
        }

        [TestMethod]
        public void Should_overwrite_existing_file_with_unrelated_content()
        {
            var engine = new TestBuildEngine();
            var projectFile = Path.GetTempFileName();
            File.WriteAllText(projectFile, "this is not a pom file");

            var t = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = Array.Empty<TaskItem>() };
            t.Execute().Should().BeTrue();

            XDocument.Load(projectFile).Root.Element(POM + "artifactId").Should().HaveValue("ikvm-test");
        }

        [TestMethod]
        public void Should_write_pom_readable_by_the_import_task()
        {
            var engine = new TestBuildEngine();
            var projectFile = Path.GetTempFileName();

            var i1 = new TaskItem("ikvm.test:dep:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "dep");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Classifier, "cls");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "runtime");

            var t = new MavenWriteProjectObjectModelFile() { BuildEngine = engine, GroupId = "ikvm.test", ArtifactId = "ikvm-test", Version = "1.0", ProjectFile = projectFile, References = new[] { i1 } };
            t.Execute().Should().BeTrue();

            var d = MavenReferenceItemImport.GetProjectObjectModelFileDependencies(projectFile).ToList();
            d.Should().ContainSingle();

            var item = MavenReferenceItemImport.GetMavenReferenceItem(d[0]);
            item.GroupId.Should().Be("ikvm.test");
            item.ArtifactId.Should().Be("dep");
            item.Version.Should().Be("1.0");
            item.Classifier.Should().Be("cls");
            item.Scope.Should().Be("runtime");
        }

        static TaskItem CreateReference(string artifactId, string version)
        {
            var i = new TaskItem($"ikvm.test:{artifactId}:{version}");
            i.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i.SetMetadata(MavenReferenceItemMetadata.ArtifactId, artifactId);
            i.SetMetadata(MavenReferenceItemMetadata.Version, version);
            return i;
        }

    }

}