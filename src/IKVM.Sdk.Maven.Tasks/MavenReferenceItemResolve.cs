
using System;
using System.Collections.Generic;

using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.apache.maven.project;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/>, resolves the full set of IkvmReferenceItem's that should be generated.
    /// </summary>
    public class MavenReferenceItemResolve : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemResolve() :
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
        /// Set of IkvmReferenceItem to emit.
        /// </summary>
        [Required]
        [Output]
        public ITaskItem[] ResolvedItems { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            try
            {
                var items = MavenReferenceItemUtil.Import(Items);
                var resolvedItems = new List<ITaskItem>();

                foreach (var item in items)
                    resolvedItems.Add(GetResolvedItem(item));

                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        TaskItem GetResolvedItem(MavenReferenceItem item)
        {
            var i = new TaskItem(item.GroupId);
            i.SetMetadata("Compile", item.ItemSpec);
            return i;
        }

        /// <summary>
        /// Assigns the metadata to the item.
        /// </summary>
        /// <param name="item"></param>
        void AssignMetadata(MavenReferenceItem item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemSpec) == false)
            {
                // if the itemspec is parsable as coordinates, we should attempt to apply or validate metadata
                var a = MavenTaskUtil.TryParseArtifact(item.ItemSpec);
                if (a != null)
                {
                    if (string.IsNullOrWhiteSpace(item.GroupId))
                        item.GroupId = a.getGroupId();
                    else if (item.GroupId != a.getGroupId())
                        throw new MavenTaskMessageException("Error.MavenInvalidGroupId");

                    if (string.IsNullOrWhiteSpace(item.ArtifactId))
                        item.ArtifactId = a.getArtifactId();
                    else if (item.ArtifactId != a.getArtifactId())
                        throw new MavenTaskMessageException("Error.MavenInvalidArtifactId");

                    if (string.IsNullOrWhiteSpace(item.Version))
                        item.Version = a.getVersion();
                    else if (item.Version != a.getVersion())
                        throw new MavenTaskMessageException("Error.MavenInvalidVersion");
                }
            }

            if (string.IsNullOrWhiteSpace(item.GroupId))
                throw new MavenTaskMessageException("Error.MavenMissingGroupId");
            
            if (string.IsNullOrWhiteSpace(item.ArtifactId))
                throw new MavenTaskMessageException("Error.MavenMissingArtifactId");

            if (string.IsNullOrWhiteSpace(item.Version))
                throw new MavenTaskMessageException("Error.MavenMissingVersion");

            // check that we can construct an artifact out of the coordinates
            var artifact = MavenTaskUtil.TryCreateArtifact(item.GroupId, item.ArtifactId, item.Version);
            if (artifact == null)
                throw new MavenTaskMessageException("Error.MavenInvalidCoordinates");

            // replace itemspec with normalized values
            item.ItemSpec = $"{artifact.getGroupId()}:{artifact.getArtifactId()}:{artifact.getVersion()}";
        }

    }

}
