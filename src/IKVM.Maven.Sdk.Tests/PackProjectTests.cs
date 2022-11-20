using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Buildalyzer;
using Buildalyzer.Environment;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tests
{

    [TestClass]
    public class PackProjectTests
    {

        /// <summary>
        /// Forwards MSBuild events to the test context.
        /// </summary>
        class TargetLogger : Logger
        {

            readonly TestContext context;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="context"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public TargetLogger(TestContext context)
            {
                this.context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public override void Initialize(IEventSource eventSource)
            {
                eventSource.AnyEventRaised += (sender, evt) => context.WriteLine(evt.Message);
            }

        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanPackProject()
        {
            var properties = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), "IKVM.Maven.Sdk.Tests.properties")).Select(i => i.Split('=', 2)).ToDictionary(i => i[0], i => i[1]);

            var nugetPackageRoot = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_PackProject", "nuget", "packages");
            if (Directory.Exists(nugetPackageRoot))
                Directory.Delete(nugetPackageRoot, true);
            Directory.CreateDirectory(nugetPackageRoot);

            var ikvmCachePath = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_PackProject", "ikvm", "cache");
            if (Directory.Exists(ikvmCachePath))
                Directory.Delete(ikvmCachePath, true);

            var ikvmExportCachePath = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_PackProject", "ikvm", "expcache");
            if (Directory.Exists(ikvmExportCachePath))
                Directory.Delete(ikvmExportCachePath, true);

            new XDocument(
                new XElement("configuration",
                    new XElement("config",
                        new XElement("add",
                            new XAttribute("key", "globalPackagesFolder"),
                            new XAttribute("value", nugetPackageRoot))),
                    new XElement("packageSources",
                        new XElement("clear"),
                        new XElement("add",
                            new XAttribute("key", "dev"),
                            new XAttribute("value", Path.GetFullPath(@"nuget"))),
                        new XElement("add",
                            new XAttribute("key", "ikvm"),
                            new XAttribute("value", "https://nuget.pkg.github.com/ikvm-revived/index.json")),
                        new XElement("add",
                            new XAttribute("key", "nuget.org"),
                            new XAttribute("value", "https://api.nuget.org/v3/index.json")))))
                .Save(Path.Combine(@"PackProject", "nuget.config"));

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), @"PackProject", "Lib", "PackProjectLib.csproj"));
            analyzer.SetGlobalProperty("ImportDirectoryBuildProps", "false");
            analyzer.SetGlobalProperty("ImportDirectoryBuildTargets", "false");
            analyzer.SetGlobalProperty("IkvmCacheDir", ikvmCachePath + Path.DirectorySeparatorChar);
            analyzer.SetGlobalProperty("IkvmExportCacheDir", ikvmExportCachePath + Path.DirectorySeparatorChar);
            analyzer.SetGlobalProperty("PackageVersion", properties["PackageVersion"]);
            analyzer.SetGlobalProperty("RestorePackagesPath", nugetPackageRoot + Path.DirectorySeparatorChar);
            analyzer.SetGlobalProperty("CreateHardLinksForAdditionalFilesIfPossible", "true");
            analyzer.SetGlobalProperty("CreateHardLinksForCopyAdditionalFilesIfPossible", "true");
            analyzer.SetGlobalProperty("CreateHardLinksForCopyFilesToOutputDirectoryIfPossible", "true");
            analyzer.SetGlobalProperty("CreateHardLinksForCopyLocalIfPossible", "true");
            analyzer.SetGlobalProperty("CreateHardLinksForPublishFilesIfPossible", "true");

            analyzer.AddBuildLogger(new TargetLogger(TestContext) { Verbosity = LoggerVerbosity.Detailed });

            foreach (var configuration in new[] { "Debug", "Release" })
            {
                var results = analyzer.Build(new EnvironmentOptions()
                {
                    DesignTime = false,
                    Restore = true,
                    GlobalProperties =
                    {
                        ["Configuration"] = configuration,
                    },
                    TargetsToBuild =
                    {
                        "Pack"
                    }
                });

                results.OverallSuccess.Should().Be(true);
            }
        }

    }

}
