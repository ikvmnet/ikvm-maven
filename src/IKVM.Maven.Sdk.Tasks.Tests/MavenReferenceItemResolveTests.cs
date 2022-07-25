using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemResolveTests
    {

        /// <summary>
        /// Creates a task item for the central repository.
        /// </summary>
        /// <returns></returns>
        static ITaskItem GetCentralRepositoryItem()
        {
            var item = new TaskItem("central");
            item.SetMetadata("Url", "https://repo1.maven.org/maven2/");
            return item;
        }

        [TestMethod]
        public void Can_resolve_maven_references()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.apache.opennlp:opennlp-brat-annotator:1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.opennlp");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "opennlp-brat-annotator");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            var i2 = new TaskItem("org.apache.maven:maven-core:3.8.6");
            i2.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.maven");
            i2.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "maven-core");
            i2.SetMetadata(MavenReferenceItemMetadata.Version, "3.8.6");
            i2.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1, i2 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec.StartsWith("maven$org.codehaus.plexus:plexus-utils:"));
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.ItemSpec));
            t.ResolvedReferences.Should().OnlyContain(i => i.ItemSpec.StartsWith("maven$"));
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.GetMetadata(IkvmReferenceItemMetadata.Compile)));
        }

        [TestMethod]
        public void Can_resolve_maven_references_with_classifier()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("com.google.inject:guice");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "com.google.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "guice");
            i1.SetMetadata(MavenReferenceItemMetadata.Classifier, "no_aop");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "4.2.2");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.google.inject:guice:no_aop:4.2.2");
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.ItemSpec));
            t.ResolvedReferences.Should().OnlyContain(i => i.ItemSpec.StartsWith("maven$"));
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.GetMetadata(IkvmReferenceItemMetadata.Compile)));
        }

        [TestMethod]
        public void Can_resolve_maven_references_with_major_version_only()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("javax.inject:javax.inject:1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.ItemSpec));
            t.ResolvedReferences.Should().OnlyContain(i => i.ItemSpec.StartsWith("maven$"));
            t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.GetMetadata(IkvmReferenceItemMetadata.Compile)));

            var r = t.ResolvedReferences.FirstOrDefault(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            r.GetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion).Should().Be("1.0");
        }

        [TestMethod]
        public void Can_resolve_maven_references_through_packaging_type_pom()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("com.yahoo.vespa:documentapi:8.12.48");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "com.yahoo.vespa");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "documentapi");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "8.12.48");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.yahoo.vespa:annotations:8.12.48");
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.yahoo.vespa:component:8.12.48");
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.yahoo.vespa:config:8.12.48");
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.yahoo.vespa:config-lib:8.12.48");
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$com.yahoo.vespa:configdefinitions:8.12.48");
        }

        [TestMethod]
        public void Can_resolve_maven_references_with_cache()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));

            {
                var t = new MavenReferenceItemResolve();
                t.BuildEngine = engine.Object;
                t.CacheFile = cacheFile;
                t.Repositories = new[] { GetCentralRepositoryItem() };

                var i1 = new TaskItem("javax.inject:javax.inject:1");
                i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
                i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
                i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
                i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
                t.References = new[] { i1 };
                t.Execute().Should().BeTrue();
                errors.Should().BeEmpty();

                t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
                t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.ItemSpec));
                t.ResolvedReferences.Should().OnlyContain(i => i.ItemSpec.StartsWith("maven$"));
                t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.GetMetadata(IkvmReferenceItemMetadata.Compile)));

                var r = t.ResolvedReferences.FirstOrDefault(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
                r.GetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion).Should().Be("1.0");
            }

            {
                var t = new MavenReferenceItemResolve();
                t.BuildEngine = engine.Object;
                t.CacheFile = cacheFile;
                t.Repositories = new[] { GetCentralRepositoryItem() };

                var i1 = new TaskItem("javax.inject:javax.inject:1");
                i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
                i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
                i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
                i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
                t.References = new[] { i1 };
                t.Execute().Should().BeTrue();
                errors.Should().BeEmpty();

                t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
                t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.ItemSpec));
                t.ResolvedReferences.Should().OnlyContain(i => i.ItemSpec.StartsWith("maven$"));
                t.ResolvedReferences.Should().OnlyContain(i => !string.IsNullOrWhiteSpace(i.GetMetadata(IkvmReferenceItemMetadata.Compile)));

                var r = t.ResolvedReferences.FirstOrDefault(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
                r.GetMetadata(IkvmReferenceItemMetadata.FallbackAssemblyVersion).Should().Be("1.0");
            }

        }

    }

}