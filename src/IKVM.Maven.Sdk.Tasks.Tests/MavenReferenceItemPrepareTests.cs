using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.2.3");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            i1 = t.Items[0];
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.2.3");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            i1 = t.Items[0];
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:::::foo:1.0");
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:::::foo:1.0");
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:::::foo:1.0");
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

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            i1 = t.Items[0];
            i1.ItemSpec.Should().Be("ikvm.test:foo:1.0");
            i1.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i1.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i1.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
        }

        [TestMethod]
        public void ShouldNotRemoveDuplicateDependencies()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine.Object;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            var i2 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i2.SetMetadata(MavenReferenceItemMetadata.Classifier, "cls");
            t.Items = new[] { i1, i2 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.Items.Should().HaveCount(2);

            i1 = t.Items[0];
            i1.ItemSpec.Should().Be("ikvm.test:foo:1.0");
            i1.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i1.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i1.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
            i1.GetMetadata(MavenReferenceItemMetadata.Classifier).Should().BeNullOrEmpty();

            i2 = t.Items[1];
            i2.ItemSpec.Should().Be("ikvm.test:foo:1.0");
            i2.GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            i2.GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            i2.GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
            i2.GetMetadata(MavenReferenceItemMetadata.Classifier).Should().Be("cls");
        }

        [TestMethod]
        public void Should_default_scope_to_compile()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;
            t.Items = new[] { (ITaskItem)new TaskItem("ikvm.test:foo:1.0") };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();
            t.Items[0].GetMetadata(MavenReferenceItemMetadata.Scope).Should().Be("compile");
        }

        [TestMethod]
        [DataRow("compile")]
        [DataRow("runtime")]
        [DataRow("provided")]
        [DataRow("system")]
        [DataRow("test")]
        public void Should_accept_valid_scope(string scope)
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, scope);
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();
            t.Items[0].GetMetadata(MavenReferenceItemMetadata.Scope).Should().Be(scope);
        }

        [TestMethod]
        [DataRow("import")]
        [DataRow("Compile")]
        [DataRow("bogus")]
        public void Should_fail_on_invalid_scope(string scope)
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, scope);
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            engine.Errors.Should().Contain(x => x.Code == "MAVEN0009");
        }

        [TestMethod]
        public void Should_fail_if_groupid_metadata_conflicts_with_itemspec()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.other");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            engine.Errors.Should().Contain(x => x.Code == "MAVEN0002");
        }

        [TestMethod]
        public void Should_fail_if_artifactid_metadata_conflicts_with_itemspec()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "bar");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            engine.Errors.Should().Contain(x => x.Code == "MAVEN0003");
        }

        [TestMethod]
        public void Should_fail_if_version_metadata_conflicts_with_itemspec()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "2.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeFalse();
            engine.Errors.Should().Contain(x => x.Code == "MAVEN0004");
        }

        [TestMethod]
        public void Should_allow_version_metadata_matching_itemspec()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "foo");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Should_work_with_arbitrary_itemspec_and_full_metadata()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("foo-lib");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "ikvm.test");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "foo");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();

            t.Items[0].ItemSpec.Should().Be("foo-lib");
            t.Items[0].GetMetadata(MavenReferenceItemMetadata.GroupId).Should().Be("ikvm.test");
            t.Items[0].GetMetadata(MavenReferenceItemMetadata.ArtifactId).Should().Be("foo");
            t.Items[0].GetMetadata(MavenReferenceItemMetadata.Version).Should().Be("1.0");
        }

        [TestMethod]
        public void Should_fail_if_itemspec_has_no_version_and_no_version_metadata()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;
            t.Items = new[] { (ITaskItem)new TaskItem("ikvm.test:foo") };

            t.Execute().Should().BeFalse();
            engine.Errors.Should().Contain(x => x.Code == "MAVEN0007");
        }

        [TestMethod]
        public void Should_preserve_optional_exclusions_and_reference_source()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Optional, "true");
            i1.SetMetadata(MavenReferenceItemMetadata.Exclusions, "ikvm.other:bar;ikvm.other:baz");
            i1.SetMetadata(MavenReferenceItemMetadata.ReferenceSource, "ProjectReference");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();

            var o = t.Items[0];
            o.GetMetadata(MavenReferenceItemMetadata.Optional).Should().Be("true");
            o.GetMetadata(MavenReferenceItemMetadata.Exclusions).Split(';').Should().BeEquivalentTo(new[] { "ikvm.other:bar", "ikvm.other:baz" });
            o.GetMetadata(MavenReferenceItemMetadata.ReferenceSource).Should().Be("ProjectReference");
        }

        /// <summary>
        /// The prepare task is invoked repeatedly across a multi-targeting build, so running it against its own
        /// output must be a no-op.
        /// </summary>
        [TestMethod]
        public void Should_be_idempotent()
        {
            var engine = new TestBuildEngine();

            var i1 = (ITaskItem)new TaskItem("ikvm.test:foo:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Classifier, "cls");
            i1.SetMetadata(MavenReferenceItemMetadata.Exclusions, "ikvm.other:bar:cls:jar");

            var t1 = new MavenReferenceItemPrepare() { BuildEngine = engine, Items = new[] { i1 } };
            t1.Execute().Should().BeTrue();

            var t2 = new MavenReferenceItemPrepare() { BuildEngine = engine, Items = t1.Items };
            t2.Execute().Should().BeTrue();

            engine.Errors.Should().BeEmpty();
            MavenReferenceItemMetadata.Import(t2.Items).Should().BeEquivalentTo(MavenReferenceItemMetadata.Import(t1.Items));
        }

        [TestMethod]
        public void Should_succeed_with_no_items()
        {
            var engine = new TestBuildEngine();
            var t = new MavenReferenceItemPrepare();
            t.BuildEngine = engine;
            t.Items = System.Array.Empty<ITaskItem>();

            t.Execute().Should().BeTrue();
            engine.Errors.Should().BeEmpty();
            t.Items.Should().BeEmpty();
        }

    }

}