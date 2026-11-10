using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Logitech;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal interface IOpenMouseValkyrieBatteryTransport
    {
        bool RequestIdentity(int timeoutMs);
    }

    // Independent battery-only implementations from documented wire facts.
    // Provenance, exact identities and upstream licenses are recorded in the integration manifest.
    internal static class OpenMouseLZBatteryProtocols
    {
        private static void Pause(CancellationToken ct, int milliseconds)
        {
            ct.ThrowIfCancellationRequested();
            if (ct.WaitHandle.WaitOne(milliseconds))
                ct.ThrowIfCancellationRequested();
        }

        private static void Require(bool valid, string protocol)
        {
            if (!valid)
                throw new InvalidDataException("Invalid " + protocol + " battery reply.");
        }

        private static byte[] FeatureQuery(IHidTransport channel, byte[] request, byte id, int length,
            CancellationToken ct, int settleMs = 25)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("Feature reports are required for battery status.");
            if (length < 1 || length > 1024)
                throw new InvalidOperationException("Invalid battery feature-report size.");
            if (request != null && !feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request battery status.");
            if (request != null)
                Pause(ct, settleMs);
            return HidBatteryExchange.Feature(channel, null, id, length, ct);
        }

        internal static HidBatteryStatus ReadCompx(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte target = 2;
            if (info.VendorId == 0x373E && (info.ProductId == 0x006A
                || info.ProductId == 0x0032 || info.ProductId == 0x0054))
                target = 0;
            var request = new byte[65];
            request[3] = target;
            request[4] = 2;
            request[6] = 0x83;
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var reply = FeatureQuery(channel, attempt == 0 ? request : null, 0, 65, ct);
                var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
                if (reply.Length - offset < 8)
                    throw new InvalidDataException("Short CompX battery reply.");
                if (reply[offset + 1] != 0 || reply[offset + 2] != target || reply[offset + 3] != 2
                    || reply[offset + 4] != 0 || reply[offset + 5] != 0x83)
                    continue;
                if (reply[offset] == 0xA0 || reply[offset] == 0xA3)
                {
                    Pause(ct, 50);
                    continue;
                }
                Require(reply[offset] == 0xA1, "CompX status");
                Require(reply[offset + 6] <= 1, "CompX charging");
                return new HidBatteryStatus(reply[offset + 7], reply[offset + 6] == 1);
            }
            throw new TimeoutException("No matching CompX battery reply.");
        }

        internal static HidBatteryStatus ReadCompxPower(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var request = new byte[17];
            request[0] = 8;
            request[1] = 4;
            request[16] = 0x49;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => HidBatteryExchange.StartsWith(bytes, 8, 4), ct);
            Require(reply.Length >= 17, "CompX power length");
            var sum = 0;
            for (var index = 0; index < 17; index++)
                sum += reply[index];
            Require((sum & 255) == 0x55, "CompX power checksum");
            Require(reply[2] <= 1, "CompX power status");
            if (reply[2] != 0)
                return null;
            Require(reply[7] <= 1, "CompX power charging");
            return new HidBatteryStatus(reply[6], reply[7] == 1);
        }

        internal static BatteryReading ReadLogitechAdc(HidppDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var feature = device.Call(0, 0, 0x1F, 0x20);
            if (feature.Status != HidppStatus.Ok)
                throw new IOException("Unable to discover Logitech ADC battery support.");
            Require(feature.Payload.Length >= 1, "Logitech ADC feature");
            if (feature.Payload[0] == 0)
                return null;
            ct.ThrowIfCancellationRequested();
            var reply = device.Call(feature.Payload[0], 0);
            if (reply.Status != HidppStatus.Ok)
                throw new IOException("Unable to read Logitech ADC battery status.");
            Require(reply.Payload.Length >= 3, "Logitech ADC");
            var millivolts = reply.Payload[0] * 256 + reply.Payload[1];
            var flags = reply.Payload[2];
            if ((flags & 1) == 0 || millivolts == 0)
                return null;
            Require(millivolts >= 2500 && millivolts <= 5000, "Logitech ADC voltage");
            return new BatteryReading { Percent = HidPp.VoltageToPercent(millivolts), Charging = (flags & 2) != 0 };
        }

        internal static HidBatteryStatus ReadModdo(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var reply = FeatureQuery(channel, null, 4, info.FeatureReportLength, ct);
            Require(reply.Length >= 5 && reply[0] == 4, "moddoMOUSE");
            if ((reply[4] & 1) == 0)
                return null;
            Require(reply[3] >= 0x44 && reply[3] <= 0x47, "moddoMOUSE charging");
            return new HidBatteryStatus(reply[1], reply[3] == 0x44);
        }

        internal static HidBatteryStatus ReadRazerLevelOnly(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (info.VendorId != 0x1532 || info.ProductId != 0x0083)
                throw new InvalidOperationException("The level-only exception is specific to Basilisk X HyperSpeed.");
            var request = RazerBatteryProtocol.Request(0xFF, 0x80);
            for (var attempt = 0; attempt < 4; attempt++)
            {
                var reply = FeatureQuery(channel, attempt == 0 ? request : null, 0, 91, ct, 60);
                Require(reply.Length == 91 && reply[0] == 0, "Razer report length");
                if (reply[2] != 0xFF || reply[7] != 7 || reply[8] != 0x80)
                    continue;
                byte checksum = 0;
                for (var index = 3; index <= 88; index++)
                    checksum ^= reply[index];
                Require(checksum == reply[89], "Razer checksum");
                if (reply[1] == 1)
                    continue;
                if (reply[1] == 4)
                    return null;
                Require(reply[1] == 2 && reply[6] >= 2, "Razer status");
                return new HidBatteryStatus((int)Math.Round(reply[10] * 100.0 / 255), false);
            }
            throw new TimeoutException("No matching Razer level-only battery reply.");
        }

        internal static HidBatteryStatus ReadMotospeed(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (info.ProductId != 0xFFE0)
                throw new InvalidOperationException("Motospeed wired battery bytes are not meaningful.");
            if (info.OutputReportLength != 61 && info.OutputReportLength != 64)
                throw new InvalidOperationException("Unknown Motospeed settings-report size.");
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0xB3, 6 },
                bytes => HidBatteryExchange.StartsWith(bytes, 0xB4, 6), ct);
            Require(reply.Length >= 21, "Motospeed X6");
            return new HidBatteryStatus(reply[20] & 0x7F, (reply[20] & 0x80) != 0);
        }

        internal static HidBatteryStatus ReadPulsarPro(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0xB4, 0 },
                bytes => bytes.Length >= 1 && (bytes[0] == 0xB4
                    || (bytes.Length >= 2 && bytes[0] == 0 && bytes[1] == 0xB4)), ct);
            var offset = reply[0] == 0 ? 1 : 0;
            Require(reply.Length >= offset + 7 && reply[offset + 6] <= 1, "Pulsar Pro");
            return new HidBatteryStatus(reply[offset + 1], reply[offset + 6] == 1);
        }

        internal static HidBatteryStatus ReadPulsarXs1(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            // Report zero has a Windows report-id slot AND the protocol's leading zero.
            var request = new byte[65];
            request[2] = 8;
            request[3] = 0x81;
            request[4] = 1;
            request[63] = 0x8A;
            var reply = FeatureQuery(channel, request, 0, 65, ct);
            Require(reply.Length == 65 && HidBatteryExchange.StartsWith(reply, 0, 0, 8, 0x81, 1),
                "Pulsar XS-1");
            return new HidBatteryStatus(reply[7], false);
        }

        internal static HidBatteryStatus ReadVaxee(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte Query(byte command)
            {
                var request = new byte[64];
                request[0] = 0x0E;
                request[1] = 0xA5;
                request[2] = command;
                request[3] = 1;
                request[4] = 1;
                var reply = FeatureQuery(channel, request, 0x0E, 64, ct, 100);
                Require(reply.Length >= 7 && HidBatteryExchange.StartsWith(reply, 0x0E, 0xA5, command, 3, 1)
                    && reply[5] >= 1, "VAXEE");
                return reply[6];
            }
            var steps = Query(0x0B);
            Require(steps <= 20, "VAXEE level");
            var charging = Query(0x10);
            Require(charging <= 1, "VAXEE charging");
            return new HidBatteryStatus(steps * 5, charging == 1);
        }

        internal static HidBatteryStatus ReadValkyrie(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IOpenMouseValkyrieBatteryTransport identity))
                throw new NotSupportedException("Valkyrie requires an exact-device read-only identity output override.");
            (channel as IHidInputQueue)?.FlushInput();
            if (!identity.RequestIdentity(HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request Valkyrie battery identity.");
            var report = HidBatteryExchange.Receive(channel,
                bytes => (bytes.Length == 32 && bytes[0] == 0x10)
                    || (bytes.Length == 33 && bytes[0] == 0 && bytes[1] == 0x10), ct);
            var offset = report.Length == 33 ? 1 : 0;
            Require(report[offset + 1] == 0 && report[offset + 2] == 1 && report[offset + 3] == 11,
                "Valkyrie identity header");
            var sum = 0;
            for (var index = offset + 4; index < offset + 31; index++)
                sum += report[index];
            Require((sum & 255) == report[offset + 31], "Valkyrie identity checksum");
            var model = System.Text.Encoding.ASCII.GetString(report, offset + 4, 4);
            Require(model == "M220" || model == "E023", "Valkyrie model");
            return new HidBatteryStatus(report[offset + 13], false);
        }

        internal static HidBatteryStatus ReadOrbital(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var command = new byte[65];
            command[1] = 0x41;
            command[3] = 0x81;
            command[4] = 1;
            command[64] = 0xDE;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, command,
                bytes => bytes.Length >= 5 && bytes[0] == 0 && (bytes[1] == 1 || bytes[1] == 0x41)
                    && bytes[4] == 1, ct);
            Require(reply.Length >= 65, "Orbital");
            var sum = 0;
            for (var index = 1; index <= 64; index++)
                sum += reply[index];
            Require((sum & 255) == 0xA1, "Orbital checksum");
            var stateIndex = info.ProductId == 0x080B ? 11 : 6;
            var state = reply[stateIndex];
            Require(state <= 3, "Orbital charging");
            // DMS v2 state 3 is normal battery operation, not a charging indication.
            return new HidBatteryStatus(reply[stateIndex + 1],
                state == 1 || (info.ProductId == 0x0746 && state == 3));
        }

        internal static HidBatteryStatus ReadRyunix(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 1 && bytes[0] == 4, ct);
            Require(reply.Length >= 7 && reply[1] <= 1 && reply[5] <= 2, "Ryunix telemetry");
            if (reply[1] == 0)
                return null;
            return new HidBatteryStatus(reply[4], reply[5] == 1);
        }

        internal static HidBatteryStatus ReadMchoseV3(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
            => ReadMchoseV3(info, channel, ct, null);

        internal static HidBatteryStatus ReadMchoseV3(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct,
            Func<long> elapsedMilliseconds)
        {
            ct.ThrowIfCancellationRequested();
            Require(info.OutputReportLength == 64, "MCHOSE V3 output descriptor");
            var request = new byte[64];
            request[0] = 0x4D;
            request[1] = 1;
            request[2] = 1;
            request[5] = 9;
            request[8] = 8;
            (channel as IHidInputQueue)?.FlushInput();
            if (!channel.Write(request))
                throw new IOException("Unable to request MCHOSE V3 battery status.");
            // Offline receiver replies arrive after about 1001 ms; a one-second budget loses them.
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 6 && bytes[0] == 0x4D && bytes[4] == 0 && bytes[5] == 9,
                ct, 1500, elapsedMilliseconds: elapsedMilliseconds);
            Require(reply.Length >= 9 && reply[1] == 1, "MCHOSE V3");
            var length = reply[3];
            Require(length <= 55 && reply.Length >= 8 + length, "MCHOSE V3 length");
            if (reply[2] == 1)
            {
                Require(reply.Length > 8 + length, "MCHOSE V3 checksum length");
                byte checksum = 0;
                for (var index = 2; index < 8 + length; index++)
                    checksum ^= reply[index];
                Require(checksum == reply[8 + length], "MCHOSE V3 checksum");
            }
            if (length == 1 && reply[8] == 0xFF)
                return null;
            Require(length >= 14, "MCHOSE V3 identity");
            if (reply[18] == 0)
                return null;
            Require(reply[19] <= 1, "MCHOSE V3 charging");
            return new HidBatteryStatus(reply[20], reply[19] == 1);
        }

        internal static HidBatteryStatus ReadRapooFeature(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var reply = FeatureQuery(channel, null, 0x2A, info.FeatureReportLength, ct);
            Require(reply.Length >= 10 && reply[0] == 0x2A, "Rapoo feature");
            return new HidBatteryStatus(reply[9], false);
        }

        internal static HidBatteryStatus ReadRapoo(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidInputReportTransport input))
                throw new NotSupportedException("Rapoo requires bounded GET_REPORT(Input).");
            var request = new byte[32];
            request[0] = 0xBA;
            request[1] = info.ProductId == 0x1205 ? (byte)0xA5 : (byte)0xFF;
            request[2] = 0xAA;
            for (var attempt = 0; attempt < 3; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                if (!channel.Write(request))
                    throw new IOException("Unable to request Rapoo battery status.");
                for (var poll = 0; poll < 3; poll++)
                {
                    Pause(ct, 25);
                    var reply = input.GetInputReport(0xBA, 32, HidBatteryExchange.TimeoutMs)
                        ?? throw new IOException("Unable to read Rapoo battery status.");
                    Require(reply.Length >= 4 && reply[0] == 0xBA, "Rapoo input");
                    if (reply[1] != 1 || (reply[2] != 1 && reply[2] != 2))
                        continue;
                    return new HidBatteryStatus(reply[3], reply[2] == 2);
                }
            }
            throw new TimeoutException("No active Rapoo battery answer.");
        }

        internal static HidBatteryStatus ReadRedragonM690(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            byte[] Query(byte command)
            {
                var request = new byte[8];
                request[0] = 5;
                request[1] = command;
                var reply = FeatureQuery(channel, request, 5, 8, ct, 50);
                Require(reply.Length >= 4 && reply[0] == 5 && reply[1] == command, "Redragon M690 PRO");
                return reply;
            }
            var identity = Query(1);
            Require(identity.Length >= 6 && identity[2] == 0x32 && identity[3] == 0x39
                && identity[4] == 0x34 && identity[5] == 0x35, "Redragon M690 model");
            if (info.ProductId == 0x002F && Query(0x80)[2] != 1)
                return null;
            var status = Query(0x90);
            if ((status[2] & 1) != 0)
                return new HidBatteryStatus(status[3], false);
            if (status[3] == 1)
                return null;
            Require(status[3] == 2, "Redragon M690 charging");
            return new HidBatteryStatus(100, false);
        }

        internal static HidBatteryStatus ReadSteelSeries(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            var product = info.ProductId;
            var rival = product == 0x1830 || product == 0x1726 || product == 0x172B;
            var rivalV2 = product == 0x1872;
            var gen2 = product == 0x1892 || product == 0x1890;
            var wired = product == 0x183A || product == 0x187A || product == 0x1854
                || product == 0x185A || product == 0x185E || product == 0x1876
                || product == 0x184A || product == 0x1842 || product == 0x1862 || product == 0x1892;
            var command = rival ? new byte[] { 0, 0xAA, 1 }
                : rivalV2 ? new byte[] { 0, 0xAA, 1 }
                : new byte[] { 0, wired ? (byte)0x92 : (byte)0xD2 };
            var minimum = rival ? 3 : 2;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, command,
                bytes => bytes.Length == minimum || (bytes.Length >= minimum + 1 && bytes[0] == 0), ct, 50);
            var offset = reply.Length == minimum ? 0 : 1;
            if (rival)
            {
                Require(reply[offset + 2] <= 1, "SteelSeries charging");
                return new HidBatteryStatus(reply[offset], reply[offset + 2] == 1);
            }
            if (rivalV2)
                return new HidBatteryStatus(reply[offset] | (reply[offset + 1] << 8), false);
            if (gen2)
                return new HidBatteryStatus(reply[offset + 1], false);
            var encoded = reply[offset + 1];
            var step = encoded & 0x7F;
            if (step == 0)
                return null;
            Require(step <= 21, "SteelSeries battery step");
            return new HidBatteryStatus((step - 1) * 5, (encoded & 0x80) != 0);
        }
    }
}
