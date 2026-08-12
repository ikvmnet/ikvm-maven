using System.Collections.Generic;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemHashCodeTests
    {

        static MavenReferenceItem CreateItem(params MavenReferenceItemExclusion[] exclusions) => new MavenReferenceItem()
        {
            ItemSpec = "ikvm.test:foo",
            GroupId = "ikvm.test",
            ArtifactId = "foo",
            Classifier = "cls",
            Version = "1.2.3",
            Optional = true,
            Scope = "compile",
            Exclusions = exclusions,
            ReferenceSource = "MavenReference",
        };

        /// <summary>
        /// Two items describing the same reference are produced independently on either side of a save and import, so
        /// they are never the same instance and their exclusion arrays are never the same array.
        /// </summary>
        [TestMethod]
        public void Should_produce_the_same_hash_code_for_equal_items()
        {
            var a = CreateItem(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null));
            var b = CreateItem(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null));

            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [TestMethod]
        public void Should_produce_the_same_hash_code_without_exclusions()
        {
            CreateItem().GetHashCode().Should().Be(CreateItem().GetHashCode());
        }

        [TestMethod]
        public void Should_not_throw_without_an_exclusion_array()
        {
            var f = () => new MavenReferenceItem().GetHashCode();
            f.Should().NotThrow();
        }

        [TestMethod]
        public void Should_take_the_exclusions_into_account()
        {
            var a = CreateItem(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null));
            var b = CreateItem(new MavenReferenceItemExclusion("ikvm.other", "baz", null, null));

            a.GetHashCode().Should().NotBe(b.GetHashCode());
        }

        /// <summary>
        /// The items are used as dictionary keys, which only works when equal items collide.
        /// </summary>
        [TestMethod]
        public void Should_allow_an_item_to_be_used_as_a_dictionary_key()
        {
            var d = new Dictionary<MavenReferenceItem, string>
            {
                [CreateItem(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null))] = "value",
            };

            d.TryGetValue(CreateItem(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null)), out var value).Should().BeTrue();
            value.Should().Be("value");
        }

    }

}
