using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using java.io;
using java.lang;
using java.text;
using java.util;

using Microsoft.Build.Utilities;

using org.eclipse.aether.transfer;

using Math = System.Math;

namespace IKVM.Maven.Sdk.Tasks
{

    /// <summary>
    /// A transfer listener that logs uploads/downloads to <see cref="Trace"/>.
    /// </summary>
    class MavenTransferListener : AbstractTransferListener
    {

        readonly TaskLoggingHelper log;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public MavenTransferListener(TaskLoggingHelper log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
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

            if (transferEvent.getException() is System.Exception e && e is not MetadataNotFoundException)
                log.LogErrorFromException(e, true, true, null);
            else
                log.LogErrorFromResources("Error.MavenTransferFailed", transferEvent.getResource().getResourceName());

        }

        public override void transferCorrupted(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));

            if (transferEvent.getException() is System.Exception e)
                log.LogErrorFromException(e);
            else
                log.LogErrorFromResources("Error.MavenTransferCorrupted", transferEvent.getResource().getResourceName());
        }

    }

}
