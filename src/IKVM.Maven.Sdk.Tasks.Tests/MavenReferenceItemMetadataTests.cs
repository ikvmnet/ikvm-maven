using System;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemMetadataTests
    {

        [TestMethod]
        public void Import_should_throw_on_null()
        {
            var f = () => MavenReferenceItemMetadata.Import(null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Save_should_throw_on_null_item()
        {
            var f = () => MavenReferenceItemMetadata.Save(null, new TaskItem("foo"));
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Save_should_throw_on_null_task()
        {
            var f = () => MavenReferenceItemMetadata.Save(new MavenReferenceItem(), null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Import_should_read_all_metadata()
        {
            var i = (ITaskItem)new TaskItem("ikvm.test:foo");
            i.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "foo");
            i.SetMetadata(MavenReferenceItemMetadata.Classifier, "cls");
            i.SetMetadata(MavenReferenceItemMetadata.Version, "1.2.3");
            i.SetMetadata(MavenReferenceItemMetadata.Optional, "true");
            i.SetMetadata(MavenReferenceItemMetadata.Scope, "runtime");
            i.SetMetadata(MavenReferenceItemMetadata.Aliases, "foo,bar");
            i.SetMetadata(MavenReferenceItemMetadata.Exclusions, "ikvm.other:bar");
            i.SetMetadata(MavenReferenceItemMetadata.ReferenceSource, "PackageReference");

            var item = MavenReferenceItemMetadata.Import(new[] { i }).Should().ContainSingle().Subject;
            item.ItemSpec.Should().Be("ikvm.test:foo");
            item.GroupId.Should().Be("ikvm.test");
            item.ArtifactId.Should().Be("foo");
            item.Classifier.Should().Be("cls");
            item.Version.Should().Be("1.2.3");
            item.Optional.Should().BeTrue();
            item.Scope.Should().Be("runtime");
            item.Aliases.Should().Be("foo,bar");
            item.Exclusions.Should().ContainSingle().Which.Should().Be(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null));
            item.ReferenceSource.Should().Be("PackageReference");
        }

        [TestMethod]
        public void Import_should_default_missing_metadata()
        {
            var item = MavenReferenceItemMetadata.Import(new[] { (ITaskItem)new TaskItem("ikvm.test:foo") }).Should().ContainSingle().Subject;
            item.GroupId.Should().BeEmpty();
            item.ArtifactId.Should().BeEmpty();
            item.Classifier.Should().BeEmpty();
            item.Version.Should().BeEmpty();
            item.Scope.Should().BeEmpty();
            item.Aliases.Should().BeEmpty();
            item.Optional.Should().BeFalse();
            item.Exclusions.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Should_round_trip_aliases()
        {
            var task = (ITaskItem)new TaskItem();
            MavenReferenceItemMetadata.Save(new MavenReferenceItem() { ItemSpec = "ikvm.test:foo", Aliases = "foo,bar" }, task);
            MavenReferenceItemMetadata.Import(new[] { task })[0].Aliases.Should().Be("foo,bar");
        }

        [TestMethod]
        [DataRow("true", true)]
        [DataRow("TRUE", true)]
        [DataRow("True", true)]
        [DataRow("false", false)]
        [DataRow("", false)]
        [DataRow("yes", false)]
        [DataRow("1", false)]
        public void Import_should_parse_optional_case_insensitively(string value, bool expected)
        {
            var i = (ITaskItem)new TaskItem("ikvm.test:foo");
            i.SetMetadata(MavenReferenceItemMetadata.Optional, value);
            MavenReferenceItemMetadata.Import(new[] { i })[0].Optional.Should().Be(expected);
        }

        [TestMethod]
        public void Import_should_parse_exclusion_with_group_and_artifact()
        {
            var e = ImportExclusions("ikvm.test:foo").Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.test");
            e.ArtifactId.Should().Be("foo");
            e.Classifier.Should().BeNull();
            e.Extension.Should().BeNull();
        }

        [TestMethod]
        public void Import_should_parse_exclusion_with_classifier()
        {
            var e = ImportExclusions("ikvm.test:foo:cls").Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.test");
            e.ArtifactId.Should().Be("foo");
            e.Classifier.Should().Be("cls");
            e.Extension.Should().BeNull();
        }

        [TestMethod]
        public void Import_should_parse_exclusion_with_classifier_and_extension()
        {
            var e = ImportExclusions("ikvm.test:foo:cls:jar").Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.test");
            e.ArtifactId.Should().Be("foo");
            e.Classifier.Should().Be("cls");
            e.Extension.Should().Be("jar");
        }

        [TestMethod]
        public void Import_should_parse_multiple_exclusions_and_skip_empty_entries()
        {
            ImportExclusions("ikvm.test:foo;;ikvm.test:bar;").Should().HaveCount(2);
        }

        [TestMethod]
        [DataRow("ikvm.test")]
        [DataRow("a:b:c:d:e")]
        public void Import_should_skip_unparsable_exclusions(string value)
        {
            ImportExclusions(value).Should().BeEmpty();
        }

        [TestMethod]
        public void Save_should_write_all_metadata()
        {
            var item = new MavenReferenceItem()
            {
                ItemSpec = "ikvm.test:foo",
                GroupId = "ikvm.test",
                ArtifactId = "foo",
                Classifier = "cls",
                Version = "1.2.3",
                Optional = true,
                Scope = "provided",
                Exclusions = new[] { new MavenReferenceItemExclusion("ikvm.other", "bar", null, null) },
                ReferenceSource = "MavenReference",
            };

            var task = (ITaskItem)new TaskItem();
            MavenReferenceItemMetadata.Save(item, task);

            task.ItemSpec.Should().Be("ikvm.test:foo");
            task.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            task.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            task.GetMetadata(MavenReferenceItemMetadata.Classifier).Should().Be("cls");
            task.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.2.3");
            task.GetMetadata(MavenReferenceItemMetadata.Optional).Should().Be("true");
            task.GetMetadata(MavenReferenceItemMetadata.Scope).Should().Be("provided");
            task.GetMetadata(MavenReferenceItemMetadata.Exclusions).Should().Be("ikvm.other:bar");
            task.GetMetadata(MavenReferenceItemMetadata.ReferenceSource).Should().Be("MavenReference");
        }

        [TestMethod]
        public void Save_should_write_optional_false()
        {
            var task = (ITaskItem)new TaskItem();
            MavenReferenceItemMetadata.Save(new MavenReferenceItem() { ItemSpec = "ikvm.test:foo", Optional = false }, task);
            task.GetMetadata(MavenReferenceItemMetadata.Optional).Should().Be("false");
        }

        /// <summary>
        /// The prepare task round trips every item through <see cref="MavenReferenceItemMetadata.Save"/> and back
        /// through <see cref="MavenReferenceItemMetadata.Import"/>, so no information may be lost by the pair.
        /// </summary>
        [TestMethod]
        public void Should_round_trip_through_save_and_import()
        {
            var item = new MavenReferenceItem()
            {
                ItemSpec = "ikvm.test:foo",
                GroupId = "ikvm.test",
                ArtifactId = "foo",
                Classifier = "cls",
                Version = "1.2.3",
                Optional = true,
                Scope = "test",
                Exclusions = new[]
                {
                    new MavenReferenceItemExclusion("ikvm.other", "bar", null, null),
                    new MavenReferenceItemExclusion("ikvm.other", "baz", "cls", "jar"),
                },
                ReferenceSource = "ProjectReference",
            };

            var task = (ITaskItem)new TaskItem();
            MavenReferenceItemMetadata.Save(item, task);
            var result = MavenReferenceItemMetadata.Import(new[] { task })[0];

            result.ItemSpec.Should().Be(item.ItemSpec);
            result.GroupId.Should().Be(item.GroupId);
            result.ArtifactId.Should().Be(item.ArtifactId);
            result.Classifier.Should().Be(item.Classifier);
            result.Version.Should().Be(item.Version);
            result.Optional.Should().Be(item.Optional);
            result.Scope.Should().Be(item.Scope);
            result.ReferenceSource.Should().Be(item.ReferenceSource);
            result.Exclusions.Should().BeEquivalentTo(item.Exclusions);
        }

        static MavenReferenceItemExclusion[] ImportExclusions(string value)
        {
            var i = (ITaskItem)new TaskItem("ikvm.test:foo");
            i.SetMetadata(MavenReferenceItemMetadata.Exclusions, value);
            return MavenReferenceItemMetadata.Import(new[] { i })[0].Exclusions;
        }

    }

}
