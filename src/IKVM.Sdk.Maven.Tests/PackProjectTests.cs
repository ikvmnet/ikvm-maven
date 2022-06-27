using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Buildalyzer;
using Buildalyzer.Environment;

using FluentAssertions;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Sdk.Maven.Tests
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
                eventSource.AnyEventRaised += (sender, evt) => context.WriteLine($"{evt.Message}");
            }

        }

        public TestContext TestContext { get; set; }

        /// <summary>
        /// Creates an analyzer for the given project.
        /// </summary>
        /// <param name="projectFile"></param>
        /// <param name="additionalPackageDir"></param>
        /// <returns></returns>
        IProjectAnalyzer CreateAnalyzer(string projectFile, string additionalPackageDir)
        {
            var nugetPackageRoot = Path.Combine(Path.GetTempPath(), "IKVM.Sdk.Maven.Tests_Package", "nuget", "packages");
            if (Directory.Exists(nugetPackageRoot))
                Directory.Delete(nugetPackageRoot, true);
            Directory.CreateDirectory(nugetPackageRoot);

            var nugetSources = new List<string>();
            nugetSources.Add("https://api.nuget.org/v3/index.json");
            nugetSources.Add(Path.GetFullPath("nuget"));
            if (additionalPackageDir is not null)
                nugetSources.Add(additionalPackageDir);

            var properties = File.ReadAllLines("IKVM.Sdk.Maven.Tests.properties").Select(i => i.Split('=', 2)).ToDictionary(i => i[0], i => i[1]);
            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(projectFile);
            analyzer.AddBuildLogger(new TargetLogger(TestContext));
            analyzer.SetGlobalProperty("ImportDirectoryBuildProps", "false");
            analyzer.SetGlobalProperty("ImportDirectoryBuildTargets", "false");
            analyzer.SetGlobalProperty("PackageVersion", properties["PackageVersion"]);
            analyzer.SetGlobalProperty("RestoreSources", string.Join("%3B", nugetSources));
            analyzer.SetGlobalProperty("RestorePackagesPath", nugetPackageRoot + Path.DirectorySeparatorChar);

            // allow NuGet to locate packages in existing global packages folder if set
            // else fallback to standard location
            if (Environment.GetEnvironmentVariable("NUGET_PACKAGES") is string nugetPackagesDir)
            {
                if (Directory.Exists(nugetPackagesDir) == false)
                    Directory.CreateDirectory(nugetPackagesDir);

                analyzer.SetGlobalProperty("RestoreAdditionalProjectFallbackFolders", nugetPackagesDir);
            }

            return analyzer;
        }

        [TestMethod]
        public void Can_generate_and_consume_nuget_package()
        {
            // build nuget package from PackProjectLib
            var libAnalyzer = CreateAnalyzer(Path.Combine(@"PackProject", "Lib", "PackProjectLib.csproj"), null);
            var libOptions = new EnvironmentOptions();
            libOptions.DesignTime = false;
            libOptions.TargetsToBuild.Clear();
            libOptions.TargetsToBuild.Add("Restore");
            libOptions.TargetsToBuild.Add("Build");
            libOptions.TargetsToBuild.Add("Pack");
            var libResults = libAnalyzer.Build(libOptions);
            libResults.OverallSuccess.Should().Be(true);

            // build exe which references lib which references nuget package
            var buildAnalyzer = CreateAnalyzer(Path.Combine(@"PackageReferenceProject", "Exe", "PackageReferenceProjectExe.csproj"), Path.GetFullPath(Path.Combine("PackProject", "Lib", "bin", "Debug")));
            var buildOptions = new EnvironmentOptions();
            buildOptions.DesignTime = false;
            buildOptions.TargetsToBuild.Clear();
            buildOptions.TargetsToBuild.Add("Build");
            var buildResults = buildAnalyzer.Build(buildOptions);
            buildResults.OverallSuccess.Should().Be(true);

            // only select supported tfms
            var tfms = new[] { "netcoreapp3.1", "net5.0", "net6.0" };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                tfms = new[] { "net461", "net472", "net48", "netcoreapp3.1", "net5.0", "net6.0" };

            // only select supported rids
            var rids = new[] { "win7-x64", "linux-x64" };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                rids = new[] { "linux-x64" };

            foreach (var tfm in tfms)
            {
                foreach (var rid in rids)
                {
                    TestContext.WriteLine("Publishing with TargetFramework {0} and RuntimeIdentifier {1}.", tfm, rid);

                    // publish exe which references lib which references nuget package
                    var pubAnalyzer = CreateAnalyzer(Path.Combine(@"PackageReferenceProject", "Exe", "PackageReferenceProjectExe.csproj"), Path.GetFullPath(Path.Combine("PackProject", "Lib", "bin", "Debug")));
                    var pubOptions = new EnvironmentOptions();
                    pubOptions.GlobalProperties.Add("TargetFramework", tfm);
                    pubOptions.GlobalProperties.Add("RuntimeIdentifier", rid);
                    pubOptions.DesignTime = false;
                    pubOptions.TargetsToBuild.Clear();
                    pubOptions.TargetsToBuild.Add("Publish");
                    var pubResults = pubAnalyzer.Build(pubOptions);
                    pubResults.OverallSuccess.Should().Be(true);
                }
            }
        }

    }

}