using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using org.eclipse.aether.artifact;
using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks.Aether
{

    class DependencyEqualityComparer : IEqualityComparer<Dependency>
    {

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static DependencyEqualityComparer Default { get; } = new DependencyEqualityComparer();

        /// <summary>
        /// Checks that the two <see cref="Dependency"/> objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Equals(Dependency a, Dependency b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            // check that artifacts match
            if (Equals(a.getArtifact(), b.getArtifact()) == false)
                return false;

            // check that optional value matches
            if (a.getOptional()?.booleanValue() != b.getOptional()?.booleanValue())
                return false;

            // check that scope matches
            if (a.getScope() != b.getScope())
                return false;

            // check that exclusions are equal
            var ae = ((IEnumerable)a.getExclusions()).Cast<Exclusion>().ToArray();
            var be = ((IEnumerable)b.getExclusions()).Cast<Exclusion>().ToArray();
            if (Equals(ae, be) == false)
                return false;

            return true;
        }

        /// <summary>
        /// Checks that the two <see cref="Dependency"/> objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool Equals(Artifact a, Artifact b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            if (a.getGroupId() != b.getGroupId() ||
                a.getArtifactId() != b.getArtifactId() ||
                a.getClassifier() != b.getClassifier() ||
                a.getExtension() != b.getExtension() ||
                a.getVersion() != b.getVersion())
                return false;

            if (a.getFile()?.toString() != b.getFile()?.toString())
                return false;

            return true;
        }

        /// <summary>
        /// Checks that the two <see cref="Dependency"/> objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool Equals(IList<Exclusion> a, IList<Exclusion> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            if (a.Count != b.Count)
                return false;

            foreach (var i in a)
                if (b.Any(j => Equals(i, j)) == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Checks that the two <see cref="Dependency"/> objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool Equals(Exclusion a, Exclusion b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (b == null)
                return false;

            if (a.getGroupId() != b.getGroupId() ||
                a.getArtifactId() != b.getArtifactId() ||
                a.getClassifier() != b.getClassifier() ||
                a.getExtension() != b.getExtension())
                return false;

            return true;
        }

        public int GetHashCode(Dependency obj)
        {
            throw new NotImplementedException();
        }

    }

}
