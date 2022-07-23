using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Describes a Maven repository.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    class MavenRepositoryItem
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
        public string Id { get; set; }

        /// <summary>
        /// URL of the repository.
        /// </summary>
        public string Url { get; set; }

    }

}
