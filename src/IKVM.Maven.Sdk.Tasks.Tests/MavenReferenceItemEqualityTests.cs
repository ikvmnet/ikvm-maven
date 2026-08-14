using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemEqualityTests
    {

        static MavenReferenceItem CreateItem() => new MavenReferenceItem()
        {
            ItemSpec = "ikvm.test:foo",
            GroupId = "ikvm.test",
            ArtifactId = "foo",
            Classifier = "cls",
            Version = "1.2.3",
            Optional = true,
            Scope = "compile",
            Aliases = "foo",
            Exclusions = new[] { new MavenReferenceItemExclusion("ikvm.other", "bar", null, null) },
            ReferenceSource = "MavenReference",
        };

        [TestMethod]
        public void Should_equal_itself()
        {
            var a = CreateItem();
            a.Equals(a).Should().BeTrue();
        }

        [TestMethod]
        public void Should_equal_an_item_with_the_same_values()
        {
            CreateItem().Equals(CreateItem()).Should().BeTrue();
        }

        [TestMethod]
        public void Should_not_equal_null()
        {
            CreateItem().Equals(null).Should().BeFalse();
            CreateItem().Equals((object)null).Should().BeFalse();
        }

        /// <summary>
        /// An item straight out of the constructor has no exclusions at all; comparing two of them must not throw.
        /// </summary>
        [TestMethod]
        public void Should_equal_default_instances()
        {
            new MavenReferenceItem().Equals(new MavenReferenceItem()).Should().BeTrue();
        }

        [TestMethod]
        public void Should_not_equal_an_item_with_different_exclusions()
        {
            var a = CreateItem();
            var b = CreateItem();
            b.Exclusions = new[] { new MavenReferenceItemExclusion("ikvm.other", "baz", null, null) };
            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_an_item_with_an_extra_exclusion()
        {
            var a = CreateItem();
            var b = CreateItem();
            b.Exclusions = new[]
            {
                new MavenReferenceItemExclusion("ikvm.other", "bar", null, null),
                new MavenReferenceItemExclusion("ikvm.other", "baz", null, null),
            };
            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_an_item_without_exclusions()
        {
            var a = CreateItem();
            var b = CreateItem();
            b.Exclusions = null;
            a.Equals(b).Should().BeFalse();
            b.Equals(a).Should().BeFalse();
        }

        [TestMethod]
        public void Should_still_compare_the_remaining_members()
        {
            var a = CreateItem();

            var b = CreateItem();
            b.Version = "2.0";
            a.Equals(b).Should().BeFalse();

            var c = CreateItem();
            c.Scope = "runtime";
            a.Equals(c).Should().BeFalse();

            var d = CreateItem();
            d.Optional = false;
            a.Equals(d).Should().BeFalse();

            var e = CreateItem();
            e.Aliases = "bar";
            a.Equals(e).Should().BeFalse();
        }

    }

}
