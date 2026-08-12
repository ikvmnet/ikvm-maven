using System.Collections;
using System.Linq;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using org.eclipse.aether.util.graph.transformer;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    /// <summary>
    /// Conflict resolution does not remove the losing nodes from the graph; it records the winner in the loser's data
    /// map under <see cref="ConflictResolver.NODE_DATA_WINNER"/>. Resolution follows that link to decide which version
    /// of an artifact is actually built, so the link has to survive a trip through the resolution cache file.
    /// </summary>
    [TestClass]
    public class DependencyNodeDataTests
    {

        static DefaultDependencyNode CreateNode(string artifactId, string version)
        {
            return new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", artifactId, "", "jar", version), "compile"));
        }

        [TestMethod]
        public void Should_round_trip_conflict_winner()
        {
            var winner = CreateNode("dup", "2.0");
            var loser = CreateNode("dup", "1.0");
            loser.setData(ConflictResolver.NODE_DATA_WINNER, winner);

            var r = JsonSerializerFixture.RoundTrip(loser);

            var w = r.getData().get(ConflictResolver.NODE_DATA_WINNER);
            w.Should().BeOfType<DefaultDependencyNode>();
            ((DefaultDependencyNode)w).getArtifact().getVersion().Should().Be("2.0");
        }

        /// <summary>
        /// This is the shape the resolver actually walks: the winner is reachable both as a sibling in the graph and
        /// through the loser's data map, and both have to end up as the same instance.
        /// </summary>
        [TestMethod]
        public void Should_round_trip_conflict_winner_within_a_cached_graph()
        {
            var winner = CreateNode("dup", "2.0");
            var loser = CreateNode("dup", "1.0");
            loser.setData(ConflictResolver.NODE_DATA_WINNER, winner);

            var root = new DefaultDependencyNode((Dependency)null);
            root.setChildren(java.util.Arrays.asList(new DependencyNode[] { winner, loser }));

            var cacheFile = new MavenResolveCacheFile()
            {
                Version = 3,
                Repositories = new[] { new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/") },
                Dependencies = new[] { new Dependency(new DefaultArtifact("ikvm.test", "dup", "", "jar", "2.0"), "compile") },
                Graph = root,
            };

            var r = JsonSerializerFixture.RoundTrip(cacheFile);
            var children = ((IEnumerable)r.Graph.getChildren()).Cast<DependencyNode>().ToList();
            children.Should().HaveCount(2);

            var cachedWinner = children[0];
            var cachedLoser = children[1];
            cachedLoser.getData().getOrDefault(ConflictResolver.NODE_DATA_WINNER, cachedLoser).Should().BeSameAs(cachedWinner);
        }

        [TestMethod]
        public void Should_round_trip_a_node_without_data()
        {
            JsonSerializerFixture.RoundTrip(CreateNode("foo", "1.0")).getData().isEmpty().Should().BeTrue();
        }

    }

}
