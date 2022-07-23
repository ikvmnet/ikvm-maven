using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Provides common utility methods for working with <see cref="MavenReferenceItem"/> sets.
    /// </summary>
    static class MavenReferenceItemUtil
    {

        /// <summary>
        /// Returns a normalized version of a <see cref="MavenReferenceItem"/> itemspec.
        /// </summary>
        /// <param name="itemSpec"></param>
        /// <returns></returns>
        public static string NormalizeItemSpec(string itemSpec)
        {
            if (string.IsNullOrWhiteSpace(itemSpec))
                throw new ArgumentException($"'{nameof(itemSpec)}' cannot be null or whitespace.", nameof(itemSpec));

            var a = MavenTaskUtil.TryParseArtifact(itemSpec);
            if (a == null)
                return itemSpec;

            var b = new StringBuilder();
            if (string.IsNullOrWhiteSpace(a.getGroupId()) == false)
                b.Append(a.getGroupId());
            if (string.IsNullOrWhiteSpace(a.getArtifactId()) == false)
                b.Append(':').Append(a.getArtifactId());
            if (string.IsNullOrWhiteSpace(a.getClassifier()) == false)
                b.Append(':').Append(a.getClassifier());
            if (string.IsNullOrWhiteSpace(a.getVersion()) == false)
                b.Append(':').Append(a.getVersion());

            return b.ToString();
        }

    }

}
