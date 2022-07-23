using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// Maintains a set of previous data mapped to previous resolutions.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    class MavenResolveCacheFile
    {

        /// <summary>
        /// Version of the cache file.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Repositories against which resolution happened.
        /// </summary>
        public MavenRepositoryItem[] Repositories { get; set; }

        /// <summary>
        /// Set of maven references that have been previously resolved.
        /// </summary>
        public MavenReferenceItem[] Items { get; set; }

        /// <summary>
        /// Result of previous resolution
        /// </summary>
        public IkvmReferenceItem[] ResolvedItems { get; set; }

    }

}
