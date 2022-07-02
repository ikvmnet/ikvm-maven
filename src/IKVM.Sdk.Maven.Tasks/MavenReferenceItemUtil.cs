using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;

namespace IKVM.Sdk.Maven.Tasks
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

        /// <summary>
        /// Attempts to import a set of <see cref="MavenReferenceItem"/> instances from the given <see cref="ITaskItem"/> instances.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static MavenReferenceItem[] Import(IEnumerable<ITaskItem> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            // normalize itemspecs into a dictionary
            var map = new Dictionary<string, MavenReferenceItem>();
            foreach (var item in items)
                map[item.ItemSpec] = new MavenReferenceItem(item);

            // populate the properties of each item
            foreach (var item in map.Values)
            {
                item.ItemSpec = item.Item.ItemSpec;
                item.GroupId = item.Item.GetMetadata(MavenReferenceItemMetadata.GroupId);
                item.ArtifactId = item.Item.GetMetadata(MavenReferenceItemMetadata.ArtifactId);
                item.Classifier = item.Item.GetMetadata(MavenReferenceItemMetadata.Classifier);
                item.Version = item.Item.GetMetadata(MavenReferenceItemMetadata.Version);
                item.Optional = string.Equals(item.Item.GetMetadata(MavenReferenceItemMetadata.Optional), "true", StringComparison.OrdinalIgnoreCase);
                item.Scope = item.Item.GetMetadata(MavenReferenceItemMetadata.Scope);
                item.Debug = string.Equals(item.Item.GetMetadata(MavenReferenceItemMetadata.Debug), "true", StringComparison.OrdinalIgnoreCase);
                item.AssemblyName = item.Item.GetMetadata(MavenReferenceItemMetadata.AssemblyName);
                item.AssemblyVersion = item.Item.GetMetadata(MavenReferenceItemMetadata.AssemblyVersion);
            }

            // return the resulting imported references
            return map.Values.ToArray();
        }

    }

}
