using System.Text.Json;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    [TestClass]
    public class PreserveReferenceResolverTests
    {

        [TestMethod]
        public void Should_assign_a_new_id_to_each_new_object()
        {
            var r = new PreserveReferenceResolver();
            r.GetReference(new object(), out var exists1).Should().Be("1");
            exists1.Should().BeFalse();
            r.GetReference(new object(), out var exists2).Should().Be("2");
            exists2.Should().BeFalse();
        }

        [TestMethod]
        public void Should_return_the_same_id_for_the_same_object()
        {
            var r = new PreserveReferenceResolver();
            var o = new object();
            r.GetReference(o, out var exists1).Should().Be("1");
            exists1.Should().BeFalse();
            r.GetReference(o, out var exists2).Should().Be("1");
            exists2.Should().BeTrue();
        }

        [TestMethod]
        public void Should_resolve_an_added_reference()
        {
            var r = new PreserveReferenceResolver();
            var o = new object();
            r.AddReference("1", o);
            r.ResolveReference("1").Should().BeSameAs(o);
        }

        [TestMethod]
        public void Should_throw_on_duplicate_reference()
        {
            var r = new PreserveReferenceResolver();
            r.AddReference("1", new object());
            var f = () => r.AddReference("1", new object());
            f.Should().Throw<JsonException>();
        }

        [TestMethod]
        public void Should_throw_on_missing_reference()
        {
            var r = new PreserveReferenceResolver();
            var f = () => r.ResolveReference("1");
            f.Should().Throw<JsonException>();
        }

        [TestMethod]
        public void Handler_should_return_a_stable_resolver()
        {
            var h = new PreserveReferenceHandler();
            h.CreateResolver().Should().BeSameAs(h.CreateResolver());
        }

    }

}
