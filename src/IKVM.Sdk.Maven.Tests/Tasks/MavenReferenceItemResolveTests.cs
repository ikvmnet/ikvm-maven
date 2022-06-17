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
    public class MavenReferenceItemResolveTests
    {

        [TestMethod]
        public void Can_resolve_maven_references()
        {
            var engine = new Mock<IBuildEngine>();
            var errors = new List<BuildErrorEventArgs>();
            engine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>())).Callback((BuildErrorEventArgs e) => errors.Add(e));
            var t = new MavenReferenceItemResolve();
            t.BuildEngine = engine.Object;

            var i1 = new TaskItem("org.apache.opennlp:opennlp-brat-annotator:1.9.1");
            i1.SetMetadata(MavenReferenceItemMetadata.GroupId, "org.apache.opennlp");
            i1.SetMetadata(MavenReferenceItemMetadata.ArtifactId, "opennlp-brat-annotator");
            i1.SetMetadata(MavenReferenceItemMetadata.Version, "1.9.1");
            t.Items = new[] { i1 };

            t.Execute().Should().BeTrue();
            errors.Should().BeEmpty();
        }

    }

}