using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using org.eclipse.aether.util.artifact;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Models the required data of a <see cref="MavenReferenceItem"/>.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    class MavenReferenceItem : IEquatable<MavenReferenceItem>
    {

        /// <summary>
        /// The identity of the item.
        /// </summary>
        public string ItemSpec { get; set; }

        /// <summary>
        /// The Maven group ID. Required.
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// The Maven artifact ID. Required.
        /// </summary>
        public string ArtifactId { get; set; }

        /// <summary>
        /// The Maven classifier. Optional.
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// The version of the Maven reference. Optional.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Whether the reference is optional.
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <summary>
        /// The scopes of this reference
        /// </summary>
        public string Scope { get; set; } = JavaScopes.COMPILE;

        public override bool Equals(object obj)
        {
            return Equals(obj as MavenReferenceItem);
        }

        public bool Equals(MavenReferenceItem other)
        {
            return other is not null &&
                ItemSpec == other.ItemSpec &&
                GroupId == other.GroupId &&
                ArtifactId == other.ArtifactId &&
                Classifier == other.Classifier &&
                Version == other.Version &&
                Optional == other.Optional &&
                Scope == other.Scope;
        }

        public override int GetHashCode()
        {
            int hashCode = 1928079503;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ItemSpec);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GroupId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ArtifactId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Classifier);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Version);
            hashCode = hashCode * -1521134295 + Optional.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Scope);
            return hashCode;
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GroupId}:{ArtifactId}";
        }

    }

}
