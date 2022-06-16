using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// For each <see cref="MavenReferenceItem"/> passed in, validates the metdata.
    /// </summary>
    public class MavenReferenceItemValidate : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemValidate() :
            base(SR.ResourceManager, "MAVEN")
        {

        }

        /// <summary>
        /// <see cref="MavenReferenceItem"/> items to validate.
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
                if (Validate(item) == false)
                    return false;

            return true;
        }

        /// <summary>
        /// Validates the item.
        /// </summary>
        /// <param name="item"></param>
        bool Validate(MavenReferenceItem item)
        {
            return true;
        }

    }

}
