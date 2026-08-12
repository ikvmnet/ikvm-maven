using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenTaskUtilTests
    {

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void TryParseArtifact_should_throw_on_missing_coordinates(string coords)
        {
            var f = () => MavenTaskUtil.TryParseArtifact(coords);
            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void TryParseArtifact_should_parse_group_artifact_version()
        {
            var a = MavenTaskUtil.TryParseArtifact("ikvm.test:foo:1.2.3");
            a.Should().NotBeNull();
            a.getGroupId().Should().Be("ikvm.test");
            a.getArtifactId().Should().Be("foo");
            a.getVersion().Should().Be("1.2.3");
            a.getExtension().Should().Be("jar");
            a.getClassifier().Should().BeEmpty();
        }

        [TestMethod]
        public void TryParseArtifact_should_parse_extension()
        {
            var a = MavenTaskUtil.TryParseArtifact("ikvm.test:foo:pom:1.2.3");
            a.Should().NotBeNull();
            a.getExtension().Should().Be("pom");
            a.getVersion().Should().Be("1.2.3");
        }

        [TestMethod]
        public void TryParseArtifact_should_parse_classifier_and_extension()
        {
            var a = MavenTaskUtil.TryParseArtifact("ikvm.test:foo:jar:cls:1.2.3");
            a.Should().NotBeNull();
            a.getClassifier().Should().Be("cls");
            a.getExtension().Should().Be("jar");
            a.getVersion().Should().Be("1.2.3");
        }

        [TestMethod]
        [DataRow("ikvm.test")]
        [DataRow("ikvm.test:foo:jar:cls:extra:1.2.3")]
        public void TryParseArtifact_should_return_null_for_invalid_coordinates(string coords)
        {
            MavenTaskUtil.TryParseArtifact(coords).Should().BeNull();
        }

    }

}
