using System;
using System.Diagnostics;

using Microsoft.Build.Utilities;

using org.eclipse.aether.transfer;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// A transfer listener that logs uploads/downloads to <see cref="Trace"/>.
    /// </summary>
    class MavenTransferListener : AbstractTransferListener
    {

        readonly TaskLoggingHelper log;
        readonly bool noError;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="noError"></param>
        public MavenTransferListener(TaskLoggingHelper log, bool noError = false)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.noError = noError;
        }

        /// <summary>
        /// Invoked when a transfer is started.
        /// </summary>
        /// <param name="transferEvent"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void transferInitiated(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));

            var message = transferEvent.getRequestType() == TransferEvent.RequestType.PUT ? "Uploading {0}: {1}" : "Downloading {0}: {1}";
            log.LogMessage(message, transferEvent.getResource().getResourceName(), transferEvent.getResource().getRepositoryUrl());
        }

        /// <summary>
        /// Invoked when a transfer progress is reported.
        /// </summary>
        /// <param name="transferEvent"></param>
        public override void transferProgressed(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));
        }

        public override void transferSucceeded(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));

            var message = transferEvent.getRequestType() == TransferEvent.RequestType.PUT ? "Uploaded {0}: {1}" : "Downloaded {0}: {1}";
            log.LogMessage(message, transferEvent.getResource().getResourceName(), transferEvent.getResource().getRepositoryUrl());
        }

        public override void transferFailed(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));

            if (noError == false)
            {
jmn                if (transferEvent.getException() is Exception e && e is not MetadataNotFoundException)
                    log.LogErrorFromException(e, true, true, null);
                else
                    log.LogErrorFromResources("Error.MavenTransferFailed", transferEvent.getResource().getResourceName());
            }
            else
            {
                if (transferEvent.getException() is Exception e && e is not MetadataNotFoundException)
                    log.LogWarningFromException(e, true);
                else
                    log.LogWarningFromResources("Error.MavenTransferFailed", transferEvent.getResource().getResourceName());
            }
        }

        public override void transferCorrupted(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));

            if (noError == false)
            {
                if (transferEvent.getException() is Exception e)
                    log.LogErrorFromException(e, true, true, null);
                else
                    log.LogErrorFromResources("Error.MavenTransferCorrupted", transferEvent.getResource().getResourceName());
            }
            else
            {
                if (transferEvent.getException() is Exception e)
                    log.LogWarningFromException(e, true);
                else
                    log.LogWarningFromResources("Error.MavenTransferCorrupted", transferEvent.getResource().getResourceName());
            }
        }

    }

}
