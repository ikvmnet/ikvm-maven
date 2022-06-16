using System;

using org.eclipse.aether.artifact;

namespace IKVM.Sdk.Maven.Tasks
{

    static class MavenTaskUtil
    {

        /// <summary>
        /// Attempts to parse the coordinates into a Maven artifact.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public static Artifact TryParseArtifact(string coords)
        {
            if (string.IsNullOrWhiteSpace(coords))
                throw new ArgumentException($"'{nameof(coords)}' cannot be null or whitespace.", nameof(coords));
            
            try
            {
                return new DefaultArtifact(coords);
            }
            catch (java.lang.IllegalArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to create a Maven artifact from the given coordinate values.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="artifactId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Artifact TryCreateArtifact(string groupId, string artifactId, string version)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException($"'{nameof(groupId)}' cannot be null or whitespace.", nameof(groupId));
            if (string.IsNullOrWhiteSpace(artifactId))
                throw new ArgumentException($"'{nameof(artifactId)}' cannot be null or whitespace.", nameof(artifactId));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException($"'{nameof(version)}' cannot be null or whitespace.", nameof(version));

            return new DefaultArtifact(groupId, artifactId, "", version);
        }

    }

}
