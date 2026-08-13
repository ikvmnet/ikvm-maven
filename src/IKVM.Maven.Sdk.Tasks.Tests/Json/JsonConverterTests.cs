using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.Json;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;
using org.eclipse.aether.repository;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    [TestClass]
    public class JsonConverterTests
    {

        [TestMethod]
        public void Should_round_trip_artifact()
        {
            var a = new DefaultArtifact("ikvm.test", "foo", "cls", "jar", "1.2.3");
            var r = JsonSerializerFixture.RoundTrip(a);

            r.Should().NotBeNull();
            r.getGroupId().Should().Be("ikvm.test");
            r.getArtifactId().Should().Be("foo");
            r.getClassifier().Should().Be("cls");
            r.getExtension().Should().Be("jar");
            r.getVersion().Should().Be("1.2.3");
        }

        [TestMethod]
        public void Should_round_trip_artifact_properties()
        {
            var p = new java.util.HashMap();
            p.put("type", "jar");
            p.put("language", "java");

            var a = new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3", p, (java.io.File)null);
            var r = JsonSerializerFixture.RoundTrip(a);

            r.getProperties().get("type").Should().Be("jar");
            r.getProperties().get("language").Should().Be("java");
        }

        /// <summary>
        /// The path to the resolved artifact on disk is the single most important thing carried by the cache; without
        /// it the cached graph cannot be turned back into IkvmReferenceItems.
        /// </summary>
        [TestMethod]
        public void Should_round_trip_artifact_file()
        {
            var path = Path.Combine(Path.GetTempPath(), "hellotest-1.0.jar");
            var a = new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3").setFile(new java.io.File(path));
            var r = JsonSerializerFixture.RoundTrip((DefaultArtifact)a);

            r.getFile().Should().NotBeNull();
            r.getFile().getPath().Should().Be(new java.io.File(path).getPath());
        }

        [TestMethod]
        public void Should_round_trip_artifact_without_file()
        {
            var a = new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3");
            JsonSerializerFixture.RoundTrip(a).getFile().Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_exclusion()
        {
            var e = new Exclusion("ikvm.test", "foo", "cls", "jar");
            var r = JsonSerializerFixture.RoundTrip(e);

            r.getGroupId().Should().Be("ikvm.test");
            r.getArtifactId().Should().Be("foo");
            r.getClassifier().Should().Be("cls");
            r.getExtension().Should().Be("jar");
        }

        [TestMethod]
        public void Should_round_trip_remote_repository()
        {
            var r = JsonSerializerFixture.RoundTrip(new RemoteRepository.Builder("central", "default", "https://repo1.maven.org/maven2/").build());

            r.getId().Should().Be("central");
            r.getContentType().Should().Be("default");
            r.getUrl().Should().Be("https://repo1.maven.org/maven2/");
        }

        [TestMethod]
        public void Should_round_trip_version()
        {
            var v = JsonSerializerFixture.Deserialize<org.eclipse.aether.version.Version>("\"1.2.3\"");
            v.Should().NotBeNull();
            v.toString().Should().Be("1.2.3");
            JsonSerializerFixture.Serialize(v).Should().Be("\"1.2.3\"");
        }

        [TestMethod]
        public void Should_round_trip_version_constraint()
        {
            var c = JsonSerializerFixture.Deserialize<org.eclipse.aether.version.VersionConstraint>("\"1.2.3\"");
            c.Should().NotBeNull();
            c.getVersion().toString().Should().Be("1.2.3");
            JsonSerializerFixture.Serialize(c).Should().Be("\"1.2.3\"");
        }

        [TestMethod]
        [DataRow("[1.0,2.0)")]
        [DataRow("(1.0,2.0]")]
        [DataRow("[1.0,2.0]")]
        public void Should_round_trip_version_range_constraint(string range)
        {
            var c = JsonSerializerFixture.Deserialize<org.eclipse.aether.version.VersionConstraint>($"\"{range}\"");
            c.Should().NotBeNull();
            c.getRange().Should().NotBeNull();
            JsonSerializerFixture.Serialize(c).Should().Be($"\"{range}\"");
        }

        [TestMethod]
        public void Should_read_null_version_from_non_string()
        {
            JsonSerializerFixture.Deserialize<org.eclipse.aether.version.Version>("null").Should().BeNull();
            JsonSerializerFixture.Deserialize<org.eclipse.aether.version.VersionConstraint>("null").Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_dependency()
        {
            var d = new Dependency(
                new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3"),
                "runtime",
                java.lang.Boolean.TRUE,
                java.util.Arrays.asList(new[] { new Exclusion("ikvm.other", "bar", "", "jar") }));

            var r = JsonSerializerFixture.RoundTrip(d);

            r.getArtifact().getArtifactId().Should().Be("foo");
            r.getScope().Should().Be("runtime");
            r.getOptional().booleanValue().Should().BeTrue();
            r.isOptional().Should().BeTrue();

            var e = ((IEnumerable)r.getExclusions()).Cast<Exclusion>().ToList();
            e.Should().ContainSingle();
            e[0].getGroupId().Should().Be("ikvm.other");
            e[0].getArtifactId().Should().Be("bar");
        }

        [TestMethod]
        public void Should_round_trip_dependency_without_exclusions()
        {
            var d = new Dependency(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3"), "compile");
            var r = JsonSerializerFixture.RoundTrip(d);

            r.getScope().Should().Be("compile");
            ((IEnumerable)r.getExclusions()).Cast<Exclusion>().Should().BeEmpty();
        }

        [TestMethod]
        public void Should_round_trip_dependency_node()
        {
            var node = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.2.3"), "compile"));
            node.setManagedBits(DependencyNode.MANAGED_VERSION);
            node.setRepositories(java.util.Arrays.asList(new[] { new RemoteRepository.Builder("central", "default", "https://repo1.maven.org/maven2/").build() }));

            var r = JsonSerializerFixture.RoundTrip(node);

            r.getDependency().getArtifact().getArtifactId().Should().Be("foo");
            r.getArtifact().getArtifactId().Should().Be("foo");
            r.getManagedBits().Should().Be(DependencyNode.MANAGED_VERSION);
            ((IEnumerable)r.getRepositories()).Cast<RemoteRepository>().Should().ContainSingle().Which.getId().Should().Be("central");
        }

        [TestMethod]
        public void Should_round_trip_dependency_node_children()
        {
            var child = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "bar", "", "jar", "2.0"), "compile"));
            var node = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.0"), "compile"));
            node.setChildren(java.util.Arrays.asList(new DependencyNode[] { child }));

            var r = JsonSerializerFixture.RoundTrip(node);

            var children = ((IEnumerable)r.getChildren()).Cast<DependencyNode>().ToList();
            children.Should().ContainSingle();
            children[0].getArtifact().getArtifactId().Should().Be("bar");
        }

        [TestMethod]
        public void Should_round_trip_dependency_node_aliases()
        {
            var node = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.0"), "compile"));
            node.setAliases(java.util.Arrays.asList(new Artifact[] { new DefaultArtifact("ikvm.test", "foo-alias", "", "jar", "1.0") }));

            var r = JsonSerializerFixture.RoundTrip(node);

            ((IEnumerable)r.getAliases()).Cast<Artifact>().Should().ContainSingle().Which.getArtifactId().Should().Be("foo-alias");
        }

        [TestMethod]
        public void Should_round_trip_dependency_node_without_dependency()
        {
            var node = new DefaultDependencyNode(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.0"));
            var r = JsonSerializerFixture.RoundTrip(node);

            r.getDependency().Should().BeNull();
            r.getArtifact().getArtifactId().Should().Be("foo");
        }

        [TestMethod]
        public void Should_round_trip_empty_dependency_node()
        {
            var r = JsonSerializerFixture.RoundTrip(new DefaultDependencyNode((Dependency)null));

            r.Should().NotBeNull();
            r.getDependency().Should().BeNull();
            r.getArtifact().Should().BeNull();
        }

        /// <summary>
        /// A dependency graph is a DAG: the same node is reachable through more than one path. The reference handler
        /// has to collapse those into a single instance or the cached graph explodes and loses its identity.
        /// </summary>
        [TestMethod]
        public void Should_preserve_shared_node_identity()
        {
            var shared = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "shared", "", "jar", "1.0"), "compile"));
            var a = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "a", "", "jar", "1.0"), "compile"));
            a.setChildren(java.util.Arrays.asList(new DependencyNode[] { shared }));
            var b = new DefaultDependencyNode(new Dependency(new DefaultArtifact("ikvm.test", "b", "", "jar", "1.0"), "compile"));
            b.setChildren(java.util.Arrays.asList(new DependencyNode[] { shared }));
            var root = new DefaultDependencyNode((Dependency)null);
            root.setChildren(java.util.Arrays.asList(new DependencyNode[] { a, b }));

            var json = JsonSerializerFixture.Serialize(root);
            json.Should().Contain("$ref");

            var r = JsonSerializerFixture.Deserialize<DefaultDependencyNode>(json);
            var children = ((IEnumerable)r.getChildren()).Cast<DependencyNode>().ToList();
            var sharedA = ((IEnumerable)children[0].getChildren()).Cast<DependencyNode>().Single();
            var sharedB = ((IEnumerable)children[1].getChildren()).Cast<DependencyNode>().Single();

            sharedA.Should().BeSameAs(sharedB);
            sharedA.getArtifact().getArtifactId().Should().Be("shared");
        }

        [TestMethod]
        public void Should_throw_when_serializing_unknown_artifact_type()
        {
            var a = new org.eclipse.aether.util.artifact.SubArtifact(new DefaultArtifact("ikvm.test", "foo", "", "jar", "1.0"), "sources", "jar");
            var f = () => JsonSerializerFixture.Serialize<Artifact>(a);
            f.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Should_throw_when_deserializing_bare_artifact()
        {
            var f = () => JsonSerializerFixture.Deserialize<Artifact>("{}");
            f.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Should_throw_when_deserializing_bare_dependency_node()
        {
            var f = () => JsonSerializerFixture.Deserialize<DependencyNode>("{}");
            f.Should().Throw<Exception>();
        }

        [TestMethod]
        public void Should_read_null_for_non_object_values()
        {
            JsonSerializerFixture.Deserialize<DefaultArtifact>("null").Should().BeNull();
            JsonSerializerFixture.Deserialize<Dependency>("null").Should().BeNull();
            JsonSerializerFixture.Deserialize<DefaultDependencyNode>("null").Should().BeNull();
            JsonSerializerFixture.Deserialize<Exclusion>("null").Should().BeNull();
            JsonSerializerFixture.Deserialize<RemoteRepository>("null").Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_boolean()
        {
            JsonSerializerFixture.RoundTrip(java.lang.Boolean.TRUE).booleanValue().Should().BeTrue();
            JsonSerializerFixture.RoundTrip(java.lang.Boolean.FALSE).booleanValue().Should().BeFalse();
            JsonSerializerFixture.RoundTrip((java.lang.Boolean)null).Should().BeNull();
        }

    }

}
