using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using java.io;
using java.lang;
using java.text;
using java.util;

using org.eclipse.aether.transfer;

using Math = System.Math;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// A transfer listener that logs uploads/downloads to <see cref="Trace"/>.
    /// </summary>
    class MavenTransferListener : AbstractTransferListener
    {

        class TracePrintStream : PrintStream
        {

            public TracePrintStream() :
                base(new ByteArrayOutputStream())
            {

            }

            public override void println(object x)
            {
                Trace.WriteLine(x);
            }

        }

       readonly ConcurrentDictionary<TransferResource, Long> downloads = new();
       int lastLength;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MavenTransferListener()
        {

        }

        public override void transferInitiated(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            string message = transferEvent.getRequestType() == TransferEvent.RequestType.PUT ? "Uploading" : "Downloading";

            Trace.WriteLine($"{message}: {transferEvent.getResource().getRepositoryUrl()}{transferEvent.getResource().getResourceName()}");
        }

        public override void transferProgressed(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            TransferResource resource = transferEvent.getResource();
            downloads[resource] = Long.valueOf(transferEvent.getTransferredBytes());

            StringBuilder buffer = new StringBuilder(64);

            foreach (var entry in downloads)
            {
                long total = entry.Key.getContentLength();
                long complete = entry.Value.longValue();

                buffer.append(GetStatus(complete, total)).append("  ");
            }

            int pad = lastLength - buffer.length();
            lastLength = buffer.length();
            this.Pad(buffer, pad);
            buffer.append('\r');

            Trace.Write(buffer);
        }

        string GetStatus(long complete, long total)
        {
            if (total >= 1024)
            {
                return toKB(complete) + "/" + toKB(total) + " KB ";
            }
            else if (total >= 0)
            {
                return complete + "/" + total + " B ";
            }
            else if (complete >= 1024)
            {
                return toKB(complete) + " KB ";
            }
            else
            {
                return complete + " B ";
            }
        }

        private void Pad(StringBuilder buffer, int spaces)
        {
            string block = "                                        ";
            while (spaces > 0)
            {
                int n = Math.Min(spaces, block.Length);
                buffer.append(block, 0, n);
                spaces -= n;
            }
        }

        public override void transferSucceeded(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            TransferCompleted(transferEvent);

            TransferResource resource = transferEvent.getResource();
            long contentLength = transferEvent.getTransferredBytes();
            if (contentLength >= 0)
            {
                string type = (transferEvent.getRequestType() == TransferEvent.RequestType.PUT ? "Uploaded" : "Downloaded");
                string len = contentLength >= 1024 ? toKB(contentLength) + " KB" : contentLength + " B";

                string throughput = "";
                long duration = java.lang.System.currentTimeMillis() - resource.getTransferStartTime();
                if (duration > 0)
                {
                    long bytes = contentLength - resource.getResumeOffset();
                    DecimalFormat format = new DecimalFormat("0.0", new DecimalFormatSymbols(Locale.ENGLISH));
                    double kbPerSec = (bytes / 1024.0) / (duration / 1000.0);
                    throughput = " at " + format.format(kbPerSec) + " KB/sec";
                }

                Trace.WriteLine($"{type}: {resource.getRepositoryUrl()}{resource.getResourceName()} ({len}{throughput})");
            }
        }

        public override void transferFailed(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            TransferCompleted(transferEvent);

            if (!(transferEvent.getException() is MetadataNotFoundException))
            {
                transferEvent.getException().printStackTrace(new TracePrintStream());
            }
        }

        private void TransferCompleted(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            downloads.TryRemove(transferEvent.getResource(), out _);

            StringBuilder buffer = new StringBuilder(64);
            Pad(buffer, lastLength);
            buffer.append('\r');
            Trace.Write(buffer);
        }

        public override void transferCorrupted(TransferEvent transferEvent)
        {
            RequireNonNull(transferEvent);
            transferEvent.getException().printStackTrace(new TracePrintStream());
        }

        protected long toKB(long bytes)
        {
            return (bytes + 1023) / 1024;
        }

        private static void RequireNonNull(TransferEvent transferEvent)
        {
            if (transferEvent is null)
                throw new ArgumentNullException(nameof(transferEvent));
        }

    }

}
