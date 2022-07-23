using System;
using System.Collections.Generic;

using Microsoft.Build.Framework;

namespace IKVM.Maven.Sdk.Tasks
{

    static class MavenRepositoryItemMetadata
    {

        public static readonly string Url = "Url";

        /// <summary>
        /// Attempts to import a set of <see cref="MavenRepositoryItem"/> instances from the given <see cref="ITaskItem"/> instances.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static MavenRepositoryItem[] Load(IEnumerable<ITaskItem> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            var list = new List<MavenRepositoryItem>();

            // populate the properties of each item
            foreach (var task in tasks)
            {
                var item = new MavenRepositoryItem();
                item.Id = task.ItemSpec;
                item.Url = task.GetMetadata(Url);
                list.Add(item);
            }

            // return the resulting imported references
            return list.ToArray();
        }

    }

}
