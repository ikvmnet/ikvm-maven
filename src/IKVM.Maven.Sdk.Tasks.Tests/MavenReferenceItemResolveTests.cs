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

        /// <summary>
        /// Creates a task item for the local repository.
        /// </summary>
        /// <returns></returns>
        static ITaskItem GetLocalRepositoryItem()
        {
            var item = new TaskItem("local");
            item.SetMetadata("Url", java.nio.file.Paths.get(Path.Combine(Path.GetDirectoryName(typeof(MavenReferenceItemResolveTests).Assembly.Location), "repository")).toAbsolutePath().toUri().toString());
            return item;
        }

        /// <summary>
        /// Creates a task item for the local repository.
        /// </summary>
        /// <returns></returns>
        static ITaskItem GetPrivateRepositoryItem()
        {
            var item = new TaskItem("github");
            item.SetMetadata("Url", "https://maven.pkg.github.com/ikvmnet/*");
            return item;
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanResolve()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
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
        public void CanResolveWithClassifier()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
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
        public void CanResolveWithMajorVersionOnly()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
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
        public void CanResolvePackagingTypePom()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
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
        public void CanResolveWithCache()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));

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

        [TestMethod]
        public void CanResolveWithVersionOverride()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("name.dmaus.schxslt:cli:1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "name.dmaus.schxslt");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "cli");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            var i2 = new TaskItem("net.sf.saxon:Saxon-HE:11.4");
            i2.SetMetadata(MavenReferenceItemMetadata.GroupId, "net.sf.saxon");
            i2.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "Saxon-HE");
            i2.SetMetadata(MavenReferenceItemMetadata.Version, "11.4");
            i2.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1, i2 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$name.dmaus.schxslt:cli:1.9.1");
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$net.sf.saxon:Saxon-HE:11.4");
        }

        [TestMethod]
        public void CanResolveWithTransitiveDependencies()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.junit.platform:junit-platform-launcher:1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.junit.platform");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "junit-platform-launcher");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$org.junit.platform:junit-platform-commons:1.9.1");

            var r1 = t.ResolvedReferences.FirstOrDefault(i => i.ItemSpec == "maven$org.junit.platform:junit-platform-engine:1.9.1");
            r1.GetMetadata("References").Split(';').Should().Contain("maven$org.junit.platform:junit-platform-commons:1.9.1");

            var r2 = t.ResolvedReferences.FirstOrDefault(i => i.ItemSpec == "maven$org.junit.platform:junit-platform-launcher:1.9.1");
            r2.GetMetadata("References").Split(';').Should().Contain("maven$org.junit.platform:junit-platform-engine:1.9.1");
            r2.GetMetadata("References").Split(';').Should().Contain("maven$org.junit.platform:junit-platform-commons:1.9.1");
        }

        [TestMethod]
        public void ShouldIncludeUnifiedVersions()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.apache.tika:tika-parsers-standard-package:2.8.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.tika");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "tika-parsers-standard-package");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "2.8.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            // ensure we unified slf4j
            t.ResolvedReferences.Should().ContainSingle(i => i.ItemSpec.StartsWith("maven$org.slf4j:slf4j-api:"));
            t.ResolvedReferences.Should().ContainSingle(i => i.ItemSpec == "maven$org.slf4j:slf4j-api:2.0.7");

            // check for dep that goes missing
            var fontbox = t.ResolvedReferences.First(i => i.ItemSpec == "maven$org.apache.pdfbox:fontbox:2.0.28");
            fontbox.GetMetadata("References").Split(';').Should().Contain("maven$commons-logging:commons-logging:1.2");
        }

        [TestMethod]
        public void ShouldIncludeProvidedDependencies()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.xerial:sqlite-jdbc:3.42.0.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.xerial");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "sqlite-jdbc");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "3.42.0.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$org.graalvm.sdk:graal-sdk:22.3.2");
            var pkg = t.ResolvedReferences.First(i => i.ItemSpec == "maven$org.xerial:sqlite-jdbc:3.42.0.0");
            pkg.GetMetadata("References").Split(';').Should().Contain("maven$org.graalvm.sdk:graal-sdk:22.3.2");
        }

        [TestMethod]
        public void CompileDependencyShouldBePrivateAndReferenced()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
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
            var pkg = t.ResolvedReferences.First(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            pkg.GetMetadata("Private").Should().Be("true");
            pkg.GetMetadata("ReferenceOutputAssembly").Should().Be("true");
        }

        [TestMethod]
        public void RuntimeDependencyShouldBePrivateAndReferenced()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("javax.inject:javax.inject:1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "runtime");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            var pkg = t.ResolvedReferences.First(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            pkg.GetMetadata("Private").Should().Be("true");
            pkg.GetMetadata("ReferenceOutputAssembly").Should().Be("false");
        }

        [TestMethod]
        public void ProvidedDependencyShouldBeNotPrivateAndReferenced()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("javax.inject:javax.inject:1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "provided");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            var pkg = t.ResolvedReferences.First(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
            pkg.GetMetadata("Private").Should().Be("false");
            pkg.GetMetadata("ReferenceOutputAssembly").Should().Be("true");
        }

        [TestMethod]
        public void SystemDependencyShouldBeExcluded()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("javax.inject:javax.inject:1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "javax.inject");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "system");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().NotContain(i => i.ItemSpec == "maven$javax.inject:javax.inject:1");
        }

        [TestMethod]
        public void CanResolveApacheFop()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.apache.xmlgraphics:fop:2.8");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.xmlgraphics");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "fop");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "2.8");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$org.apache.xmlgraphics:fop:2.8");
            var pkg = t.ResolvedReferences.First(i => i.ItemSpec == "maven$org.apache.xmlgraphics:fop:2.8");
            pkg.GetMetadata("References").Split(';').Should().Contain("maven$javax.media:jai-core:1.1.3");
        }

        [TestMethod]
        public void CanResolveCircularDependency()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("org.apache.commons:commons-text:1.11.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.commons");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "commons-text");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.11.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            var pkg1 = t.ResolvedReferences.First(i => i.ItemSpec == "maven$org.apache.commons:commons-text:1.11.0");
            pkg1.GetMetadata("References").Split(';').Should().Contain("maven$org.apache.commons:commons-lang3:3.13.0");
            var pkg2 = t.ResolvedReferences.First(i => i.ItemSpec == "maven$org.apache.commons:commons-lang3:3.13.0");
            pkg2.GetMetadata("References").Split(';').Should().Contain("maven$org.apache.commons:commons-text:1.11.0");
        }

        [TestMethod]
        public void CanResolveFromLocalRepository()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem(), GetLocalRepositoryItem() };

            var i1 = new TaskItem("hellotest:hellotest:1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "hellotest");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "hellotest");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.0");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$hellotest:hellotest:1.0");
        }

        [TestMethod]
        public void CanResolveFromPrivateRepository()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem(), GetPrivateRepositoryItem() };

            var i1 = new TaskItem("org.apache.xmlgraphics:fop:2.8");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.xmlgraphics");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "fop");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "2.8");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");
            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$org.apache.xmlgraphics:fop:2.8");
        }

        [TestMethod]
        public void ExclusionsShouldExcludeSystemDependency()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("net.sf.jt400:jt400:20.0.6");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "net.sf.jt400");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "jt400");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "20.0.6");
            i1.SetMetadata(MavenReferenceItemMetadata.Exclusions, "com.sun:tools");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().NotContain(i => i.ItemSpec == "maven$com.sun:tools:jar:1.8.0");
        }

        [TestMethod]
        public void CanResolveClassifiers()
        {
            var cacheFile = Path.GetTempFileName();

            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => { errors.Add(e); TestContext.WriteLine("ERROR: " + e.Message); });
            engine.Setup(x => x.LogWarningEvent(It.IsAny<BuildWarningEventArgs>())).Callback((BuildWarningEventArgs e) => TestContext.WriteLine("WARNING: " + e.Message));
            engine.Setup(x => x.LogMessageEvent(It.IsAny<BuildMessageEventArgs>())).Callback((BuildMessageEventArgs e) => TestContext.WriteLine(e.Message));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;
            t.CacheFile = cacheFile;
            t.Repositories = new[] { GetCentralRepositoryItem() };

            var i1 = new TaskItem("edu.stanford.nlp:stanford-corenlp:4.5.5");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "edu.stanford.nlp");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "stanford-corenlp");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "4.5.5");
            i1.SetMetadata(MavenReferenceItemMetadata.Classifier, "models");
            i1.SetMetadata(MavenReferenceItemMetadata.Scope, "compile");

            t.References = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();

            t.ResolvedReferences.Should().Contain(i => i.ItemSpec == "maven$edu.stanford.nlp:stanford-corenlp:models:4.5.5");
        }

    }

}
