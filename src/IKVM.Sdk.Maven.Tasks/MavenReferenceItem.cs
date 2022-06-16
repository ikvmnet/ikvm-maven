using Microsoft.Build.Framework;
using System;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="MavenReferenceItem"/>.
    /// </summary>
    internal class MavenReferenceItem
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public MavenReferenceItem(ITaskItem item)
        {
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>
        /// Referenced node.
        /// </summary>
        public ITaskItem Item { get; }

        /// <summary>
        /// The Maven artifact ID. Required.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The Maven group ID. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The extension of the artifact to download.
        /// </summary>
        public string Extension { get; set; } // TODO: Make enum?

        /// <summary>
        /// The version of the Maven artifact. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The Maven classifier (i.e. "sources"), which is appended to the end of the coords to make it unique.
        /// Using this is required if the artifact was created with a classifier.
        /// </summary>
        public string Classifier { get; set; }
    }
}
