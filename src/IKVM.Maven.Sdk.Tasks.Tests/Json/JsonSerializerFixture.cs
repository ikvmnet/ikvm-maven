using System.Text.Json;

using IKVM.Maven.Sdk.Tasks.Json;

namespace IKVM.Maven.Sdk.Tasks.Tests.Json
{

    /// <summary>
    /// Mirrors the serializer configuration used by <see cref="MavenReferenceItemResolve"/> for the resolution cache
    /// file. A fresh set of options is handed out per operation because <see cref="PreserveReferenceHandler"/> keeps
    /// its resolver, and therefore its reference identity table, for the lifetime of the handler.
    /// </summary>
    static class JsonSerializerFixture
    {

        static JsonSerializerOptions CreateOptions() => new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new BooleanConverter(),
                new ArtifactJsonConverter(),
                new DefaultArtifactJsonConverter(),
                new DependencyNodeJsonConverter(),
                new DefaultDependencyNodeJsonConverter(),
                new DependencyJsonConverter(),
                new ExclusionJsonConverter(),
                new RemoteRepositoryJsonConverter(),
                new VersionJsonConverter(),
                new VersionConstraintJsonConverter(),
            },
            MaxDepth = 1024,
            ReferenceHandler = new PreserveReferenceHandler(),
        };

        /// <summary>
        /// Serializes the given value the same way the resolution cache file is written.
        /// </summary>
        public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, CreateOptions());

        /// <summary>
        /// Deserializes the given JSON the same way the resolution cache file is read.
        /// </summary>
        public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, CreateOptions());

        /// <summary>
        /// Serializes and then deserializes the given value.
        /// </summary>
        public static T RoundTrip<T>(T value) => Deserialize<T>(Serialize(value));

    }

}
