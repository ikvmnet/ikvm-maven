using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class MavenReferenceItemImportTests
    {

        readonly string binPath = Path.GetDirectoryName(typeof(MavenReferenceItemImportTests).Assembly.Location);

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanDiscoverProjectModelFiles()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemImport();
            t.BuildEngine = engine.Object;

            foreach (var tfm in new[] { "net6.0", "net8.0", "net481" })
            {
                var l = t.GetProjectObjectModelFiles(Path.Combine(binPath, "Test.project.assets.json"), tfm, null);
                l.Should().HaveCount(1);
                foreach (var pom in l)
                {
                    var d = MavenReferenceItemImport.GetProjectObjectModelFileDependencies(pom).ToList();
                    d.Should().HaveCount(1);
                }
            }
        }

    }

}