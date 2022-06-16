using IKVM.Sdk.Maven.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace IKVM.Sdk.Maven.Tests.Tasks
{

    [TestClass]
    public class GetMavenArtifactsTests
    {

        [TestMethod]
        public void Can_receive_maven_artifacts()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var e = new GetMavenArtifacts
            {
                MavenReferences = new ITaskItem[]
                {
                    new TaskItem("opennlp-tools", new Dictionary<string, string>
                    {
                        [IkvmMavenReferenceItemMetadata.GroupId] = "org.apache.opennlp",
                        [IkvmMavenReferenceItemMetadata.Version] = "1.9.1",
                        [IkvmMavenReferenceItemMetadata.Extension] = "jar"
                    }),
                    new TaskItem("opennlp-uima", new Dictionary<string, string>
                    {
                        [IkvmMavenReferenceItemMetadata.GroupId] = "org.apache.opennlp",
                        [IkvmMavenReferenceItemMetadata.Version] = "1.9.1",
                        [IkvmMavenReferenceItemMetadata.Extension] = "jar"
                    }),
                    new TaskItem("opennlp-morfologik-addon", new Dictionary<string, string>
                    {
                        [IkvmMavenReferenceItemMetadata.GroupId] = "org.apache.opennlp",
                        [IkvmMavenReferenceItemMetadata.Version] = "1.9.1",
                        [IkvmMavenReferenceItemMetadata.Extension] = "jar"
                    }),
                    new TaskItem("opennlp-brat-annotator", new Dictionary<string, string>
                    {
                        [IkvmMavenReferenceItemMetadata.GroupId] = "org.apache.opennlp",
                        [IkvmMavenReferenceItemMetadata.Version] = "1.9.1",
                        [IkvmMavenReferenceItemMetadata.Extension] = "jar"
                    })

                },
                DestinationFolder = tempFolder
            };
            e.Execute();

            Assert.AreEqual(37, e.MavenArtifacts.Length);
            Assert.AreEqual(37, Directory.GetFiles(tempFolder).Length);

            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch { }
        }
    }

}