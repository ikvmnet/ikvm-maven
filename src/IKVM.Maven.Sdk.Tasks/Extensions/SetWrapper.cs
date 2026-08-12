using System;
using System.Collections.Generic;
using System.Linq;

using java.util;

namespace IKVM.Maven.Sdk.Tasks.Extensions
{

    class SetWrapper<T> : CollectionWrapper<T>, ISet<T>
    {

        readonly Set set;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="set"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SetWrapper(Set set) :
            base(set)
        {
            this.set = set ?? throw new ArgumentNullException(nameof(set));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                set.remove(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            // T[] is not covariant with object[] for an unconstrained T, so the array has to be built as object[]
            // explicitly or it ends up boxed as the single element of the argument list
            set.retainAll(Arrays.asList(other.Cast<object>().ToArray()));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.Add(T item)
        {
            return set.add(item);
        }

    }

}