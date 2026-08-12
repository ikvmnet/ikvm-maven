using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemImportTests
    {

        readonly string binPath = Path.GetDirectoryName(typeof(MavenReferenceItemImportTests).Assembly.Location);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanDiscoverProjectModelFiles()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemImport();
            t.BuildEngine = engine.Object;

            foreach (var tfm in new[] { "net481", "net6.0", "net8.0", "net8.0-windows" })
            {
                var l = t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), tfm, null);
                l.Should().HaveCount(1);
                foreach (var pom in l)
                {
                    var d = MavenReferenceItemImport.GetProjectObjectModelFileDependencies(pom).ToList();
                    d.Should().HaveCount(1);
                }
            }
        }

        /// <summary>
        /// The package only carries POM files for net481, net6.0 and net8.0; a newer target framework must fall back
        /// to the nearest compatible group rather than silently dropping the reference.
        /// </summary>
        [TestMethod]
        public void Should_fall_back_to_nearest_compatible_project_model_file()
        {
            var t = new MavenReferenceItemImport() { BuildEngine = new TestBuildEngine(TestContext) };
            var l = t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), "net8.0", null);
            l.Should().ContainSingle().Which.Should().EndWith(Path.Combine("net8.0", "HelloTest.pom"));
        }

        [TestMethod]
        public void Should_return_no_project_model_files_for_unknown_target_framework()
        {
            var t = new MavenReferenceItemImport() { BuildEngine = new TestBuildEngine(TestContext) };
            t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), "netstandard2.0", null).Should().BeEmpty();
        }

        [TestMethod]
        public void Should_import_dependencies_from_assets_file()
        {
            var engine = new TestBuildEngine(TestContext);
            var t = new MavenReferenceItemImport();
            t.BuildEngine = engine;
            t.AssetsFilePath = Path.Combine(binPath, "Test.project.assets.json");
            t.TargetFramework = "net8.0";

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();

            t.Items.Should().ContainSingle();
            var i = t.Items[0];
            i.ItemSpec.Should().Be("hellotest:hellotest");
            i.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("hellotest");
            i.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("hellotest");
            i.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
            i.GetMetadata(MavenReferenceItemMetadata.Scope).Should().Be("compile");
        }

        [TestMethod]
        public void Should_import_nothing_for_unknown_target_framework()
        {
            var engine = new TestBuildEngine(TestContext);
            var t = new MavenReferenceItemImport();
            t.BuildEngine = engine;
            t.AssetsFilePath = Path.Combine(binPath, "Test.project.assets.json");
            t.TargetFramework = "netstandard2.0";

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();
            t.Items.Should().BeEmpty();
        }

        [TestMethod]
        public void GetProjectObjectModelFileDependencies_should_throw_on_null()
        {
            var f = () => MavenReferenceItemImport.GetProjectObjectModelFileDependencies(null).ToList();
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetProjectObjectModelFileDependencies_should_return_empty_for_missing_file()
        {
            MavenReferenceItemImport.GetProjectObjectModelFileDependencies(Path.Combine(binPath, "DoesNotExist.pom")).Should().BeEmpty();
        }

        [TestMethod]
        public void GetMavenReferenceItem_should_throw_on_null()
        {
            var f = () => MavenReferenceItemImport.GetMavenReferenceItem(null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetMavenReferenceItem_should_map_dependency()
        {
            var d = new org.apache.maven.model.Dependency();
            d.setGroupId("ikvm.test");
            d.setArtifactId("foo");
            d.setClassifier("cls");
            d.setVersion("1.2.3");
            d.setScope("provided");

            var i = MavenReferenceItemImport.GetMavenReferenceItem(d);
            i.ItemSpec.Should().Be("ikvm.test:foo");
            i.GroupId.Should().Be("ikvm.test");
            i.ArtifactId.Should().Be("foo");
            i.Classifier.Should().Be("cls");
            i.Version.Should().Be("1.2.3");
            i.Scope.Should().Be("provided");
        }

        /// <summary>
        /// The itemspec deliberately omits the version so that references imported from different packages for the
        /// same artifact collapse onto a single item and take part in conflict resolution.
        /// </summary>
        [TestMethod]
        public void GetMavenReferenceItem_should_not_include_version_in_itemspec()
        {
            var d = new org.apache.maven.model.Dependency();
            d.setGroupId("ikvm.test");
            d.setArtifactId("foo");
            d.setVersion("1.2.3");

            MavenReferenceItemImport.GetMavenReferenceItem(d).ItemSpec.Should().Be("ikvm.test:foo");
        }

        [TestMethod]
        public void Should_read_all_dependencies_from_a_project_model_file()
        {
            var pom = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pom");

            try
            {
                File.WriteAllText(pom, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<project xmlns=""http://maven.apache.org/POM/4.0.0"">
    <modelVersion>4.0.0</modelVersion>
    <groupId>ikvm.nuget</groupId>
    <artifactId>Test</artifactId>
    <version>1.0</version>
    <dependencies>
        <dependency>
            <groupId>ikvm.test</groupId>
            <artifactId>foo</artifactId>
            <version>1.0</version>
        </dependency>
        <dependency>
            <groupId>ikvm.test</groupId>
            <artifactId>bar</artifactId>
            <version>2.0</version>
            <classifier>cls</classifier>
            <scope>runtime</scope>
        </dependency>
    </dependencies>
</project>");

                var d = MavenReferenceItemImport.GetProjectObjectModelFileDependencies(pom).ToList();
                d.Should().HaveCount(2);

                var items = d.Select(MavenReferenceItemImport.GetMavenReferenceItem).ToList();
                items[0].ArtifactId.Should().Be("foo");
                items[0].Version.Should().Be("1.0");
                items[1].ArtifactId.Should().Be("bar");
                items[1].Version.Should().Be("2.0");
                items[1].Classifier.Should().Be("cls");
                items[1].Scope.Should().Be("runtime");
            }
            finally
            {
                File.Delete(pom);
            }
        }

    }

}
