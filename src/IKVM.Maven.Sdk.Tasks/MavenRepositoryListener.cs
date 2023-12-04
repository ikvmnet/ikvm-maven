using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using org.eclipse.aether;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// A private implementation of <see cref="RepositoryListener"/> that logs to MSBuild.
    /// </summary>
    class MavenRepositoryListener : AbstractRepositoryListener
    {

        readonly TaskLoggingHelper log;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public MavenRepositoryListener(TaskLoggingHelper log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public override void artifactDeployed(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Deployed {repositoryEvent.getArtifact()} to {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void artifactDeploying(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Deploying {repositoryEvent.getArtifact()} to {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void artifactDescriptorInvalid(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Invalid artifact descriptor for {repositoryEvent.getArtifact()}: {repositoryEvent.getException().getMessage()}", MessageImportance.Low);
        }

        public override void artifactDescriptorMissing(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Missing artifact descriptor for {repositoryEvent.getArtifact()}", MessageImportance.Low);
        }

        public override void artifactInstalled(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Installed {repositoryEvent.getArtifact()} to {repositoryEvent.getFile()}", MessageImportance.Low);
        }

        public override void artifactInstalling(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Installing {repositoryEvent.getArtifact()} to {repositoryEvent.getFile()}", MessageImportance.Low);
        }

        public override void artifactResolved(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Resolved artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void artifactDownloading(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Downloading artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void artifactDownloaded(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Downloaded artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void artifactResolving(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Resolving artifact {repositoryEvent.getArtifact()}", MessageImportance.Low);
        }

        public override void metadataDeployed(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Deployed {repositoryEvent.getMetadata()} to {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void metadataDeploying(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Deploying {repositoryEvent.getMetadata()} to {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void metadataInstalled(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Installed {repositoryEvent.getMetadata()} to {repositoryEvent.getFile()}", MessageImportance.Low);
        }

        public override void metadataInstalling(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Installing {repositoryEvent.getMetadata()} to {repositoryEvent.getFile()}", MessageImportance.Low);
        }

        public override void metadataInvalid(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Invalid metadata {repositoryEvent.getMetadata()}", MessageImportance.Low);
        }

        public override void metadataResolved(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Resolved metadata {repositoryEvent.getMetadata()} from {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

        public override void metadataResolving(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));

            log.LogMessageFromText($"Resolving metadata {repositoryEvent.getMetadata()} from {repositoryEvent.getRepository()}", MessageImportance.Low);
        }

    }

}
