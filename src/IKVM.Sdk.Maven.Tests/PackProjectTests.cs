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

        [TestMethod]
        public void Can_pack_test_project()
        {
            var properties = File.ReadAllLines("IKVM.Sdk.Maven.Tests.properties").Select(i => i.Split('=', 2)).ToDictionary(i => i[0], i => i[1]);

            var nugetPackageRoot = Path.Combine(Path.GetTempPath(), "IKVM.Sdk.Maven.Tests_PackProject", "nuget", "packages");
            if (Directory.Exists(nugetPackageRoot))
                Directory.Delete(nugetPackageRoot, true);
            Directory.CreateDirectory(nugetPackageRoot);

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(Path.Combine(@"PackProject", "Lib", "PackProjectLib.csproj"));
            analyzer.SetGlobalProperty("PackageVersion", properties["PackageVersion"]);
            analyzer.SetGlobalProperty("RestoreSources", string.Join("%3B", "https://api.nuget.org/v3/index.json", Path.GetFullPath(@"nuget")));
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

            var options = new EnvironmentOptions();
            options.DesignTime = false;
            options.TargetsToBuild.Clear();
            options.TargetsToBuild.Add("Restore");
            options.TargetsToBuild.Add("Pack");
            var results = analyzer.Build(options);
            results.OverallSuccess.Should().Be(true);
        }

    }

}