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
            var a = MavenTaskUtil.TryParseArtifact(itemSpec);
            var b = new StringBuilder();
            if (string.IsNullOrWhiteSpace(a.getGroupId()) == false)
                b.Append(a.getGroupId());
            if (string.IsNullOrWhiteSpace(a.getArtifactId()) == false)
                b.Append(':').Append(a.getArtifactId());
            if (string.IsNullOrWhiteSpace(a.getVersion()) == false)
                b.Append(':').Append(a.getVersion());

            return b.ToString();
        }

        /// <summary>
        /// Returns a normalized version of a <see cref="MavenReferenceItem"/> itemspec.
        /// </summary>
        /// <returns></returns>
        public static string GetItemSpec(string groupId, string artifactId, string version)
        {
            var a = MavenTaskUtil.TryCreateArtifact(groupId, artifactId, version);
            var b = new StringBuilder();
            if (string.IsNullOrWhiteSpace(a.getGroupId()) == false)
                b.Append(a.getGroupId());
            if (string.IsNullOrWhiteSpace(a.getArtifactId()) == false)
                b.Append(':').Append(a.getArtifactId());
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
                item.ItemSpec = NormalizeItemSpec(item.Item.ItemSpec);
                item.GroupId = item.Item.GetMetadata(MavenReferenceItemMetadata.GroupId);
                item.ArtifactId = item.Item.GetMetadata(MavenReferenceItemMetadata.ArtifactId);
                item.Version = item.Item.GetMetadata(MavenReferenceItemMetadata.Version);
                item.Dependencies = ResolveDependencies(map, item, item.Item.GetMetadata(MavenReferenceItemMetadata.Dependencies));
                item.Compile = item.Item.GetMetadata(MavenReferenceItemMetadata.Compile)?.Split(MavenReferenceItemMetadata.PropertySeperatorCharArray, StringSplitOptions.RemoveEmptyEntries).ToList();
                item.Sources = item.Item.GetMetadata(MavenReferenceItemMetadata.Sources)?.Split(MavenReferenceItemMetadata.PropertySeperatorCharArray, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            // return the resulting imported references
            return map.Values.ToArray();
        }

        /// <summary>
        /// Attempts to resolve the dependencies given by the dependency string <paramref name="dependencies"/> for
        /// <paramref name="item"/> against <paramref name="map"/>.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="item"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskException"></exception>
        static List<MavenReferenceItem> ResolveDependencies(Dictionary<string, MavenReferenceItem> map, MavenReferenceItem item, string dependencies)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            var l = new List<MavenReferenceItem>(8);
            foreach (var itemSpec in dependencies.Split(MavenReferenceItemMetadata.PropertySeperatorCharArray, StringSplitOptions.RemoveEmptyEntries))
                if (TryResolveDependency(map, itemSpec, out var resolved))
                    l.Add(resolved);
                else
                    throw new MavenTaskMessageException("Error.MavenInvalidReference", item.ItemSpec, itemSpec);

            return l;
        }

        /// <summary>
        /// Attempts to resolve the given <see cref="MavenReferenceItem"/> itemspec against the set of  <see cref="MavenReferenceItem"/> instances
        /// </summary>
        /// <param name="map"></param>
        /// <param name="itemSpec"></param>
        /// <param name="resolved"></param>
        /// <returns></returns>
        static bool TryResolveDependency(Dictionary<string, MavenReferenceItem> map, string itemSpec, out MavenReferenceItem resolved)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));
            if (string.IsNullOrEmpty(itemSpec))
                throw new ArgumentException($"'{nameof(itemSpec)}' cannot be null or empty.", nameof(itemSpec));

            resolved = map.TryGetValue(itemSpec, out var r) ? r : null;
            return resolved != null;
        }

    }

}
