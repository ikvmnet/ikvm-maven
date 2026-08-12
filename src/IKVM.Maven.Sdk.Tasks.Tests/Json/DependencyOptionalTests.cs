using System.Collections;
using System.Linq;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    /// <summary>
    /// A POM which does not declare &lt;optional&gt; produces a dependency whose optional flag is null rather than
    /// false, and the writer emits that as a JSON null. The reader has to accept it back.
    /// </summary>
    [TestClass]
    public class DependencyOptionalTests
    {

        static Dependency CreateDependency(java.lang.Boolean optional)
        {
            return new Dependency(
                new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.0"),
                "compile",
                optional,
                java.util.Collections.emptyList());
        }

        [TestMethod]
        public void Should_round_trip_an_unspecified_optional_flag()
        {
            JsonSerializerFixture.RoundTrip(CreateDependency(null)).getOptional().Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_an_optional_dependency()
        {
            JsonSerializerFixture.RoundTrip(CreateDependency(java.lang.Boolean.TRUE)).getOptional().booleanValue().Should().BeTrue();
        }

        [TestMethod]
        public void Should_round_trip_a_non_optional_dependency()
        {
            JsonSerializerFixture.RoundTrip(CreateDependency(java.lang.Boolean.FALSE)).getOptional().booleanValue().Should().BeFalse();
        }

        /// <summary>
        /// The graph the resolver caches is full of dependencies read straight out of POM files, so a single one of
        /// them without an optional flag is enough to make the whole cache file unreadable.
        /// </summary>
        [TestMethod]
        public void Should_round_trip_a_cached_graph_containing_an_unspecified_optional_flag()
        {
            var child = new DefaultDependencyNode(CreateDependency(null));
            var root = new DefaultDependencyNode((Dependency)null);
            root.setChildren(java.util.Arrays.asList(new DependencyNode[] { child }));

            var cacheFile = new MavenResolveCacheFile()
            {
                Version = 3,
                Repositories = new[] { new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/") },
                Dependencies = new[] { CreateDependency(null) },
                Graph = root,
            };

            var r = JsonSerializerFixture.RoundTrip(cacheFile);
            r.Dependencies.Should().ContainSingle().Which.getOptional().Should().BeNull();

            var children = ((IEnumerable)r.Graph.getChildren()).Cast<DependencyNode>().ToList();
            children.Should().ContainSingle();
            children[0].getDependency().getOptional().Should().BeNull();
            children[0].getArtifact().getArtifactId().Should().Be("foo");
        }

    }

}
