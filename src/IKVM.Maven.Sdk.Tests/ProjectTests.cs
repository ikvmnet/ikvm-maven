using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
                eventSource.AnyEventRaised += (sender, evt) => context.WriteLine($"{evt.Message}");
            }

        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Can_build_test_project()
        {
            var properties = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), "IKVM.Maven.Sdk.Tests.properties")).Select(i => i.Split('=', 2)).ToDictionary(i => i[0], i => i[1]);

            var nugetPackageRoot = Path.Combine(Path.GetTempPath(), "IKVM.Maven.Sdk.Tests_Project", "nuget", "packages");
            if (Directory.Exists(nugetPackageRoot))
                Directory.Delete(nugetPackageRoot, true);
            Directory.CreateDirectory(nugetPackageRoot);

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), @"Project", "Exe", "ProjectExe.csproj"));
            analyzer.SetGlobalProperty("PackageVersion", properties["PackageVersion"]);
            analyzer.SetGlobalProperty("RestoreSources", string.Join("%3B", "https://api.nuget.org/v3/index.json", Path.Combine(Path.GetDirectoryName(typeof(PackProjectTests).Assembly.Location), "nuget")));
            analyzer.SetGlobalProperty("RestorePackagesPath", nugetPackageRoot + Path.DirectorySeparatorChar);

            // allow NuGet to locate packages in existing global packages folder if set
            // else fallback to standard location
            if (Environment.GetEnvironmentVariable("NUGET_PACKAGES") is string nugetPackagesDir)
            {
                if (Directory.Exists(nugetPackagesDir) == false)
                    Directory.CreateDirectory(nugetPackagesDir);
                analyzer.SetGlobalProperty("RestoreAdditionalProjectFallbackFolders", nugetPackagesDir);
            }

            analyzer.AddBuildLogger(new TargetLogger(TestContext));

            {
                var options = new EnvironmentOptions();
                options.DesignTime = false;
                options.TargetsToBuild.Clear();
                options.TargetsToBuild.Add("Clean");
                options.TargetsToBuild.Add("Restore");
                var results = analyzer.Build(options);
                results.OverallSuccess.Should().Be(true);
            }

            foreach (var tfmrid in new[] {
                "net461/win7-x64",
                "net472/win7-x64",
                "net48/win7-x64",
                "netcoreapp3.1/win7-x64",
                "net5.0/win7-x64",
                "net6.0/win7-x64",
                "netcoreapp3.1/linux-x64",
                "net5.0/linux-x64",
                "net6.0/linux-x64" })
            {
                var _ = tfmrid.Split('/');
                var tfm = _[0];
                var rid = _[1];

                var options = new EnvironmentOptions();
                options.DesignTime = false;
                options.Restore = false;
                options.GlobalProperties.Add("TargetFramework", tfm);
                options.GlobalProperties.Add("RuntimeIdentifier", rid);
                options.TargetsToBuild.Clear();
                options.TargetsToBuild.Add("Build");
                options.TargetsToBuild.Add("Publish");
                var results = analyzer.Build(options);
                results.OverallSuccess.Should().Be(true);
            }
        }

    }

}