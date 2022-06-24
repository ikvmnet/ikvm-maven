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

    }

}
