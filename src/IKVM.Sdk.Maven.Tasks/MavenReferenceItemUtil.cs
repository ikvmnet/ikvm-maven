using System;
using System.Collections.Generic;
using System.Linq;

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
            return itemSpec;
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
                map[NormalizeItemSpec(item.ItemSpec)] = new MavenReferenceItem(item);

            // populate the properties of each item
            foreach (var item in map.Values)
            {
                item.ItemSpec = NormalizeItemSpec(item.Item.ItemSpec);
            }

            // return the resulting imported references
            return map.Values.ToArray();
        }

    }

}
