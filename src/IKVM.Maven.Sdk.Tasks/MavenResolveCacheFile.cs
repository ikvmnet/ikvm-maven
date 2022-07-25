using IKVM.Maven.Sdk.Tasks.Aether;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using org.eclipse.aether.graph;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Maintains a set of previous data mapped to previous resolutions.
    /// </summary>
    class MavenResolveCacheFile
    {

        /// <summary>
        /// Version of the cache file.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Repositories against which resolution happened.
        /// </summary>
        [JsonProperty("repositories")]
        public MavenRepositoryItem[] Repositories { get; set; }

        /// <summary>
        /// Set of maven references that have been previously resolved.
        /// </summary>
        [JsonProperty("dependencies")]
        public Dependency[] Dependencies { get; set; }

        /// <summary>
        /// Result of previous resolution
        /// </summary>
        [JsonProperty("graph")]
        public DefaultDependencyNode Graph { get; set; }

    }

}
