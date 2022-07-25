using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Describes a Maven repository.
    /// </summary>
    class MavenRepositoryItem : IEquatable<MavenRepositoryItem>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenRepositoryItem()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="url"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public MavenRepositoryItem(string id, string url)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        /// <summary>
        /// ID of the repository.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// URL of the repository.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as MavenRepositoryItem);
        }

        public bool Equals(MavenRepositoryItem other)
        {
            return other is not null && Id == other.Id && Url == other.Url;
        }

        public override int GetHashCode()
        {
            int hashCode = 315393214;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Url);
            return hashCode;
        }

    }

}
