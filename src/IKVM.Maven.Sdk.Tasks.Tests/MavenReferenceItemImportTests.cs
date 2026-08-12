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
                    var d = MavenReferenceItemImport.GetProjectObjectModelFileReferences(pom).ToList();
                    d.Should().HaveCount(1);
                }
            }
        }

        [TestMethod]
        public void CanImportDeclaredDependencies()
        {
            var pom = Path.GetTempFileName();
            File.WriteAllText(pom, """
                <?xml version="1.0" encoding="UTF-8"?>
                <project xmlns="http://maven.apache.org/POM/4.0.0" xmlns:ikvm="http://ikvm.org/POM-EXT/1.0.0">
                  <modelVersion>4.0.0</modelVersion>
                  <groupId>ikvm.nuget</groupId>
                  <artifactId>Test.Package</artifactId>
                  <version>1.0.0</version>
                  <dependencies>
                    <dependency>
                      <groupId>org.apache.calcite</groupId>
                      <artifactId>calcite-core</artifactId>
                      <version>1.43.0</version>
                      <scope>compile</scope>
                      <ikvm:dependencies>
                        <dependency>
                          <groupId>com.jayway.jsonpath</groupId>
                          <artifactId>json-path</artifactId>
                          <ikvm:dependencies>
                            <dependency>
                              <groupId>com.fasterxml.jackson.core</groupId>
                              <artifactId>jackson-databind</artifactId>
                              <version>2.18.2</version>
                              <optional>true</optional>
                            </dependency>
                          </ikvm:dependencies>
                        </dependency>
                      </ikvm:dependencies>
                    </dependency>
                  </dependencies>
                </project>
                """);

            var items = MavenReferenceItemImport.GetProjectObjectModelFileReferences(pom).ToList();
            items.Should().HaveCount(1);

            var item = items[0];
            item.GroupId.Should().Be("org.apache.calcite");
            item.ArtifactId.Should().Be("calcite-core");
            item.Version.Should().Be("1.43.0");

            item.Dependencies.Should().HaveCount(1);
            var d = item.Dependencies[0];
            d.Path.Should().HaveCount(1);
            d.Path[0].GroupId.Should().Be("com.jayway.jsonpath");
            d.Path[0].ArtifactId.Should().Be("json-path");
            d.Path[0].Version.Should().BeNull();
            d.GroupId.Should().Be("com.fasterxml.jackson.core");
            d.ArtifactId.Should().Be("jackson-databind");
            d.Version.Should().Be("2.18.2");
            d.Optional.Should().BeTrue();
            d.Scope.Should().Be("compile");
        }

    }

}