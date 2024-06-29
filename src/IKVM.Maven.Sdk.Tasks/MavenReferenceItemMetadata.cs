using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Framework;

namespace IKVM.Maven.Sdk.Tasks
{

    static class MavenReferenceItemMetadata
    {

        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string GroupId = "GroupId";
        public static readonly string ArtifactId = "ArtifactId";
        public static readonly string Classifier = "Classifier";
        public static readonly string Version = "Version";
        public static readonly string Dependencies = "Dependencies";
        public static readonly string Scope = "Scope";
        public static readonly string Optional = "Optional";
        public static readonly string Exclusions = "Exclusions";
        public static readonly string Debug = "Debug";
        public static readonly string ReferenceSource = "ReferenceSource";

        /// <summary>
        /// Writes the metadata to the item.
        /// </summary>
        public static void Save(MavenReferenceItem item, ITaskItem task)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            task.ItemSpec = item.ItemSpec;
            task.SetMetadata(MavenReferenceItemMetadata.GroupId, item.GroupId);
            task.SetMetadata(MavenReferenceItemMetadata.ArtifactId, item.ArtifactId);
            task.SetMetadata(MavenReferenceItemMetadata.Classifier, item.Classifier);
            task.SetMetadata(MavenReferenceItemMetadata.Version, item.Version);
            task.SetMetadata(MavenReferenceItemMetadata.Optional, item.Optional ? "true" : "false");
            task.SetMetadata(MavenReferenceItemMetadata.Scope, item.Scope);
            task.SetMetadata(MavenReferenceItemMetadata.Exclusions, item.Exclusions != null ? string.Join(";", item.Exclusions.Select(i => i.ToString())) : null);
            task.SetMetadata(MavenReferenceItemMetadata.ReferenceSource, item.ReferenceSource);
        }

        /// <summary>
        /// Attempts to import a set of <see cref="MavenReferenceItem"/> instances from the given <see cref="ITaskItem"/> instances.
        /// </summary>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static MavenReferenceItem[] Import(IEnumerable<ITaskItem> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            var list = new List<MavenReferenceItem>();

            // populate the properties of each item
            foreach (var task in tasks)
            {
                var item = new MavenReferenceItem();
                item.ItemSpec = task.ItemSpec;
                item.GroupId = task.GetMetadata(MavenReferenceItemMetadata.GroupId);
                item.ArtifactId = task.GetMetadata(MavenReferenceItemMetadata.ArtifactId);
                item.Classifier = task.GetMetadata(MavenReferenceItemMetadata.Classifier);
                item.Version = task.GetMetadata(MavenReferenceItemMetadata.Version);
                item.Optional = string.Equals(task.GetMetadata(MavenReferenceItemMetadata.Optional), "true", StringComparison.OrdinalIgnoreCase);
                item.Scope = task.GetMetadata(MavenReferenceItemMetadata.Scope);
                item.Exclusions = task.GetMetadata(MavenReferenceItemMetadata.Exclusions)?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(i => ParseExclusion(i)).Where(i => i != null).ToArray() ?? Array.Empty<MavenReferenceItemExclusion>();
                item.ReferenceSource = task.GetMetadata(MavenReferenceItemMetadata.ReferenceSource);
                list.Add(item);
            }

            // return the resulting imported references
            return list.ToArray();
        }

        /// <summary>
        /// Parses a compressed exclusion string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="MavenTaskMessageException"></exception>
        static MavenReferenceItemExclusion ParseExclusion(string value)
        {
            var a = value.Split(':');
            if (a.Length is 2 or 3 or 4)
            {
                var groupId = a[0];
                var artifactId = a[1];
                var classifier = a.Length >= 3 ? a[2] : null;
                var extension = a.Length >= 4 ? a[3] : null;
                return new MavenReferenceItemExclusion(groupId, artifactId, classifier, extension);
            }

            return null;
        }
    }

}
