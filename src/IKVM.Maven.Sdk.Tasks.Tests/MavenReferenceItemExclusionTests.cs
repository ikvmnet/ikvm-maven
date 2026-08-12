using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemExclusionTests
    {

        [TestMethod]
        public void Should_throw_on_null_group_id()
        {
            var f = () => new MavenReferenceItemExclusion(null, "foo", null, null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Should_throw_on_null_artifact_id()
        {
            var f = () => new MavenReferenceItemExclusion("ikvm.test", null, null, null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Should_allow_null_classifier_and_extension()
        {
            var e = new MavenReferenceItemExclusion("ikvm.test", "foo", null, null);
            e.GroupId.Should().Be("ikvm.test");
            e.ArtifactId.Should().Be("foo");
            e.Classifier.Should().BeNull();
            e.Extension.Should().BeNull();
        }

        [TestMethod]
        public void Should_equal_exclusion_with_same_values()
        {
            var a = new MavenReferenceItemExclusion("ikvm.test", "foo", "cls", "jar");
            var b = new MavenReferenceItemExclusion("ikvm.test", "foo", "cls", "jar");
            a.Equals(b).Should().BeTrue();
            a.Equals((object)b).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [TestMethod]
        [DataRow("other", "foo", "cls", "jar")]
        [DataRow("ikvm.test", "other", "cls", "jar")]
        [DataRow("ikvm.test", "foo", "other", "jar")]
        [DataRow("ikvm.test", "foo", "cls", "other")]
        [DataRow("ikvm.test", "foo", null, "jar")]
        [DataRow("ikvm.test", "foo", "cls", null)]
        public void Should_not_equal_exclusion_with_different_values(string groupId, string artifactId, string classifier, string extension)
        {
            var a = new MavenReferenceItemExclusion("ikvm.test", "foo", "cls", "jar");
            var b = new MavenReferenceItemExclusion(groupId, artifactId, classifier, extension);
            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_null()
        {
            var a = new MavenReferenceItemExclusion("ikvm.test", "foo", null, null);
            a.Equals(null).Should().BeFalse();
            a.Equals((object)null).Should().BeFalse();
        }

        [TestMethod]
        public void ToString_should_return_coordinates()
        {
            new MavenReferenceItemExclusion("ikvm.test", "foo", null, null).ToString().Should().Be("ikvm.test:foo");
        }

    }

}
