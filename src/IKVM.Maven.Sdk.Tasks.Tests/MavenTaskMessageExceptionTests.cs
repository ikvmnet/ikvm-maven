using System;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Resources;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests
{

    /// <summary>
    /// The tasks raise diagnostics by resource name, and the name is only resolved at the moment the diagnostic is
    /// raised. A renamed or removed resource therefore turns a useful error message into a crash on the error path,
    /// which is exactly where it is hardest to notice.
    /// </summary>
    [TestClass]
    public class MavenTaskMessageExceptionTests
    {

        [TestMethod]
        [DataRow("Error.MavenInvalidCoordinates", "MAVEN0001")]
        [DataRow("Error.MavenInvalidGroupId", "MAVEN0002")]
        [DataRow("Error.MavenInvalidArtifactId", "MAVEN0003")]
        [DataRow("Error.MavenInvalidVersion", "MAVEN0004")]
        [DataRow("Error.MavenMissingGroupId", "MAVEN0005")]
        [DataRow("Error.MavenMissingArtifactId", "MAVEN0006")]
        [DataRow("Error.MavenMissingVersion", "MAVEN0007")]
        [DataRow("Error.MavenMissingScope", "MAVEN0008")]
        [DataRow("Error.MavenInvalidScope", "MAVEN0009")]
        [DataRow("Error.MavenTransferCorrupted", "MAVEN0010")]
        [DataRow("Error.MavenTransferFailed", "MAVEN0011")]
        [DataRow("Warning.MavenIgnoreCyclicReference", "MAVEN0012")]
        public void Should_resolve_diagnostic_resource_to_its_code(string resourceName, string code)
        {
            SR.ResourceManager.GetString(resourceName).Should().StartWith(code + ":");
        }

        [TestMethod]
        public void Should_format_the_message_from_the_resource()
        {
            var e = new MavenTaskMessageException("Error.MavenInvalidScope", "ikvm.test:foo:1.0", "bogus");
            e.MessageResourceName.Should().Be("Error.MavenInvalidScope");
            e.MessageArgs.Should().Equal("ikvm.test:foo:1.0", "bogus");
            e.Message.Should().Be("MAVEN0009: Invalid Maven scope 'bogus' on 'ikvm.test:foo:1.0'.");
        }

        [TestMethod]
        public void Should_carry_an_inner_exception()
        {
            var inner = new InvalidOperationException("inner");
            var e = new MavenTaskMessageException(inner, "Error.MavenMissingVersion");
            e.InnerException.Should().BeSameAs(inner);
            e.MessageResourceName.Should().Be("Error.MavenMissingVersion");
            e.Message.Should().StartWith("MAVEN0007:");
        }

        [TestMethod]
        public void Should_throw_on_null_resource_name()
        {
            var f = () => new MavenTaskMessageException(null);
            f.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Should_be_a_maven_task_exception()
        {
            new MavenTaskMessageException("Error.MavenMissingVersion").Should().BeAssignableTo<MavenTaskException>();
        }

    }

}
