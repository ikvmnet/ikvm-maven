using System.Collections.Generic;

using java.lang;

namespace IKVM.Maven.Sdk.Tasks.Extensions
{

    /// <summary>
    /// Provides extension methods for working against Java <see cref="Iterable"/> instances.
    /// </summary>
    public static class IterableExtensions
    {

        /// <summary>
        /// Returns the appropriate wrapper type for the given <see cref="Iterable"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iterable"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this Iterable iterable)
        {
            var e = iterable.iterator().AsEnumerator<T>();
            while (e.MoveNext())
                yield return e.Current;
        }

    }

}
