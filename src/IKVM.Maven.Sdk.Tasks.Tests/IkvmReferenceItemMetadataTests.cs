using System;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// The IkvmReferenceItem metadata is the hand-off to IKVM.NET.Sdk, which reads it back out of MSBuild item
    /// metadata; the encoding of the list-valued members is part of that contract.
    /// </summary>
    [TestClass]
    public class IkvmReferenceItemMetadataTests
    {

        [TestMethod]
        public void Save_should_throw_on_null_item()
        {
            var f = () => IkvmReferenceItemMetadata.Save(null, new TaskItem("foo"));
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Save_should_throw_on_null_task()
        {
            var f = () => IkvmReferenceItemMetadata.Save(new IkvmReferenceItem(), null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Save_should_write_all_metadata()
        {
            var item = new IkvmReferenceItem()
            {
                ItemSpec = "maven$ikvm.test:foo:1.0",
                AssemblyName = "foo",
                AssemblyVersion = "1.0.0.0",
                DisableAutoAssemblyName = true,
                DisableAutoAssemblyVersion = true,
                FallbackAssemblyName = "foo",
                FallbackAssemblyVersion = "1.0",
                ClassLoader = "ikvm.runtime.AppDomainAssemblyClassLoader",
                Debug = true,
                KeyFile = "key.snk",
                DelaySign = true,
                Aliases = "foo,bar",
                Private = false,
                ReferenceOutputAssembly = false,
                IkvmIdentity = "identity",
                MavenGroupId = "ikvm.test",
                MavenArtifactId = "foo",
                MavenClassifier = "cls",
                MavenVersion = "1.0",
            };

            var task = (ITaskItem)new TaskItem();
            IkvmReferenceItemMetadata.Save(item, task);

            task.ItemSpec.Should().Be("maven$ikvm.test:foo:1.0");
            task.GetMetadata(IkvmReferenceItemMetadata.AssemblyName).Should().Be("foo");
            task.GetMetadata(IkvmReferenceItemMetadata.AssemblyVersion).Should().Be("1.0.0.0");
            task.GetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyName).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyVersion).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyName).Should().Be("foo");
            task.GetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion).Should().Be("1.0");
            task.GetMetadata(IkvmReferenceItemMetadata.ClassLoader).Should().Be("ikvm.runtime.AppDomainAssemblyClassLoader");
            task.GetMetadata(IkvmReferenceItemMetadata.Debug).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.KeyFile).Should().Be("key.snk");
            task.GetMetadata(IkvmReferenceItemMetadata.DelaySign).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.Aliases).Should().Be("foo,bar");
            task.GetMetadata(IkvmReferenceItemMetadata.Private).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.ReferenceOutputAssembly).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.IkvmIdentity).Should().Be("identity");
            task.GetMetadata(IkvmReferenceItemMetadata.MavenGroupId).Should().Be("ikvm.test");
            task.GetMetadata(IkvmReferenceItemMetadata.MavenArtifactId).Should().Be("foo");
            task.GetMetadata(IkvmReferenceItemMetadata.MavenClassifier).Should().Be("cls");
            task.GetMetadata(IkvmReferenceItemMetadata.MavenVersion).Should().Be("1.0");
        }

        [TestMethod]
        public void Save_should_write_defaults_for_a_new_item()
        {
            var task = (ITaskItem)new TaskItem();
            IkvmReferenceItemMetadata.Save(new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:foo:1.0" }, task);

            task.GetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyName).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.DisableAutoAssemblyVersion).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.Debug).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.DelaySign).Should().Be("false");
            task.GetMetadata(IkvmReferenceItemMetadata.Private).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.ReferenceOutputAssembly).Should().Be("true");
            task.GetMetadata(IkvmReferenceItemMetadata.Compile).Should().BeEmpty();
            task.GetMetadata(IkvmReferenceItemMetadata.Sources).Should().BeEmpty();
            task.GetMetadata(IkvmReferenceItemMetadata.References).Should().BeEmpty();
        }

        [TestMethod]
        public void Save_should_join_list_metadata_with_the_property_separator()
        {
            var reference = new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:bar:1.0" };
            var item = new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:foo:1.0" };
            item.Compile.Add("foo-1.0.jar");
            item.Compile.Add("foo-extra-1.0.jar");
            item.Sources.Add("foo-1.0-sources.jar");
            item.References.Add(reference);

            var task = (ITaskItem)new TaskItem();
            IkvmReferenceItemMetadata.Save(item, task);

            task.GetMetadata(IkvmReferenceItemMetadata.Compile).Should().Be("foo-1.0.jar;foo-extra-1.0.jar");
            task.GetMetadata(IkvmReferenceItemMetadata.Sources).Should().Be("foo-1.0-sources.jar");
            task.GetMetadata(IkvmReferenceItemMetadata.References).Should().Be("maven$ikvm.test:bar:1.0");
        }

        /// <summary>
        /// References are written as itemspecs; a reference cycle would otherwise recurse forever.
        /// </summary>
        [TestMethod]
        public void Save_should_write_references_as_itemspecs()
        {
            var a = new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:a:1.0" };
            var b = new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:b:1.0" };
            a.References.Add(b);
            b.References.Add(a);

            var task = (ITaskItem)new TaskItem();
            IkvmReferenceItemMetadata.Save(a, task);
            task.GetMetadata(IkvmReferenceItemMetadata.References).Should().Be("maven$ikvm.test:b:1.0");
        }

        [TestMethod]
        public void ToString_should_return_the_itemspec()
        {
            new IkvmReferenceItem() { ItemSpec = "maven$ikvm.test:foo:1.0" }.ToString().Should().Be("maven$ikvm.test:foo:1.0");
        }

    }

}
