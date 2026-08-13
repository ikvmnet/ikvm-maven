using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    [TestClass]
    public class NuGetApiTests
    {

        readonly string binPath = Path.GetDirectoryName(typeof(NuGetApiTests).Assembly.Location);

        string AssetsFilePath => Path.Combine(binPath, "Test.project.assets.json");

        [TestMethod]
        public void LoadLockFile_should_throw_on_null_path()
        {
            var f = () => new NuGetApi().LoadLockFile(null, NuGet.Common.NullLogger.Instance);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void LoadLockFile_should_throw_on_missing_file()
        {
            var f = () => new NuGetApi().LoadLockFile(Path.Combine(binPath, "DoesNotExist.assets.json"), NuGet.Common.NullLogger.Instance);
            f.Should().Throw<FileNotFoundException>();
        }

        [TestMethod]
        public void LoadLockFile_should_load_assets_file()
        {
            var lockFile = new NuGetApi().LoadLockFile(AssetsFilePath, NuGet.Common.NullLogger.Instance);
            lockFile.Should().NotBeNull();
            lockFile.Libraries.Should().ContainSingle().Which.Name.Should().Be("HelloTest");
        }

        [TestMethod]
        [DataRow(null, "net8.0")]
        [DataRow("", "net8.0")]
        public void GetProjectObjectModelFiles_should_throw_on_missing_assets_file_path(string assetsFilePath, string tfm)
        {
            var f = () => new NuGetApi().GetProjectObjectModelFiles(assetsFilePath, tfm, null, new NullNuGetLogger());
            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void GetProjectObjectModelFiles_should_throw_on_missing_target_framework(string tfm)
        {
            var f = () => new NuGetApi().GetProjectObjectModelFiles(AssetsFilePath, tfm, null, new NullNuGetLogger());
            f.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void GetProjectObjectModelFiles_should_throw_on_null_logger()
        {
            var f = () => new NuGetApi().GetProjectObjectModelFiles(AssetsFilePath, "net8.0", null, null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("net481")]
        [DataRow("net6.0")]
        [DataRow("net8.0")]
        [DataRow("net8.0-windows")]
        public void GetProjectObjectModelFiles_should_locate_pom_on_disk(string tfm)
        {
            var l = new NuGetApi().GetProjectObjectModelFiles(AssetsFilePath, tfm, null, new NullNuGetLogger());
            l.Should().ContainSingle();
            File.Exists(l[0]).Should().BeTrue();
        }

        /// <summary>
        /// Only libraries of type 'package' carry POM files; a project reference contributes its own items instead.
        /// </summary>
        [TestMethod]
        public void GetProjectObjectModelFiles_should_ignore_non_package_libraries()
        {
            var api = new NuGetApi();
            var lockFile = api.LoadLockFile(AssetsFilePath, NuGet.Common.NullLogger.Instance);

            foreach (var library in lockFile.Targets.SelectMany(i => i.Libraries))
                library.Type = "project";

            api.GetProjectObjectModelFiles(lockFile, "net8.0", null).Should().BeEmpty();
        }

        [TestMethod]
        public void GetProjectObjectModelFiles_should_throw_on_null_lock_file()
        {
            var f = () => new NuGetApi().GetProjectObjectModelFiles(null, "net8.0", null).ToList();
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetProjectObjectModelFiles_should_throw_on_null_target_framework()
        {
            var api = new NuGetApi();
            var lockFile = api.LoadLockFile(AssetsFilePath, NuGet.Common.NullLogger.Instance);
            var f = () => api.GetProjectObjectModelFiles(lockFile, null, null).ToList();
            f.Should().Throw<ArgumentNullException>();
        }

        /// <summary>
        /// A <see cref="INuGetLogger"/> which discards everything.
        /// </summary>
        class NullNuGetLogger : INuGetLogger
        {

            public void LogDebug(string data) { }

            public void LogVerbose(string data) { }

            public void LogInformation(string data) { }

            public void LogMinimal(string data) { }

            public void LogWarning(string data) { }

            public void LogError(string data) { }

        }

    }

}
