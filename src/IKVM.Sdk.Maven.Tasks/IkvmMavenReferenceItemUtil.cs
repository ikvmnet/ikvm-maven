using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;

namespace IKVM.Sdk.Maven.Tasks
{
    internal class IkvmMavenReferenceItemUtil
    {
        public static MavenReferenceItem[] Import(IEnumerable<ITaskItem> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            var list = new List<MavenReferenceItem>();
            foreach (var item in items)
                list.Add(new MavenReferenceItem(item));

            foreach (var item in list)
            {
                item.ArtifactId = item.Item.ItemSpec;
                item.GroupId = item.Item.GetMetadata(IkvmMavenReferenceItemMetadata.GroupId);
                item.Version = item.Item.GetMetadata(IkvmMavenReferenceItemMetadata.Version);
                item.Classifier = item.Item.GetMetadata(IkvmMavenReferenceItemMetadata.Classifier);
                var extension = item.Item.GetMetadata(IkvmMavenReferenceItemMetadata.Extension);
                item.Extension = string.IsNullOrWhiteSpace(extension) ? IkvmMavenReferenceItemMetadata.DefaultExtension : extension;
            }

            return list.ToArray();
        }
    }
}
