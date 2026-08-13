using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// Exclusion metadata is written by <see cref="MavenReferenceItemMetadata.Save"/> and read back by
    /// <see cref="MavenReferenceItemMetadata.Import"/>. Every item passes through that pair on each invocation of the
    /// prepare task, so anything the pair cannot represent is lost before resolution ever sees it.
    /// </summary>
    [TestClass]
    public class MavenReferenceItemExclusionMetadataTests
    {

        static MavenReferenceItemExclusion[] RoundTrip(params MavenReferenceItemExclusion[] exclusions)
        {
            var task = (ITaskItem)new TaskItem();
            MavenReferenceItemMetadata.Save(new MavenReferenceItem() { ItemSpec = "ikvm.test:foo", Exclusions = exclusions }, task);
            return MavenReferenceItemMetadata.Import(new[] { task })[0].Exclusions;
        }

        [TestMethod]
        public void Should_round_trip_group_and_artifact_id()
        {
            var e = RoundTrip(new MavenReferenceItemExclusion("ikvm.other", "bar", null, null)).Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.other");
            e.ArtifactId.Should().Be("bar");
            e.Classifier.Should().BeNull();
            e.Extension.Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_a_classifier()
        {
            var e = RoundTrip(new MavenReferenceItemExclusion("ikvm.other", "bar", "cls", null)).Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.other");
            e.ArtifactId.Should().Be("bar");
            e.Classifier.Should().Be("cls");
            e.Extension.Should().BeNull();
        }

        [TestMethod]
        public void Should_round_trip_a_classifier_and_extension()
        {
            var e = RoundTrip(new MavenReferenceItemExclusion("ikvm.other", "bar", "cls", "jar")).Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.other");
            e.ArtifactId.Should().Be("bar");
            e.Classifier.Should().Be("cls");
            e.Extension.Should().Be("jar");
        }

        [TestMethod]
        public void Should_round_trip_an_extension_without_a_classifier()
        {
            var e = RoundTrip(new MavenReferenceItemExclusion("ikvm.other", "bar", null, "jar")).Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.other");
            e.ArtifactId.Should().Be("bar");
            e.Extension.Should().Be("jar");
        }

        [TestMethod]
        public void Should_round_trip_several_exclusions()
        {
            RoundTrip(
                new MavenReferenceItemExclusion("ikvm.other", "bar", null, null),
                new MavenReferenceItemExclusion("ikvm.other", "baz", "cls", "jar"))
                .Should().BeEquivalentTo(new[]
                {
                    new MavenReferenceItemExclusion("ikvm.other", "bar", null, null),
                    new MavenReferenceItemExclusion("ikvm.other", "baz", "cls", "jar"),
                });
        }

        /// <summary>
        /// The prepare task runs against its own output on repeated invocations, so a second pass must not degrade
        /// what a first pass produced.
        /// </summary>
        [TestMethod]
        public void Prepare_should_preserve_a_fully_qualified_exclusion()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Exclusions, "ikvm.other:bar:cls:jar");

            var t1 = new MavenReferenceItemPrepare() { BuildEngine = engine.Object, Items = new[] { i1 } };
            t1.Execute().Should().BeTrue();

            var t2 = new MavenReferenceItemPrepare() { BuildEngine = engine.Object, Items = t1.Items };
            t2.Execute().Should().BeTrue();

            errors.Should().BeEmpty();

            var e = MavenReferenceItemMetadata.Import(t2.Items).Single().Exclusions.Should().ContainSingle().Subject;
            e.GroupId.Should().Be("ikvm.other");
            e.ArtifactId.Should().Be("bar");
            e.Classifier.Should().Be("cls");
            e.Extension.Should().Be("jar");
        }

    }

}
