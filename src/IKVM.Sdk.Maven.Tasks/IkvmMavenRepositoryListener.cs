using System;
using System.Diagnostics;

using org.eclipse.aether;

namespace IKVM.Sdk.Maven.Tasks
{
    /// <summary>
    /// A repository listener that logs events to <see cref="Trace"/>.
    /// </summary>
    internal class IkvmMavenRepositoryListener : AbstractRepositoryListener
    {
        public IkvmMavenRepositoryListener()
        {
            Trace.AutoFlush = true;
        }

        public override void artifactDeployed(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Deployed {repositoryEvent.getArtifact()} to {repositoryEvent.getRepository()}");
        }

        public override void artifactDeploying(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Deploying {repositoryEvent.getArtifact()} to {repositoryEvent.getRepository()}");
        }

        public override void artifactDescriptorInvalid(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Invalid artifact descriptor for {repositoryEvent.getArtifact()}: {repositoryEvent.getException().getMessage()}");
        }

        public override void artifactDescriptorMissing(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Missing artifact descriptor for {repositoryEvent.getArtifact()}");
        }

        public override void artifactInstalled(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Installed {repositoryEvent.getArtifact()} to {repositoryEvent.getFile()}");
        }

        public override void artifactInstalling(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Installing {repositoryEvent.getArtifact()} to {repositoryEvent.getFile()}");
        }

        public override void artifactResolved(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Resolved artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}");
        }

        public override void artifactDownloading(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Downloading artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}");
        }

        public override void artifactDownloaded(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Downloaded artifact {repositoryEvent.getArtifact()} from {repositoryEvent.getRepository()}");
        }

        public override void artifactResolving(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Resolving artifact {repositoryEvent.getArtifact()}");
        }

        public override void metadataDeployed(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Deployed {repositoryEvent.getMetadata()} to {repositoryEvent.getRepository()}");
        }

        public override void metadataDeploying(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Deploying {repositoryEvent.getMetadata()} to {repositoryEvent.getRepository()}");
        }

        public override void metadataInstalled(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Installed {repositoryEvent.getMetadata()} to {repositoryEvent.getFile()}");
        }

        public override void metadataInstalling(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Installing {repositoryEvent.getMetadata()} to {repositoryEvent.getFile()}");
        }

        public override void metadataInvalid(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Invalid metadata {repositoryEvent.getMetadata()}");
        }

        public override void metadataResolved(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Resolved metadata {repositoryEvent.getMetadata()} from {repositoryEvent.getRepository()}");
        }

        public override void metadataResolving(RepositoryEvent repositoryEvent)
        {
            RequireNonNull(repositoryEvent);
            Trace.WriteLine($"Resolving metadata {repositoryEvent.getMetadata()} from {repositoryEvent.getRepository()}");
        }

        private static void RequireNonNull(RepositoryEvent repositoryEvent)
        {
            if (repositoryEvent is null)
                throw new ArgumentNullException(nameof(repositoryEvent));
        }
    }
}
