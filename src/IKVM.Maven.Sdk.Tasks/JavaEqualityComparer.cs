using System.Collections.Generic;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Defines equality by using <see cref="java.lang.Object"/> methods.
    /// </summary>
    class JavaEqualityComparer : IEqualityComparer<java.lang.Object>
    {

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static JavaEqualityComparer Default { get; } = new JavaEqualityComparer();

        public bool Equals(java.lang.Object x, java.lang.Object y)
        {
            if (x == y)
                return true;
            if (y == null)
                return false;
            if (x.equals(y) == false)
                return false;

            return true;
        }

        public int GetHashCode(java.lang.Object obj)
        {
            return obj.hashCode();
        }

    }

}
