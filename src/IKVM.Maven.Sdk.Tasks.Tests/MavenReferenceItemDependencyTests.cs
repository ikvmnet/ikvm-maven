using System;

using FluentAssertions;

using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemDependencyTests
    {

        [TestMethod]
        public void CanParseSimpleDependency()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().BeEmpty();
            d.GroupId.Should().Be("com.fasterxml.jackson.core");
            d.ArtifactId.Should().Be("jackson-databind");
            d.Extension.Should().Be("jar");
            d.Classifier.Should().BeEmpty();
            d.Version.Should().Be("2.18.2");
            d.Scope.Should().Be("compile");
            d.Optional.Should().BeFalse();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseDependencyWithPath()
        {
            var d = MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().HaveCount(1);
            d.Path[0].GroupId.Should().Be("com.jayway.jsonpath");
            d.Path[0].ArtifactId.Should().Be("json-path");
            d.Path[0].Version.Should().BeNull();
            d.GroupId.Should().Be("com.fasterxml.jackson.core");
            d.ArtifactId.Should().Be("jackson-databind");
            d.Version.Should().Be("2.18.2");
            d.ToString().Should().Be("com.jayway.jsonpath:json-path/com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseDependencyWithVersionedPath()
        {
            var d = MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path:2.10.0/com.fasterxml.jackson.core:jackson-databind:2.18.2");
            d.Path.Should().HaveCount(1);
            d.Path[0].Version.Should().Be("2.10.0");
            d.ToString().Should().Be("com.jayway.jsonpath:json-path:2.10.0/com.fasterxml.jackson.core:jackson-databind:2.18.2");
        }

        [TestMethod]
        public void CanParseDependencyWithExtension()
        {
            var d = MavenReferenceItemDependency.Parse("org.example:thing:zip:1.0");
            d.Extension.Should().Be("zip");
            d.Classifier.Should().BeEmpty();
            d.Version.Should().Be("1.0");
            d.ToString().Should().Be("org.example:thing:zip:1.0");
        }

        [TestMethod]
        public void CanParseDependencyWithExtensionAndClassifier()
        {
            var d = MavenReferenceItemDependency.Parse("edu.stanford.nlp:stanford-corenlp:jar:models:4.5.5");
            d.Extension.Should().Be("jar");
            d.Classifier.Should().Be("models");
            d.Version.Should().Be("4.5.5");
            d.ToString().Should().Be("edu.stanford.nlp:stanford-corenlp:jar:models:4.5.5");
        }

        [TestMethod]
        public void CanParseOptionalDependency()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2,optional=true");
            d.Optional.Should().BeTrue();
            d.Scope.Should().Be("compile");
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2,optional=true");
        }

        [TestMethod]
        public void CanParseDependencyWithScope()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2,scope=runtime");
            d.Scope.Should().Be("runtime");
            d.Optional.Should().BeFalse();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2,scope=runtime");
        }

        [TestMethod]
        public void CanParseDependencyWithScopeAndOptional()
        {
            var d = MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2,scope=runtime,optional=true");
            d.Scope.Should().Be("runtime");
            d.Optional.Should().BeTrue();
            d.ToString().Should().Be("com.fasterxml.jackson.core:jackson-databind:2.18.2,scope=runtime,optional=true");
        }

        [TestMethod]
        public void ShouldThrowOnMissingVersion()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldThrowOnEmptySegment()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.jayway.jsonpath:json-path//com.fasterxml.jackson.core:jackson-databind:2.18.2");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldThrowOnUnknownQualifier()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2,exclude=foo");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldThrowOnMalformedQualifier()
        {
            var a = () => MavenReferenceItemDependency.Parse("com.fasterxml.jackson.core:jackson-databind:2.18.2,optional");
            a.Should().Throw<MavenTaskException>();
        }

        [TestMethod]
        public void ShouldRoundTripThroughMetadata()
        {
            var d = MavenReferenceItemDependency.Parse("a:b/c:d:1.0/e:f:2.0,scope=runtime,optional=true");
            var r = MavenReferenceItemDependency.Parse(d.ToString());
            r.Should().Be(d);
        }

        /// <summary>
        /// The prepare task imports every item it is given and saves it again, so the declarations pass through this
        /// pair on each invocation. Anything the encoding drops is dropped on the first build.
        /// </summary>
        [TestMethod]
        public void ShouldRoundTripThroughSaveAndImport()
        {
            var item = new MavenReferenceItem()
            {
                ItemSpec = "ikvm.test:foo",
                GroupId = "ikvm.test",
                ArtifactId = "foo",
                Version = "1.0",
                Dependencies = new[]
                {
                    MavenReferenceItemDependency.Parse("a:b:1.0"),
                    MavenReferenceItemDependency.Parse("c:d/e:f:2.0,scope=runtime,optional=true"),
                    MavenReferenceItemDependency.Parse("g:h:1.0/i:j:zip:linux-x86_64:3.0"),
                },
            };

            var task = new TaskItem();
            MavenReferenceItemMetadata.Save(item, task);

            MavenReferenceItemMetadata.Import(new[] { task })[0].Dependencies.Should().BeEquivalentTo(item.Dependencies);
        }

        [TestMethod]
        public void ShouldImportEmptyDependenciesWhenMetadataIsMissing()
        {
            MavenReferenceItemMetadata.Import(new[] { new TaskItem("ikvm.test:foo") })[0].Dependencies.Should().NotBeNull().And.BeEmpty();
        }

    }

}
