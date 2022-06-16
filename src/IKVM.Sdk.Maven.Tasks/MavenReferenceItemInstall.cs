
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
    /// For each <see cref="MavenReferenceItem"/>, ensures that the appropriate items are installed locally.
    /// </summary>
    public class MavenReferenceItemInstall : Task
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenReferenceItemInstall() :
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
                var items = MavenReferenceItemUtil.Import(Items);

                foreach (var item in items)
                    InstallItem(item);

                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
        }

        void InstallItem(MavenReferenceItem item)
        {
            // do the thing
        }

    }

}
