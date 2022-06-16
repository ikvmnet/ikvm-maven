using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/> passed in, assigns default metadata if required.
    /// </summary>
    public class MavenReferenceItemAssignMetadata : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemAssignMetadata() :
            base(SR.ResourceManager, "MAVEN")
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
            var items = MavenReferenceItemUtil.Import(Items);

            // assign other metadata
            foreach (var item in items)
                AssignMetadata(item);

            // save each back to the original task item
            foreach (var item in items)
                item.Save();

            return false;
        }

        /// <summary>
        /// Assigns the metadata to the item.
        /// </summary>
        /// <param name="item"></param>
        void AssignMetadata(MavenReferenceItem item)
        {

        }

    }

}
