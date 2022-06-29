using System.Collections.Generic;

using FluentAssertions;

using IKVM.Sdk.Maven.Tasks;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Sdk.Maven.Tests.Tasks
{

    [TestClass]
    public class MavenReferenceItemPrepareTests
    {

        [TestMethod]
        public void Should_work_with_itemspec_and_version_as_metadata()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:foo");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.2.3");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            i1.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i1.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i1.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.2.3");
            errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Should_work_with_itemspec_with_version()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:foo:1.2.3");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            i1.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i1.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i1.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.2.3");
            errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Should_fail_if_no_groupid_with_bad_itemspec()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:::::foo:1.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            errors.Should().Contain(x => x.Code == "MAVEN0005");
        }

        [TestMethod]
        public void Should_fail_if_no_artifactid_with_bad_itemspec()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:::::foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            errors.Should().Contain(x => x.Code == "MAVEN0006");
        }

        [TestMethod]
        public void Should_fail_if_no_version_with_bad_itemspec()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:::::foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "foo");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            errors.Should().Contain(x => x.Code == "MAVEN0007");
        }

        [TestMethod]
        public void Should_populate_metadata_from_coordinates()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("ikvm.test:foo:1.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            i1.ItemSpec.Should().Be("ikvm.test:foo:1.0");
            i1.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i1.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i1.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
        }

    }

}