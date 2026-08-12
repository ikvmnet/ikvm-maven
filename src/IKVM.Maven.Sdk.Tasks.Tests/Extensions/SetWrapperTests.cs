using System.Linq;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests.Extensions
{

    /// <summary>
    /// Covers the <see cref="System.Collections.Generic.ISet{T}"/> view over a Java set returned by
    /// <see cref="SetExtensions.AsSet{T}"/>.
    /// </summary>
    [TestClass]
    public class SetWrapperTests
    {

        static java.util.LinkedHashSet CreateSet(params string[] values)
        {
            var s = new java.util.LinkedHashSet();
            foreach (var v in values)
                s.add(v);

            return s;
        }

        [TestMethod]
        public void Should_expose_the_set_contents()
        {
            var s = CreateSet("a", "b").AsSet<string>();
            s.Count.Should().Be(2);
            s.Contains("a").Should().BeTrue();
            s.Contains("z").Should().BeFalse();
            s.Should().Equal("a", "b");
        }

        [TestMethod]
        public void Should_report_whether_add_changed_the_set()
        {
            var s = CreateSet("a").AsSet<string>();
            s.Add("a").Should().BeFalse();
            s.Add("b").Should().BeTrue();
            s.Count.Should().Be(2);
        }

        [TestMethod]
        public void ExceptWith_should_remove_the_given_items()
        {
            var j = CreateSet("a", "b", "c");
            j.AsSet<string>().ExceptWith(new[] { "a", "c" });
            j.AsSet<string>().Should().Equal("b");
        }

        [TestMethod]
        public void IntersectWith_should_keep_the_items_present_in_both()
        {
            var j = CreateSet("a", "b", "c");
            j.AsSet<string>().IntersectWith(new[] { "a", "c", "z" });
            j.AsSet<string>().Should().Equal("a", "c");
        }

        [TestMethod]
        public void IntersectWith_should_keep_everything_when_the_set_is_a_subset()
        {
            var j = CreateSet("a", "b");
            j.AsSet<string>().IntersectWith(new[] { "a", "b", "c" });
            j.AsSet<string>().Should().Equal("a", "b");
        }

        [TestMethod]
        public void IntersectWith_should_empty_the_set_when_nothing_is_shared()
        {
            var j = CreateSet("a", "b");
            j.AsSet<string>().IntersectWith(new[] { "y", "z" });
            j.AsSet<string>().Should().BeEmpty();
        }

        [TestMethod]
        public void Should_wrap_a_tree_set()
        {
            var t = new java.util.TreeSet();
            t.add("c");
            t.add("a");
            t.add("b");

            t.AsSet<string>().Should().Equal("a", "b", "c");
        }

    }

}
