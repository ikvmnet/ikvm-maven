using System.Collections;
using System.IO;
using System.Linq;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Tests.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using org.eclipse.aether.repository;
using org.eclipse.aether.util.graph.transformer;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// Exercises the shape written to and read from the resolution cache file. A resolution served from the cache has
    /// to be indistinguishable from a fresh resolution, so everything the resolver reads off the graph must survive.
    /// The conflict resolution links carried on the nodes are covered by
    /// <see cref="Json.DependencyNodeDataTests"/>.
    /// </summary>
    [TestClass]
    public class MavenResolveCacheFileTests
    {

        static DefaultDependencyNode CreateNode(string artifactId, string version, string scope, string file = null)
        {
            var artifact = new DefaultArtifact("ikvm.test", artifactId, "", "jar", version);
            if (file != null)
                artifact = (DefaultArtifact)artifact.setFile(new java.io.File(file));

            return new DefaultDependencyNode(new Dependency(artifact, scope));
        }

        static MavenResolveCacheFile CreateCacheFile()
        {
            var leaf = CreateNode("leaf", "1.0", "compile", Path.Combine(Path.GetTempPath(), "leaf-1.0.jar"));
            var branch = CreateNode("branch", "1.0", "compile", Path.Combine(Path.GetTempPath(), "branch-1.0.jar"));
            branch.setChildren(java.util.Arrays.asList(new DependencyNode[] { leaf }));

            var root = new DefaultDependencyNode((Dependency)null);
            root.setChildren(java.util.Arrays.asList(new DependencyNode[] { branch }));

            return new MavenResolveCacheFile()
            {
                Version = 3,
                Repositories = new[] { new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/") },
                Dependencies = new[] { new Dependency(new DefaultArtifact("ikvm.test", "branch", "", "jar", "1.0"), "compile") },
                Graph = root,
            };
        }

        [TestMethod]
        public void Should_round_trip_cache_file()
        {
            var r = JsonSerializerFixture.RoundTrip(CreateCacheFile());

            r.Version.Should().Be(3);
            r.Repositories.Should().ContainSingle().Which.Should().Be(new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/"));
            r.Dependencies.Should().ContainSingle();
            r.Graph.Should().NotBeNull();
        }

        [TestMethod]
        public void Should_round_trip_cached_dependency_graph()
        {
            var r = JsonSerializerFixture.RoundTrip(CreateCacheFile());

            var branch = ((IEnumerable)r.Graph.getChildren()).Cast<DependencyNode>().Should().ContainSingle().Subject;
            branch.getArtifact().getArtifactId().Should().Be("branch");
            branch.getArtifact().getFile().Should().NotBeNull();
            branch.getDependency().getScope().Should().Be("compile");

            var leaf = ((IEnumerable)branch.getChildren()).Cast<DependencyNode>().Should().ContainSingle().Subject;
            leaf.getArtifact().getArtifactId().Should().Be("leaf");
            leaf.getArtifact().getFile().Should().NotBeNull();
        }

        [TestMethod]
        public void Should_round_trip_empty_cache_file()
        {
            var r = JsonSerializerFixture.RoundTrip(new MavenResolveCacheFile());

            r.Version.Should().Be(0);
            r.Repositories.Should().BeNull();
            r.Dependencies.Should().BeNull();
            r.Graph.Should().BeNull();
        }

        [TestMethod]
        public void Should_use_lower_case_property_names()
        {
            var json = JsonSerializerFixture.Serialize(CreateCacheFile());
            json.Should().Contain("\"version\"");
            json.Should().Contain("\"repositories\"");
            json.Should().Contain("\"dependencies\"");
            json.Should().Contain("\"graph\"");
        }

    }

}
