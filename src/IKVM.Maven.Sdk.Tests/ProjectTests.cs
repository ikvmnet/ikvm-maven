using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class ProjectTests
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
        public void CanBuildProject()
        {
            // times out on linux, need to figure this out
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
                return;

            var properties = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), "IKVM.Maven.Sdk.Tests.properties")).Select(i => i.Split('=', 2)).ToDictionary(i => i[0], i => i[1]);

            var nugetPackageRoot = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_Project", "nuget", "packages");
            if (Directory.Exists(nugetPackageRoot))
                Directory.Delete(nugetPackageRoot, true);
            Directory.CreateDirectory(nugetPackageRoot);

            var ikvmCachePath = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_Project", "ikvm", "cache");
            if (Directory.Exists(ikvmCachePath))
                Directory.Delete(ikvmCachePath, true);

            var ikvmExportCachePath = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_Project", "ikvm", "expcache");
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
                            new XAttribute("key", "nuget.org"),
                            new XAttribute("value", "https://api.nuget.org/v3/index.json")))))
                .Save(Path.Combine(@"Project", "nuget.config"));

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), @"Project", "Exe", "ProjectExe.csproj"));
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

            var targets = new[]
            {
                ("net461",          "win7-x86"),
                ("net472",          "win7-x86"),
                ("net48",           "win7-x86"),
                ("netcoreapp3.1",   "win7-x86"),
                ("net6.0",          "win7-x86"),
                ("net7.0",          "win7-x86"),
                ("net461",          "win7-x64"),
                ("net472",          "win7-x64"),
                ("net48",           "win7-x64"),
                ("netcoreapp3.1",   "win7-x64"),
                ("net6.0",          "win7-x64"),
                ("net7.0",          "win7-x64"),
                ("net461",          "win81-arm"),
                ("net472",          "win81-arm"),
                ("net48",           "win81-arm"),
                ("netcoreapp3.1",   "win81-arm"),
                ("net6.0",          "win81-arm"),
                ("net7.0",          "win81-arm"),
                ("netcoreapp3.1",   "linux-x64"),
                ("net6.0",          "linux-x64"),
                ("net7.0",          "linux-x64"),
                ("netcoreapp3.1",   "linux-arm"),
                ("net6.0",          "linux-arm"),
                ("net7.0",          "linux-arm"),
                ("netcoreapp3.1",   "linux-arm64"),
                ("net6.0",          "linux-arm64"),
                ("net7.0",          "linux-arm64"),
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                targets = new[]
                {
                    ("netcoreapp3.1",   "win7-x86"),
                    ("net6.0",          "win7-x86"),
                    ("net7.0",          "win7-x86"),
                    ("netcoreapp3.1",   "win7-x64"),
                    ("net6.0",          "win7-x64"),
                    ("net7.0",          "win7-x64"),
                    ("netcoreapp3.1",   "win81-arm"),
                    ("net6.0",          "win81-arm"),
                    ("net7.0",          "win81-arm"),
                    ("netcoreapp3.1",   "linux-x64"),
                    ("net6.0",          "linux-x64"),
                    ("net7.0",          "linux-x64"),
                    ("netcoreapp3.1",   "linux-arm"),
                    ("net6.0",          "linux-arm"),
                    ("net7.0",          "linux-arm"),
                    ("netcoreapp3.1",   "linux-arm64"),
                    ("net6.0",          "linux-arm64"),
                    ("net7.0",          "linux-arm64"),
                };
            }

            foreach (var (tfm, rid) in targets)
            {
                TestContext.WriteLine("Publishing with TargetFramework {0} and RuntimeIdentifier {1}.", tfm, rid);

                foreach (var configuration in new[] { "Debug", "Release" })
                {
                    var results = analyzer.Build(new EnvironmentOptions()
                    {
                        DesignTime = false,
                        Restore = true,
                        GlobalProperties =
                        {
                            ["Configuration"] = configuration,
                            ["TargetFramework"] = tfm,
                            ["RuntimeIdentifier"] = rid,
                        },
                        TargetsToBuild =
                        {
                            "Build",
                            "Publish"
                        }
                    });

                    results.OverallSuccess.Should().Be(true);
                }
            }
        }

    }

}