using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using IKVM.Maven.Sdk.Tasks.Extensions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKVM.Maven.Sdk.Tasks.Tests.Extensions
{

    /// <summary>
    /// The Maven object model is a Java object graph; these wrappers are what lets the tasks walk it with LINQ. The
    /// set wrapper is covered separately by <see cref="SetWrapperTests"/>.
    /// </summary>
    [TestClass]
    public class JavaCollectionExtensionsTests
    {

        static java.util.ArrayList CreateArrayList(params string[] values)
        {
            var l = new java.util.ArrayList();
            foreach (var v in values)
                l.add(v);

            return l;
        }

        static java.util.LinkedHashSet CreateSet(params string[] values)
        {
            var s = new java.util.LinkedHashSet();
            foreach (var v in values)
                s.add(v);

            return s;
        }

        static java.util.LinkedHashMap CreateMap(params string[] values)
        {
            var m = new java.util.LinkedHashMap();
            for (int i = 0; i < values.Length; i += 2)
                m.put(values[i], values[i + 1]);

            return m;
        }

        #region Iterator

        [TestMethod]
        public void RemainingToList_should_drain_the_iterator()
        {
            var i = CreateArrayList("a", "b", "c").iterator();
            i.RemainingToList<string>().Should().Equal("a", "b", "c");
            i.hasNext().Should().BeFalse();
        }

        [TestMethod]
        public void RemainingToList_should_only_return_what_is_left()
        {
            var i = CreateArrayList("a", "b", "c").iterator();
            i.next();
            i.RemainingToList<string>().Should().Equal("b", "c");
        }

        [TestMethod]
        public void RemainingToEnumerable_should_drain_the_iterator()
        {
            CreateArrayList("a", "b", "c").iterator().RemainingToEnumerable<string>().Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void RemainingToList_should_return_empty_for_exhausted_iterator()
        {
            CreateArrayList().iterator().RemainingToList<string>().Should().BeEmpty();
        }

        [TestMethod]
        public void AsEnumerator_should_wrap_the_iterator()
        {
            var e = CreateArrayList("a", "b").iterator().AsEnumerator<string>();
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be("a");
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be("b");
            e.MoveNext().Should().BeFalse();
        }

        [TestMethod]
        public void AsEnumerator_should_throw_before_first_move_next()
        {
            var e = CreateArrayList("a").iterator().AsEnumerator<string>();
            var f = () => e.Current;
            f.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void AsEnumerator_should_not_support_reset()
        {
            var e = CreateArrayList("a").iterator().AsEnumerator<string>();
            var f = () => e.Reset();
            f.Should().Throw<NotSupportedException>();
        }

        #endregion

        #region Iterable

        [TestMethod]
        public void AsEnumerable_should_enumerate_a_list()
        {
            CreateArrayList("a", "b", "c").AsEnumerable<string>().Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void AsEnumerable_should_enumerate_a_set()
        {
            CreateSet("a", "b").AsEnumerable<string>().Should().BeEquivalentTo(new[] { "a", "b" });
        }

        [TestMethod]
        public void AsEnumerable_should_enumerate_more_than_once()
        {
            var e = CreateArrayList("a", "b").AsEnumerable<string>();
            e.Should().Equal("a", "b");
            e.Should().Equal("a", "b");
        }

        #endregion

        #region List

        [TestMethod]
        public void AsList_should_expose_the_list_contents()
        {
            var l = CreateArrayList("a", "b", "c").AsList<string>();
            l.Count.Should().Be(3);
            l[0].Should().Be("a");
            l[2].Should().Be("c");
            l.IndexOf("b").Should().Be(1);
            l.Contains("b").Should().BeTrue();
            l.Contains("z").Should().BeFalse();
            l.Should().Equal("a", "b", "c");
        }

        [TestMethod]
        public void AsList_should_write_through_to_the_java_list()
        {
            var j = CreateArrayList("a", "b");
            var l = j.AsList<string>();

            l.Add("c");
            j.size().Should().Be(3);
            j.get(2).Should().Be("c");

            l.Insert(0, "z");
            j.get(0).Should().Be("z");

            l[0] = "y";
            j.get(0).Should().Be("y");

            l.RemoveAt(0);
            j.get(0).Should().Be("a");

            l.Remove("a").Should().BeTrue();
            j.contains("a").Should().BeFalse();

            l.Clear();
            j.isEmpty().Should().BeTrue();
        }

        [TestMethod]
        public void AsList_should_copy_to_an_array()
        {
            var a = new string[4];
            CreateArrayList("a", "b", "c").AsList<string>().CopyTo(a, 1);
            a.Should().Equal(new string[] { null, "a", "b", "c" });
        }

        #endregion

        #region Collection

        [TestMethod]
        public void AsCollection_should_expose_the_collection_contents()
        {
            var q = new java.util.ArrayDeque();
            q.add("a");
            q.add("b");

            var c = q.AsCollection<string>();
            c.Count.Should().Be(2);
            c.Contains("a").Should().BeTrue();
            c.Should().Equal("a", "b");

            c.Add("c");
            q.size().Should().Be(3);

            c.Remove("a").Should().BeTrue();
            q.contains("a").Should().BeFalse();

            c.Clear();
            q.isEmpty().Should().BeTrue();
        }

        #endregion

        #region Map

        [TestMethod]
        public void AsDictionary_should_expose_the_map_contents()
        {
            var d = CreateMap("a", "1", "b", "2").AsDictionary<string, string>();
            d.Count.Should().Be(2);
            d["a"].Should().Be("1");
            d.Keys.Should().BeEquivalentTo(new[] { "a", "b" });
            d.Values.Should().BeEquivalentTo(new[] { "1", "2" });
            d.ContainsKey("a").Should().BeTrue();
            d.ContainsKey("z").Should().BeFalse();
            d.Should().Equal(new Dictionary<string, string>() { ["a"] = "1", ["b"] = "2" });
        }

        [TestMethod]
        public void AsDictionary_should_support_try_get_value()
        {
            var d = CreateMap("a", "1").AsDictionary<string, string>();

            d.TryGetValue("a", out var found).Should().BeTrue();
            found.Should().Be("1");

            d.TryGetValue("z", out var missing).Should().BeFalse();
            missing.Should().BeNull();
        }

        [TestMethod]
        public void AsDictionary_should_write_through_to_the_java_map()
        {
            var m = CreateMap("a", "1");
            var d = m.AsDictionary<string, string>();

            d.Add("b", "2");
            m.get("b").Should().Be("2");

            d["a"] = "9";
            m.get("a").Should().Be("9");

            d.Remove("a").Should().BeTrue();
            d.Remove("a").Should().BeFalse();
            m.containsKey("a").Should().BeFalse();

            d.Clear();
            m.isEmpty().Should().BeTrue();
        }

        [TestMethod]
        public void AsDictionary_should_support_contains_of_key_value_pair()
        {
            var d = CreateMap("a", "1").AsDictionary<string, string>();
            d.Contains(new KeyValuePair<string, string>("a", "1")).Should().BeTrue();
            d.Contains(new KeyValuePair<string, string>("a", "2")).Should().BeFalse();
            d.Contains(new KeyValuePair<string, string>("z", "1")).Should().BeFalse();
        }

        [TestMethod]
        public void AsDictionary_should_copy_to_an_array()
        {
            var a = new KeyValuePair<string, string>[2];
            CreateMap("a", "1", "b", "2").AsDictionary<string, string>().CopyTo(a, 0);
            a.Select(i => i.Key).Should().BeEquivalentTo(new[] { "a", "b" });
        }

        #endregion

    }

}
