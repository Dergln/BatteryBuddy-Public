using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class RazerBatteryProtocol
    {
        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (info.VendorId == 0x1532 && info.ProductId == 0x0083)
                return OpenMouseLZBatteryProtocols.ReadRazerLevelOnly(info, channel, ct);
            var profile = DeviceCatalog.CustomProfiles.Single(item => item.VendorId == info.VendorId
                && item.ProductId == info.ProductId);
            var transaction = profile.TransactionId ?? throw new InvalidOperationException("Missing Razer transaction ID.");
            var level = Query(channel, transaction, 0x80, ct);
            if (!level.HasValue)
                return null;
            var charging = Query(channel, transaction, 0x84, ct);
            if (!charging.HasValue)
                return null;
            if (charging.Value > 1)
                throw new InvalidDataException("Invalid Razer charging state.");
            return new HidBatteryStatus((int)Math.Round(level.Value * 100.0 / 255), charging.Value == 1);
        }

        internal static byte[] Request(byte transaction, byte command)
        {
            var report = new byte[91];
            report[2] = transaction;
            report[6] = 2;
            report[7] = 7;
            report[8] = command;
            for (var index = 3; index <= 88; index++)
                report[89] ^= report[index];
            return report;
        }

        private static byte? Query(IHidTransport channel, byte transaction, byte command, CancellationToken ct)
        {
            var request = Request(transaction, command);
            var clock = Stopwatch.StartNew();
            var sends = 0;
            for (var attempt = 0; attempt < 12 && clock.ElapsedMilliseconds < 1000; attempt++)
            {
                if (ct.WaitHandle.WaitOne(60))
                    ct.ThrowIfCancellationRequested();
                var send = attempt % 4 == 0 && sends < 3;
                var wire = HidBatteryExchange.Feature(channel, send ? request : null, 0, 91, ct);
                if (send)
                    sends++;
                var offset = wire.Length >= 91 ? 1 : 0;
                if (wire.Length - offset < 90)
                    throw new InvalidDataException("Short Razer feature report.");
                if (wire[offset + 6] != 7 || wire[offset + 7] != command || wire[offset + 1] != transaction)
                    continue;
                byte checksum = 0;
                for (var index = 2; index < 88; index++)
                    checksum ^= wire[offset + index];
                if (checksum != wire[offset + 88])
                    throw new InvalidDataException("Invalid Razer feature checksum.");
                switch (wire[offset])
                {
                    case 1: continue;
                    case 2: return wire[offset + 9];
                    case 4: return null;
                    default: throw new InvalidDataException($"Razer battery request failed (status {wire[offset]:X2}).");
                }
            }
            throw new TimeoutException("Razer battery reply remained busy or belonged to another application.");
        }
    }
}
