using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Provides common utility methods for working with <see cref="MavenReferenceItem"/> sets.
    /// </summary>
    static class MavenReferenceItemUtil
    {

        /// <summary>
        /// Attempts to import a set of <see cref="MavenReferenceItem"/> instances from the given <see cref="ITaskItem"/> instances.
        /// </summary>
        /// <param name="taskItems"></param>
        /// <returns></returns>
        public static MavenReferenceItem[] Import(IEnumerable<ITaskItem> taskItems)
        {
            if (taskItems is null)
                throw new ArgumentNullException(nameof(taskItems));

            // normalize itemspecs into a dictionary
            var items = new List<MavenReferenceItem>();

            // populate the properties of each item
            foreach (var taskItem in taskItems)
            {
                var item = new MavenReferenceItem(taskItem);
                item.ItemSpec = taskItem.ItemSpec;
                item.GroupId = taskItem.GetMetadata(MavenReferenceItemMetadata.GroupId);
                item.ArtifactId = taskItem.GetMetadata(MavenReferenceItemMetadata.ArtifactId);
                item.Version = taskItem.GetMetadata(MavenReferenceItemMetadata.Version);
                items.Add(item);
            }

            // return the resulting imported references
            return items.ToArray();
        }

    }

}
