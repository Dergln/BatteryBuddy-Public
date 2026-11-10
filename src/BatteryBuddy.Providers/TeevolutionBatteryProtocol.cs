using System;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class TeevolutionBatteryProtocol
    {
        internal static bool HasKnownIdentity(HidInterfaceInfo info)
            => info != null && info.VendorId == 0x3554
                && (info.ProductId == 0xF520 || info.ProductId == 0xF523
                    || info.ProductId == 0xF5BB || info.ProductId == 0xF522)
                && info.InputReportLength == 17 && info.OutputReportLength == 17
                && !(info.UsagePage == 0xFF08 && info.Usage == 2);

        internal static bool IsBatteryInterface(HidInterfaceInfo info)
            => HasKnownIdentity(info)
                && info.InputReportIds?.Length == 1 && info.InputReportIds[0] == 8
                && info.OutputReportIds?.Length == 1 && info.OutputReportIds[0] == 8;

        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!IsBatteryInterface(info))
                throw new NotSupportedException("This HID collection is not a documented Teevolution battery endpoint.");
            if (!(channel is IHidInputQueue queue))
                throw new NotSupportedException("Teevolution battery queries require input-queue flushing.");

            // The factory's plain online query has no host-control enable/disable parameter.
            var online = Query(channel, queue, 3, ct);
            if (online[6] == 0)
                return null;
            if (online[6] != 1)
                throw new InvalidDataException("Invalid Teevolution online status.");

            var battery = Query(channel, queue, 4, ct);
            if (battery[7] > 1)
                throw new InvalidDataException("Invalid Teevolution charging status.");
            return battery[6] <= 100 ? new HidBatteryStatus(battery[6], battery[7] == 1) : null;
        }

        private static byte[] Query(IHidTransport channel, IHidInputQueue queue, byte command, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var request = new byte[17];
            request[0] = 8;
            request[1] = command;
            request[16] = (byte)(0x55 - 8 - command);
            queue.FlushInput();
            ct.ThrowIfCancellationRequested();
            if (!channel.Write(request))
                throw new IOException("Unable to request Teevolution battery status.");
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 2 && bytes[0] == 8 && bytes[1] == command,
                ct, timeoutMs: 1200, maxReports: 32);
            ct.ThrowIfCancellationRequested();
            if (reply.Length != 17 || (reply.Sum(value => (int)value) & 255) != 0x55 || reply[2] != 0)
                throw new InvalidDataException("Invalid Teevolution battery reply framing, checksum or status.");
            return reply;
        }
    }
}
