using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.sun.tools.javac.util;

using IKVM.Maven.Sdk.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.eclipse.aether.util.artifact;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/> passed in, assigns default metadata if required.
    /// </summary>
    public class MavenReferenceItemPrepare : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemPrepare() :
            base(SR.ResourceManager, "MAVEN:")
        {

        }

        /// <summary>
        /// Set of MavenReferenceItem
        /// </summary>
        [Required]
        [Output]
        public ITaskItem[] Items { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                var items = MavenReferenceItemMetadata.Import(Items);

                // assign other metadata
                foreach (var item in items)
                    AssignMetadata(item);

                Items = items.Select(ToTaskItem).ToArray();
                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        /// <summary>
        /// Persists the item to a task item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ITaskItem ToTaskItem(MavenReferenceItem item)
        {
            var task = new TaskItem();
            MavenReferenceItemMetadata.Save(item, task);
            return task;
        }

        /// <summary>
        /// Assigns the metadata to the item.
        /// </summary>
        /// <param name="item"></param>
        void AssignMetadata(MavenReferenceItem item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemSpec) == false)
            {
                var a = item.ItemSpec.Split(':');
                if (a.Length is 2 or 3)
                {
                    // itemspec may set various properties
                    var groupId = a[0];
                    var artifactId = a[1];
                    var version = a.Length >= 3 ? a[2] : null;

                    // if the itemspec is parsable as coordinates, we should attempt to apply or validate metadata
                    if (string.IsNullOrWhiteSpace(item.GroupId))
                        item.GroupId = groupId;
                    else if (item.GroupId != groupId)
                        throw new MavenTaskMessageException("Error.MavenInvalidGroupId", item.ItemSpec);

                    if (string.IsNullOrWhiteSpace(item.ArtifactId))
                        item.ArtifactId = artifactId;
                    else if (item.ArtifactId != artifactId)
                        throw new MavenTaskMessageException("Error.MavenInvalidArtifactId", item.ItemSpec);

                    if (string.IsNullOrWhiteSpace(item.Version))
                        item.Version = version;
                    else if (version != null && item.Version != version)
                        throw new MavenTaskMessageException("Error.MavenInvalidVersion", item.ItemSpec);
                }
            }

            if (string.IsNullOrWhiteSpace(item.GroupId))
                throw new MavenTaskMessageException("Error.MavenMissingGroupId", item.ItemSpec);

            if (string.IsNullOrWhiteSpace(item.ArtifactId))
                throw new MavenTaskMessageException("Error.MavenMissingArtifactId", item.ItemSpec);

            if (string.IsNullOrWhiteSpace(item.Version))
                throw new MavenTaskMessageException("Error.MavenMissingVersion", item.ItemSpec);

            if (string.IsNullOrWhiteSpace(item.Scope))
                item.Scope = JavaScopes.COMPILE;

            if (IsValidScope(item.Scope) == false)
                throw new MavenTaskMessageException("Error.MavenInvalidScope", item.ItemSpec, item.Scope);
        }

        /// <summary>
        /// Returns <c>true</c> if the given scope is a valid value.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        bool IsValidScope(string scope) => scope switch
        {
            JavaScopes.COMPILE => true,
            JavaScopes.RUNTIME => true,
            JavaScopes.PROVIDED => true,
            JavaScopes.SYSTEM => true,
            JavaScopes.TEST => true,
            _ => false,
        };

    }

}
