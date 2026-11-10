using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    internal static class OpenMouseAKBatteryProtocols
    {
        private static readonly ushort[] GWolvesReport5Products =
        {
            0x5618, 0x3854, 0x2619
        };

        private static readonly ushort[] GWolvesXviProducts =
        {
            0x3908, 0x3917, 0x7904, 0x7913, 0x3708, 0x3717, 0x5308, 0x5317,
            0x7704, 0x7713, 0x3508, 0x3517, 0x7908, 0x7917, 0x5808, 0x5817,
            0x2708, 0x2717, 0x7204, 0x7203, 0x5807, 0x5407, 0x5707, 0x5907,
            0x5408, 0x5417, 0x5708, 0x5717, 0x5908, 0x5917, 0x7708, 0x7717,
            0x5804, 0x5803, 0x5404, 0x5403, 0x5704, 0x5703, 0x5904, 0x5903,
            0x001A, 0x00AA
        };

        private static readonly ushort[] GWolvesXviNewProducts =
        {
            0x3808, 0x3817, 0x6808, 0x6817, 0x5608, 0x5617, 0x3608, 0x3617
        };

        private static readonly ushort[] GWolvesXviWirelessProducts =
        {
            0x3917, 0x7913, 0x3717, 0x5317, 0x7713, 0x3517, 0x7917, 0x5817,
            0x2717, 0x7203, 0x5807, 0x5407, 0x5707, 0x5907, 0x5417, 0x5717,
            0x5917, 0x7717, 0x5803, 0x5403, 0x5703, 0x5903, 0x00AA
        };

        private static readonly ushort[] AjazzProducts = { 0x2157, 0x2158, 0x2167, 0x2168, 0x2177, 0x2178 };

        private static readonly ushort[] GloriousClassicWirelessProducts =
        {
            0x2022, 0x2023, 0x2024, 0x2025, 0x2033, 0x2039, 0x2027, 0x2031, 0x2034, 0xA312, 0xA300
        };

        private static readonly ushort[] Keychron4kProducts =
        {
            0xD040, 0xD046, 0xD03C, 0xD037, 0xD041, 0xD045
        };

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

        internal static HidBatteryStatus ReadGWolvesReport5(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x33E4
                || !GWolvesReport5Products.Contains((ushort)info.ProductId)
                || info.UsagePage != 0xFF02
                || info.Usage != 2
                || ((info.ProductId == 0x5618 || info.ProductId == 0x3854) && info.FeatureReportLength != 0)
                || info.InputReportLength != 17
                || info.OutputReportLength != 17)
                throw new InvalidOperationException("Unknown G-Wolves report-5 identity or report shape.");
            var request = new byte[17];
            request[0] = 8;
            request[1] = 4;
            byte checksum = 8;
            for (var index = 1; index < 16; index++)
                checksum = unchecked((byte)(checksum + request[index]));
            request[16] = unchecked((byte)(0x55 - checksum));
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 8 && bytes[0] == 8 && bytes[1] == 4, ct);
            byte responseSum = 0;
            for (var index = 0; index < 17; index++)
                responseSum = unchecked((byte)(responseSum + reply[index]));
            Require(responseSum == 0x55 && reply[2] == 0, "G-Wolves report-5 checksum/status");
            Require(reply[6] <= 100 && reply[7] <= 1, "G-Wolves report-5 battery fields");
            return new HidBatteryStatus(reply[6], reply[7] == 1);
        }

        internal static HidBatteryStatus ReadGWolvesXvi(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x33E4 || !GWolvesXviProducts.Contains((ushort)info.ProductId))
                throw new InvalidOperationException("Unknown G-Wolves XVI product.");
            if (info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown G-Wolves XVI feature-report length.");
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("G-Wolves XVI battery status requires feature reports.");
            var request = new byte[65];
            request[1] = 2;
            request[2] = 0x8F;
            request[3] = GWolvesXviWirelessProducts.Contains((ushort)info.ProductId) ? (byte)1 : (byte)0;
            var reply = HidBatteryExchange.Feature(channel, request, 0, info.FeatureReportLength, ct);
            var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
            Require(reply.Length - offset >= 6 && reply[offset] == 0xA1
                && reply[offset + 2] == 0x8F, "G-Wolves XVI battery command echo");
            Require(reply[offset + 4] <= 1 && reply[offset + 5] <= 100, "G-Wolves XVI battery fields");
            return new HidBatteryStatus(reply[offset + 5], reply[offset + 4] == 1);
        }

        internal static HidBatteryStatus ReadGWolvesXviNew(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x33E4 || !GWolvesXviNewProducts.Contains((ushort)info.ProductId))
                throw new InvalidOperationException("Unknown G-Wolves XVI:1 product.");
            if (info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown G-Wolves XVI:1 feature-report length.");
            var request = new byte[65];
            request[2] = 2;
            request[3] = 2;
            request[5] = 0x83;
            var reply = HidBatteryExchange.Feature(channel, request, 0, info.FeatureReportLength, ct);
            var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
            Require(reply.Length - offset >= 8 && reply[offset] == 0xA1
                && reply[offset + 5] == 0x83, "G-Wolves XVI:1 battery command echo");
            Require(reply[offset + 6] <= 1 && reply[offset + 7] <= 100, "G-Wolves XVI:1 battery fields");
            return new HidBatteryStatus(reply[offset + 7], reply[offset + 6] == 1);
        }

        internal static HidBatteryStatus ReadAjazz(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if ((info.VendorId != 0xA8A4 && info.VendorId != 0xA8A5)
                || !AjazzProducts.Contains((ushort)info.ProductId)
                || info.UsagePage != 0xFF01 || info.Usage != 0x10
                || info.OutputReportLength != 64)
                throw new InvalidOperationException("Unknown AJAZZ output-report length.");
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength,
                new byte[] { 0xF0, 0x30, 0x01 },
                bytes => bytes.Length >= 10 && bytes[0] == 0xF0 && bytes[1] == 0x30, ct);
            Require(reply[2] == 0x01, "AJAZZ command echo");
            Require(reply[8] <= 100, "AJAZZ percentage");
            return new HidBatteryStatus(reply[8], reply[9] != 0);
        }

        internal static HidBatteryStatus ReadAj200p(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x363C || !IsAj200pProduct(info.ProductId))
                throw new InvalidOperationException("Unknown AJAZZ AJ200P product.");
            if (info.UsagePage != 0xFFA0 || info.Usage != 2 || info.OutputReportLength != 128)
                throw new InvalidOperationException("Unknown AJ200P output-report length.");
            var request = new byte[128];
            request[0] = 0x0B;
            request[1] = 0x13;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 21 && (bytes[0] == 0x0A || bytes[0] == 0x0B)
                    && bytes[1] == 0x13, ct);
            Require(reply[2] == 55 && reply[3] == 0, "AJ200P identity response");
            var percent = reply[18];
            var wireless = reply[20] == 1;
            var charging = !wireless && reply[17] == 1 && percent < 100;
            Require(percent <= 100 && reply[17] <= 1 && reply[20] <= 1, "AJ200P battery fields");
            return new HidBatteryStatus(percent, charging);
        }

        private static bool IsAj200pProduct(int productId)
        {
            switch (productId)
            {
                case 0xED03: case 0xED05: case 0xED06: case 0xED00:
                case 0xED07: case 0xED08: case 0xFC08: case 0xED09:
                case 0xED0A: case 0xFC0A: case 0xED0B: case 0xED0C:
                case 0xFC0C: case 0xED1D: case 0xED1E: case 0xFC1E:
                case 0xED1B: case 0xED1C: case 0xFC1C: case 0xED25:
                case 0xED26: case 0xFC26:
                    return true;
                default:
                    return false;
            }
        }

        internal static HidBatteryStatus ReadBytech(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x372E || (info.ProductId != 0x1014 && info.ProductId != 0x1015)
                || info.UsagePage != 0xFF00 || info.Usage != 1)
                throw new InvalidOperationException("Unknown IPI Float 88 product.");
            if (info.FeatureReportLength != 64)
                throw new InvalidOperationException("Unknown IPI feature-report length.");
            var request = new byte[64];
            request[0] = 3;
            request[2] = 80;
            request[4] = 2;
            request[5] = 79;
            request[6] = 129;
            byte checksum = 0;
            for (var index = 1; index < request.Length; index++)
                checksum = unchecked((byte)(checksum + request[index]));
            request[1] = checksum;
            var reply = HidBatteryExchange.Feature(channel, request, 3, 64, ct);
            var offset = reply.Length == 64 && reply[0] == 3 ? 1 : 0;
            Require(reply.Length - offset >= 6 && reply[offset + 3] == 2, "IPI Float 88 query echo");
            var raw = reply[offset + 5];
            var percent = raw == 0xE4 ? 100 : raw;
            Require(percent <= 100, "IPI Float 88 percentage");
            return new HidBatteryStatus(percent, false);
        }

        internal static HidBatteryStatus ReadAmkette(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x04F3
                || (info.ProductId != 0x026D && info.ProductId != 0x026E && info.ProductId != 0x026F)
                || info.UsagePage != 0x000A || info.Usage != 0)
                throw new InvalidOperationException("Unknown Amkette XS telemetry identity.");
            if (info.FeatureReportLength < 1 || info.InputReportLength < 7)
                throw new InvalidOperationException("Unknown Amkette XS telemetry report lengths.");
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("Amkette XS battery status requires a feature-report channel.");
            (channel as IHidInputQueue)?.FlushInput();
            var trigger = feature.GetFeature(5, info.FeatureReportLength, HidBatteryExchange.TimeoutMs);
            Require(trigger != null && trigger.Length >= 1 && trigger[0] == 5, "Amkette telemetry trigger");
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 7 && bytes[0] == 4, ct);
            var percent = reply[4];
            var charging = reply[5];
            Require((reply[1] == 0 || reply[1] == 1)
                && (reply[3] == 0x08 || reply[3] == 0x04 || reply[3] == 0x02 || reply[3] == 0x01
                    || reply[3] == 0x11 || reply[3] == 0x12 || reply[3] == 0x14)
                && percent <= 100 && charging <= 2, "Amkette telemetry fields");
            return new HidBatteryStatus(percent, charging == 1);
        }

        internal static HidBatteryStatus ReadDareu(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x260D || (info.ProductId != 0x1114 && info.ProductId != 0x1117)
                || info.UsagePage != 0xFF02 || info.Usage != 2)
                throw new InvalidOperationException("Unknown Dareu battery product.");
            if (info.InputReportLength != 17 || info.OutputReportLength != 17)
                throw new InvalidOperationException("Unknown Dareu report-8 length.");
            var request = new byte[17];
            request[0] = 8;
            request[1] = 4;
            request[16] = 0x49;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 8 && bytes[0] == 8 && bytes[1] == 4, ct);
            Require(reply.Length >= 8 && reply[6] <= 100, "Dareu battery percentage");
            Require(reply[7] <= 2, "Dareu charging state");
            return new HidBatteryStatus(reply[6], reply[7] == 1 || reply[7] == 2);
        }

        internal static HidBatteryStatus ReadKsnake(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if ((info.VendorId != 0xA8A4 && info.VendorId != 0xA8A5) || info.ProductId != 0x2255
                || info.UsagePage != 0xFF01 || info.Usage != 0x10)
                throw new InvalidOperationException("Unknown K-snake X11 identity.");
            if (info.OutputReportLength != 65)
                throw new InvalidOperationException("Unknown K-snake output-report length.");
            var request = new byte[65];
            request[1] = 0x55;
            request[2] = 0x30;
            request[3] = 0xA5;
            request[4] = 0x0B;
            request[5] = 0x2E;
            request[6] = 0x01;
            request[7] = 0x01;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 11 && bytes[0] == 0 && bytes[1] == 0x55 && bytes[2] == 0x30, ct);
            Require(reply.Length >= 11 && reply[9] <= 100, "K-snake percentage");
            return new HidBatteryStatus(reply[9], reply[10] != 0);
        }

        internal static HidBatteryStatus ReadAttackSharkX11(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x1D57 || info.ProductId != 0xFA60
                || info.UsagePage != 0x000A || info.InputReportLength < 5)
                throw new InvalidOperationException("Unknown Attack Shark X11 / Delux M600 Pro receiver input profile.");
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 5 && bytes[0] == 3
                    && (bytes[1] == 0x55 || bytes[1] == 0x07 || bytes[1] == 0x20)
                    && bytes[2] == 0x40 && bytes[3] == 0x01, ct);
            Require(reply[4] <= 100, "Attack Shark X11 battery percentage");
            return new HidBatteryStatus(reply[4], false);
        }

        internal static HidBatteryStatus ReadIncott(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x093A
                || (info.ProductId != 0x522C && info.ProductId != 0x622C)
                || info.UsagePage != 0xFF05 || info.InputReportLength < 3)
                throw new InvalidOperationException("Unknown Incott input telemetry identity.");
            var reply = HidBatteryExchange.Receive(channel,
                bytes => bytes.Length >= 3 && bytes[0] == 9, ct);
            var raw = reply[1];
            var charging = raw > 100;
            var percent = charging ? raw - 128 : raw;
            Require(percent >= 0 && percent <= 100, "Incott battery telemetry");
            return new HidBatteryStatus(percent, charging);
        }

        internal static HidBatteryStatus ReadBitmouse(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x373B || (info.ProductId != 0x1154 && info.ProductId != 0x1155))
                throw new InvalidOperationException("Unknown ATK Bitmouse identity.");
            if (info.UsagePage != 0xFF05 || info.Usage != 1 || info.OutputReportLength != 64)
                throw new InvalidOperationException("Unknown ATK Bitmouse output-report length.");
            var target = info.ProductId == 0x1155 ? (byte)1 : (byte)0;
            var level = BitmouseRequest(7, target);
            var levelReply = HidBatteryExchange.Output(channel, info.OutputReportLength, level,
                bytes => BitmouseReplyMatches(bytes, 7, target), ct);
            Require(levelReply[6] >= 1 && levelReply[7] <= 100, "ATK Bitmouse percentage");
            var charge = BitmouseRequest(15, target);
            var chargeReply = HidBatteryExchange.Output(channel, info.OutputReportLength, charge,
                bytes => BitmouseReplyMatches(bytes, 15, target), ct);
            Require(chargeReply[6] >= 1 && chargeReply[7] <= 1, "ATK Bitmouse charging state");
            return new HidBatteryStatus(levelReply[7], chargeReply[7] == 1);
        }

        private static byte[] BitmouseRequest(byte command, byte target)
        {
            var frame = new byte[64];
            frame[0] = 8;
            frame[2] = 0x72;
            frame[3] = 2;
            frame[5] = target;
            frame[6] = command;
            frame[7] = 1;
            byte sum = 0;
            for (var index = 2; index < frame.Length; index++)
                sum = unchecked((byte)(sum + frame[index]));
            frame[1] = sum;
            return frame;
        }

        private static bool BitmouseReplyMatches(byte[] report, byte command, byte target)
        {
            return report.Length >= 8 && report[0] == 8 && report[1] == 0x72
                && report[2] == 0 && report[4] == target && report[5] == command;
        }

        internal static HidBatteryStatus ReadHyperX(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x03F0 || info.ProductId != 0x028E)
                throw new InvalidOperationException("The evidenced HyperX battery query is specific to the Pulsefire Haste wireless receiver.");
            if (info.UsagePage != 0xFF00 || info.Usage != 1
                || info.InputReportLength != 65 || info.OutputReportLength != 65)
                throw new InvalidOperationException("Unknown HyperX output-report length.");
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, new byte[] { 0, 0x51 },
                bytes => bytes.Length >= 7 && (bytes[0] == 0 && bytes[1] == 0x51 || bytes[0] == 0x51), ct);
            var offset = reply[0] == 0 && reply[1] == 0x51 ? 1 : 0;
            var percent = reply[offset + 4];
            var state = reply[offset + 5];
            if (percent == 0 || percent > 100)
                return null;
            Require(state <= 2, "HyperX charging state");
            return new HidBatteryStatus(percent, state == 1);
        }

        internal static HidBatteryStatus ReadGloriousClassic(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if ((info.VendorId != 0x258A && info.VendorId != 0x22D4
                    && info.VendorId != 0x320F && info.VendorId != 0x3794)
                || !GloriousClassicWirelessProducts.Contains((ushort)info.ProductId)
                || info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown Glorious classic feature-report length.");
            return ReadGloriousFeature(info, channel, ct, false);
        }

        internal static HidBatteryStatus ReadGloriousCore2(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x258A
                || (info.ProductId != 0x201B && info.ProductId != 0x2035 && info.ProductId != 0x201C && info.ProductId != 0x2036))
                throw new InvalidOperationException("Unknown Glorious Core 2 identity.");
            if (info.UsagePage != 0xFFFF || info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown Glorious Core 2 feature-report length.");
            return ReadGloriousFeature(info, channel, ct, true);
        }

        private static HidBatteryStatus ReadGloriousFeature(HidInterfaceInfo info, IHidTransport channel,
            CancellationToken ct, bool core2)
        {
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("Glorious battery status requires a feature-report channel.");
            var request = new byte[65];
            if (core2)
            {
                request[3] = 0x02;
                request[4] = 0x02;
                request[6] = 0x83;
            }
            else
            {
                request[3] = 0x02;
                request[4] = 0x02;
                request[6] = 0x83;
            }
            if (!feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request Glorious battery status.");
            Pause(ct, 60);
            var reply = HidBatteryExchange.Feature(channel, null, 0, info.FeatureReportLength, ct);
            var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
            Require(reply.Length - offset >= 8, "Glorious report length");
            if (core2)
            {
                Require(reply[offset] == 0xA1 && reply[offset + 1] == 0
                    && reply[offset + 2] == 2 && reply[offset + 3] == 2
                    && reply[offset + 4] == 0 && reply[offset + 5] == 0x83, "Glorious Core 2 response");
                Require(reply[offset + 6] <= 1 && reply[offset + 7] <= 100, "Glorious Core 2 battery fields");
                return new HidBatteryStatus(reply[offset + 7], reply[offset + 6] == 1);
            }

            Require(reply[offset + 5] == 0x83, "Glorious classic command echo");
            if (reply[offset] != 0xA1)
                return null;
            Require(reply[offset + 6] <= 1 && reply[offset + 7] <= 100, "Glorious classic battery fields");
            return new HidBatteryStatus(reply[offset + 7], reply[offset + 6] == 1);
        }

        internal static HidBatteryStatus ReadGearHub(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (info.VendorId != 0x3151)
                throw new InvalidOperationException("Unknown GearHub vendor identity.");
            if (info.ProductId == 0x402D)
                return ReadGearHubDongle(info, channel, ct);
            if (info.ProductId == 0x402C)
                return ReadGearHubBluetooth(info, channel, ct);
            throw new InvalidOperationException("No battery-only GearHub query is established for this product.");
        }

        internal static HidBatteryStatus ReadGearHubDongle(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3151 || info.ProductId != 0x402D
                || info.UsagePage != 0xFFFF || info.Usage != 2)
                throw new InvalidOperationException("The evidenced GearHub receiver query is specific to VID:PID 3151:402D.");
            if (info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown GearHub receiver feature-report length.");
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("GearHub receiver battery status requires feature reports.");
            var request = new byte[65];
            request[1] = 0xF7;
            if (!feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request GearHub receiver status.");
            Pause(ct, 10);
            var reply = HidBatteryExchange.Feature(channel, null, 0, info.FeatureReportLength, ct);
            var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
            Require(reply.Length - offset >= 5 && reply[offset] == 1 && reply[offset + 4] == 0,
                "GearHub receiver link status");
            Require(reply[offset + 2] <= 100, "GearHub receiver percentage");
            return new HidBatteryStatus(reply[offset + 2], false);
        }

        internal static HidBatteryStatus ReadGearHubBluetooth(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3151 || info.ProductId != 0x402C
                || info.UsagePage != 0xFF55 || info.Usage != 0x0202)
                throw new InvalidOperationException("The evidenced GearHub Bluetooth query is specific to VID:PID 3151:402C.");
            if (info.OutputReportLength != 66)
                throw new InvalidOperationException("Unknown GearHub Bluetooth report length.");
            var request = new byte[66];
            request[0] = 6;
            request[1] = 0x77;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 3 && bytes[0] == 6
                    && (bytes[1] == 0x77 || bytes[1] == 0x88), ct);
            Require(reply[1] == 0x77, "GearHub Bluetooth awake state");
            Require(!ArrayEquals(reply, request), "GearHub Bluetooth response");
            Require(reply[2] <= 100, "GearHub Bluetooth percentage");
            return new HidBatteryStatus(reply[2], false);
        }

        private static bool ArrayEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                return false;
            for (var index = 0; index < left.Length; index++)
                if (left[index] != right[index])
                    return false;
            return true;
        }

        internal static HidBatteryStatus ReadKeychron1k(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3434 || info.ProductId != 0xD033
                || info.UsagePage != 0x008C || info.Usage != 1 || info.FeatureReportLength != 21)
                throw new InvalidOperationException("Unknown Keychron 1K identity or feature-report length.");
            var request = new byte[21];
            request[0] = 0x51;
            request[1] = 0x06;
            var clock = Stopwatch.StartNew();
            for (var attempt = 0; attempt < 15 && clock.ElapsedMilliseconds < HidBatteryExchange.TimeoutMs; attempt++)
            {
                var reply = HidBatteryExchange.Feature(channel, attempt == 0 ? request : null, 0x51, 21, ct);
                var offset = reply.Length == 21 && reply[0] == 0x51 ? 1 : 0;
                if (reply.Length - offset >= 12 && reply[offset] == 0x06
                    && reply[offset + 3] == 0x34 && reply[offset + 4] == 0x34)
                {
                    var percent = reply[offset + 10];
                    var state = reply[offset + 11] & 0x03;
                    if (percent == 0xFF)
                        return null;
                    Require(percent <= 100 && state <= 2, "Keychron 1K battery fields");
                    return new HidBatteryStatus(percent, state == 1);
                }
                if (attempt < 14)
                    Pause(ct, 50);
            }
            throw new TimeoutException("No matching Keychron 1K identity response.");
        }

        internal static HidBatteryStatus ReadKeychron4k(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3434 || !Keychron4kProducts.Contains((ushort)info.ProductId)
                || info.UsagePage != 0xFF0A || info.Usage != 1 || info.OutputReportLength != 65)
                throw new InvalidOperationException("Unknown Keychron 4K identity or report length.");
            var request = new byte[] { 0, 0x01, 0, 0x81, 0x01 };
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 8 && bytes[0] == 0 && bytes[1] == 1
                    && bytes[3] == 0x81 && bytes[4] == 1, ct);
            Require(reply[6] <= 3 && reply[7] <= 100, "Keychron 4K battery fields");
            return new HidBatteryStatus(reply[7], reply[6] == 1 || reply[6] == 3);
        }

        internal static HidBatteryStatus ReadKeychron8k(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3434 || (info.ProductId != 0xD060 && info.ProductId != 0xD029))
                throw new InvalidOperationException("The evidenced Keychron 8K battery path is for M6 and its Link-KM receiver.");
            if (info.UsagePage != 0xFFC1 || info.Usage != 1 || info.OutputReportLength != 64)
                throw new InvalidOperationException("Unknown Keychron 8K output-report length.");
            var request = new byte[64];
            request[0] = 0xB3;
            request[1] = 0x06;
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 21 && bytes[0] == 0xB4 && bytes[1] == 0x06, ct);
            var battery = reply[20];
            Require((battery & 0x7F) <= 100, "Keychron 8K battery fields");
            return new HidBatteryStatus(battery & 0x7F, (battery & 0x80) != 0);
        }

        internal static HidBatteryStatus ReadKeychronNordic(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3434
                || (info.ProductId != 0xD077 && info.ProductId != 0xD05B && info.ProductId != 0xD078))
                throw new InvalidOperationException("Unknown Keychron Nordic identity.");
            if (info.UsagePage != 0xFF0A || info.Usage != 1 || info.OutputReportLength != 65)
                throw new InvalidOperationException("Unknown Keychron Nordic report length.");
            var request = new byte[] { 0, 0x01, 0, 0x81, 0x01 };
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 13 && bytes[0] == 0 && bytes[1] == 1
                    && bytes[3] == 0x81 && bytes[4] == 1, ct);
            var state = reply[11];
            var percent = reply[12];
            Require(state <= 2 && percent <= 100, "Keychron Nordic battery fields");
            return new HidBatteryStatus(percent, state == 1);
        }

        internal static HidBatteryStatus ReadKeychronNape(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3434
                || (info.ProductId != 0x0440 && info.ProductId != 0xD026 && info.ProductId != 0xD029))
                throw new InvalidOperationException("Unknown Keychron Nape identity.");
            if (info.UsagePage != 0xFF60 || info.Usage != 0x0061 || info.OutputReportLength != 33)
                throw new InvalidOperationException("Unknown Keychron Nape report length.");
            var request = new byte[] { 0, 0xA7, 0x31 };
            var reply = HidBatteryExchange.Output(channel, info.OutputReportLength, request,
                bytes => bytes.Length >= 5 && bytes[0] == 0 && bytes[1] == 0xA7 && bytes[2] == 0x31, ct);
            var percent = reply[3];
            var state = reply[4];
            Require(percent <= 100 && state <= 2, "Keychron Nape battery fields");
            return new HidBatteryStatus(percent, state == 1);
        }

        internal static HidBatteryStatus ReadHitscanXvi(HidInterfaceInfo info, IHidTransport channel, CancellationToken ct)
        {
            if (info.VendorId != 0x3770
                || (info.ProductId != 0x0136 && info.ProductId != 0x0194 && info.ProductId != 0x0300))
                throw new InvalidOperationException("Unknown Hitscan Xvi identity.");
            if (info.FeatureReportLength != 65)
                throw new InvalidOperationException("Unknown Hitscan Xvi feature-report length.");
            ct.ThrowIfCancellationRequested();
            if (!(channel is IHidFeatureTransport feature))
                throw new NotSupportedException("Hitscan Xvi battery status requires feature reports.");
            var request = new byte[65];
            request[3] = 2;
            request[4] = 2;
            request[5] = 0;
            request[6] = 0x83;
            if (!feature.SetFeature(request, HidBatteryExchange.TimeoutMs))
                throw new IOException("Unable to request Hitscan battery status.");
            var clock = Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < HidBatteryExchange.TimeoutMs)
            {
                Pause(ct, 15);
                var reply = HidBatteryExchange.Feature(channel, null, 0, info.FeatureReportLength, ct);
                var offset = reply.Length == 65 && reply[0] == 0 ? 1 : 0;
                if (reply.Length - offset >= 8 && reply[offset] == 0xA1
                    && reply[offset + 4] == 0 && reply[offset + 5] == 0x83)
                {
                    Require(reply[offset + 6] <= 1 && reply[offset + 7] <= 100, "Hitscan battery fields");
                    return new HidBatteryStatus(reply[offset + 7], reply[offset + 6] == 1);
                }
            }
            throw new TimeoutException("No matching Hitscan battery response.");
        }
    }
}
