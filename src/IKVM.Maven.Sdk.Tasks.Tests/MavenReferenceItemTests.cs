using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// The exclusion handling of <see cref="MavenReferenceItem.Equals(MavenReferenceItem)"/> and
    /// <see cref="MavenReferenceItem.GetHashCode"/> is covered by <see cref="MavenReferenceItemEqualityTests"/> and
    /// <see cref="MavenReferenceItemHashCodeTests"/>.
    /// </summary>
    [TestClass]
    public class MavenReferenceItemTests
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
            Exclusions = new[] { new MavenReferenceItemExclusion("ikvm.other", "bar", null, null) },
            ReferenceSource = "MavenReference",
        };

        [TestMethod]
        [DataRow("ItemSpec")]
        [DataRow("GroupId")]
        [DataRow("ArtifactId")]
        [DataRow("Classifier")]
        [DataRow("Version")]
        [DataRow("Scope")]
        [DataRow("ReferenceSource")]
        public void Should_not_equal_item_with_different_value(string property)
        {
            var a = CreateItem();
            var b = CreateItem();

            switch (property)
            {
                case "ItemSpec": b.ItemSpec = "other"; break;
                case "GroupId": b.GroupId = "other"; break;
                case "ArtifactId": b.ArtifactId = "other"; break;
                case "Classifier": b.Classifier = "other"; break;
                case "Version": b.Version = "other"; break;
                case "Scope": b.Scope = "runtime"; break;
                case "ReferenceSource": b.ReferenceSource = "PackageReference"; break;
                default: throw new NotSupportedException(property);
            }

            a.Equals(b).Should().BeFalse();
        }

        [TestMethod]
        public void Should_not_equal_other_type()
        {
            CreateItem().Equals("ikvm.test:foo").Should().BeFalse();
        }

        [TestMethod]
        public void ToString_should_return_coordinates()
        {
            CreateItem().ToString().Should().Be("ikvm.test:foo");
        }

    }

}
