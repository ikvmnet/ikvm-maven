using System;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenRepositoryItemTests
    {

        [TestMethod]
        public void Should_throw_on_null_id()
        {
            var f = () => new MavenRepositoryItem(null, "https://repo1.maven.org/maven2/");
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Should_throw_on_null_url()
        {
            var f = () => new MavenRepositoryItem("central", null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Should_equal_repository_with_same_values()
        {
            var a = new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/");
            var b = new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/");
            a.Equals(b).Should().BeTrue();
            a.Equals((object)b).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [TestMethod]
        [DataRow("other", "https://repo1.maven.org/maven2/")]
        [DataRow("central", "https://example.org/maven2/")]
        public void Should_not_equal_repository_with_different_values(string id, string url)
        {
            var a = new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/");
            a.Equals(new MavenRepositoryItem(id, url)).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_null()
        {
            var a = new MavenRepositoryItem("central", "https://repo1.maven.org/maven2/");
            a.Equals(null).Should().BeFalse();
            a.Equals((object)null).Should().BeFalse();
        }

        [TestMethod]
        public void Load_should_throw_on_null()
        {
            var f = () => MavenRepositoryItemMetadata.Load(null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Load_should_map_itemspec_to_id_and_url_metadata()
        {
            var i1 = (ITaskItem)new TaskItem("central");
            i1.SetMetadata(MavenRepositoryItemMetadata.Url, "https://repo1.maven.org/maven2/");
            var i2 = (ITaskItem)new TaskItem("local");
            i2.SetMetadata(MavenRepositoryItemMetadata.Url, "file:///c:/repo/");

            var r = MavenRepositoryItemMetadata.Load(new[] { i1, i2 });
            r.Should().HaveCount(2);
            r[0].Id.Should().Be("central");
            r[0].Url.Should().Be("https://repo1.maven.org/maven2/");
            r[1].Id.Should().Be("local");
            r[1].Url.Should().Be("file:///c:/repo/");
        }

        [TestMethod]
        public void Load_should_default_missing_url_to_empty()
        {
            var r = MavenRepositoryItemMetadata.Load(new[] { (ITaskItem)new TaskItem("central") });
            r.Should().ContainSingle().Which.Url.Should().BeEmpty();
        }

        [TestMethod]
        public void Load_should_return_empty_for_empty_input()
        {
            MavenRepositoryItemMetadata.Load(Array.Empty<ITaskItem>()).Should().BeEmpty();
        }

    }

}
