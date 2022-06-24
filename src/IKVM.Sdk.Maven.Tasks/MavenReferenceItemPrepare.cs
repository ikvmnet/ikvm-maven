
using IKVM.Sdk.Maven.Tasks.Resources;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.eclipse.aether.util.artifact;

namespace IKVM.Sdk.Maven.Tasks
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
                var items = MavenReferenceItemUtil.Import(Items);

                // assign other metadata
                foreach (var item in items)
                    AssignMetadata(item);

                return true;
            }
            catch (MavenTaskMessageException e)
            {
                Log.LogErrorWithCodeFromResources(e.MessageResourceName, e.MessageArgs);
                return false;
            }
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
                        throw new MavenTaskMessageException("Error.MavenInvalidGroupId", item.ItemSpec);

                    if (string.IsNullOrWhiteSpace(item.ArtifactId))
                        item.ArtifactId = a.getArtifactId();
                    else if (item.ArtifactId != a.getArtifactId())
                        throw new MavenTaskMessageException("Error.MavenInvalidArtifactId", item.ItemSpec);

                    if (string.IsNullOrWhiteSpace(item.Classifier))
                        item.Classifier = a.getClassifier();
                    else if (item.Classifier != a.getClassifier())
                        throw new MavenTaskMessageException("Error.MavenInvalidClassifier", item.ItemSpec);

                    if (string.IsNullOrWhiteSpace(item.Version))
                        item.Version = a.getVersion();
                    else if (item.Version != a.getVersion())
                        throw new MavenTaskMessageException("Error.MavenInvalidVersion", item.ItemSpec);
                }
            }

            if (string.IsNullOrWhiteSpace(item.GroupId))
                throw new MavenTaskMessageException("Error.MavenMissingGroupId", item.ItemSpec);

            if (string.IsNullOrWhiteSpace(item.ArtifactId))
                throw new MavenTaskMessageException("Error.MavenMissingArtifactId", item.ItemSpec);

            if (string.IsNullOrWhiteSpace(item.Version))
                throw new MavenTaskMessageException("Error.MavenMissingVersion", item.ItemSpec);

            // check that we can construct an artifact out of the coordinates
            var artifact = MavenTaskUtil.TryCreateArtifact(item.GroupId, item.ArtifactId, item.Classifier, item.Version);
            if (artifact == null)
                throw new MavenTaskMessageException("Error.MavenInvalidCoordinates", item.ItemSpec);

            // add default scopes
            if (item.Scopes.Count == 0)
            {
                item.Scopes.Add(JavaScopes.COMPILE);
                item.Scopes.Add(JavaScopes.RUNTIME);
            }

            // validate scopes
            if (item.Scopes != null)
                foreach (var scope in item.Scopes)
                    ValidateScope(item, scope);

            // replace itemspec with normalized values
            item.ItemSpec = MavenReferenceItemUtil.GetItemSpec(artifact.getGroupId(), artifact.getArtifactId(), artifact.getClassifier(), artifact.getVersion());

            // save item
            item.Save();
        }

        /// <summary>
        /// Validates the scope value.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="scope"></param>
        /// <exception cref="MavenTaskMessageException"></exception>
        void ValidateScope(MavenReferenceItem item, string scope)
        {
            switch (scope)
            {
                case JavaScopes.COMPILE:
                case JavaScopes.RUNTIME:
                case JavaScopes.PROVIDED:
                case JavaScopes.TEST:
                    break;
                default:
                    throw new MavenTaskMessageException("Error.MavenInvalidScope", item.ItemSpec, scope);
            }
        }

    }

}
