using System;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    /// <summary>
    /// The comparer decides whether the resolution cache file still applies to the current set of references, so a
    /// false positive silently serves a stale dependency graph.
    /// </summary>
    [TestClass]
    public class DependencyEqualityComparerTests
    {

        static Dependency CreateDependency(string artifactId = "foo", string version = "1.0", string scope = "compile", bool optional = false, string exclusionArtifactId = "bar")
        {
            return new Dependency(
                new DefaultArtifact("ikvm.test", artifactId, "", "jar", version),
                scope,
                optional ? java.lang.Boolean.TRUE : java.lang.Boolean.FALSE,
                exclusionArtifactId != null ? java.util.Arrays.asList(new[] { new Exclusion("ikvm.other", exclusionArtifactId, "", "jar") }) : (java.util.Collection)java.util.Collections.emptyList());
        }

        [TestMethod]
        public void Should_equal_same_reference()
        {
            var d = CreateDependency();
            DependencyEqualityComparer.Default.Equals(d, d).Should().BeTrue();
        }

        [TestMethod]
        public void Should_equal_equivalent_dependencies()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency()).Should().BeTrue();
        }

        [TestMethod]
        public void Should_not_equal_null()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), null).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_artifact_id()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(artifactId: "other")).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_version()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(version: "2.0")).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_scope()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(scope: "runtime")).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_optional()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(optional: true)).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_exclusions()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(exclusionArtifactId: "baz")).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_missing_exclusions()
        {
            DependencyEqualityComparer.Default.Equals(CreateDependency(), CreateDependency(exclusionArtifactId: null)).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_different_artifact_file()
        {
            var a = CreateDependency();
            var b = a.setArtifact(a.getArtifact().setFile(new java.io.File("foo.jar")));
            DependencyEqualityComparer.Default.Equals(a, b).Should().BeFalse();
        }

        [TestMethod]
        public void Should_equal_dependencies_that_survive_a_cache_round_trip()
        {
            var d = CreateDependency();
            DependencyEqualityComparer.Default.Equals(d, JsonSerializerFixture.RoundTrip(d)).Should().BeTrue();
        }

        [TestMethod]
        public void GetHashCode_should_not_be_supported()
        {
            var f = () => DependencyEqualityComparer.Default.GetHashCode(CreateDependency());
            f.Should().Throw<NotImplementedException>();
        }

    }

}
