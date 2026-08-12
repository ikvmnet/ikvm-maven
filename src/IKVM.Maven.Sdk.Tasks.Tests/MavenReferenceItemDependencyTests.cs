using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemDependencyTests
    {

        [TestMethod]
        public void CanParseSimpleDependency()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().BeEmpty();
            d.GroupId.Should().Be("com.fasterxml.jackson.core");
            d.ArtifactId.Should().Be("jackson-databind");
            d.Version.Should().Be("2.18.2");
            d.Scope.Should().Be("compile");
            d.Optional.Should().BeFalse();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseDependencyWithPath()
        {
            var d = MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().HaveCount(1);
            d.Path[0].GroupId.Should().Be("com.jayway.jsonpath");
            d.Path[0].ArtifactId.Should().Be("json-path");
            d.Path[0].Version.Should().BeNull();
            d.GroupId.Should().Be("com.fasterxml.jackson.core");
            d.ArtifactId.Should().Be("jackson-databind");
            d.Version.Should().Be("2.18.2");
            d.ToString().Should().Be("com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseDependencyWithVersionedPath()
        {
            var d = MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path:2.10.0/com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().HaveCount(1);
            d.Path[0].Version.Should().Be("2.10.0");
            d.ToString().Should().Be("com.jayway.jsonpath:json-path:2.10.0/com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseOptionalDependency()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2:optional");
            d.Optional.Should().BeTrue();
            d.Scope.Should().Be("compile");
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2:optional");
        }

        [TestMethod]
        public void CanParseDependencyWithScope()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2:runtime");
            d.Scope.Should().Be("runtime");
            d.Optional.Should().BeFalse();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2:runtime");
        }

        [TestMethod]
        public void CanParseDependencyWithScopeAndOptional()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2:runtime:optional");
            d.Scope.Should().Be("runtime");
            d.Optional.Should().BeTrue();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2:runtime:optional");
        }

        [TestMethod]
        public void ShouldThrowOnMissingVersion()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldThrowOnEmptySegment()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path//com.fasterxml.jackson.core:jackson-databind:2.18.2");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldRoundTripThroughMetadata()
        {
            var d = MavenReferenceItemDependency.Parse("a:b/c:d:1.0/e:f:2.0:runtime:optional");
            var r = MavenReferenceItemDependency.Parse(d.ToString());
            r.Should().Be(d);
        }

    }

}
