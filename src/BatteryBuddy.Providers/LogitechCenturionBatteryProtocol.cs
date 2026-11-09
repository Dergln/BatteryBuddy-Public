using System;
using System.IO;
using System.Threading;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers
{
    // Protocol facts independently checked against HeadsetControl 0c8a76f (GPLv3),
    // HaloBattery 2251f05 (MIT), and Solaar e7304c4 (GPL-2.0-or-later).
    // This adapter only performs Root.getFeature, battery reads and connection-status reads.
    internal static class LogitechCenturionBatteryProtocol
    {
        private const int MaxFeatureIndex = 64;
        private const byte SoftwareId = 1;

        internal static HidBatteryStatus Read(HidInterfaceInfo info, IHidTransport transport, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (info == null || info.VendorId != 0x046D
                || info.ProductId != 0x0AF7 && info.ProductId != 0x0B18 && info.ProductId != 0x0B45
                || info.UsagePage != 0xFFA0 || info.Usage != 0x0001
                || !HidBatteryIdentity.IsUsbInterface(info.Path, 3)
                || info.InputReportLength < 64 || info.InputReportLength > 1024
                || info.OutputReportLength < 64 || info.OutputReportLength > 1024)
                throw new NotSupportedException("Unsupported Logitech Centurion HID collection.");
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));
            return new Cycle(info, transport, ct).Read();
        }

        private sealed class Offline : Exception { }
        private sealed class BridgeRejected : IOException { }

        private sealed class Cycle
        {
            private readonly HidInterfaceInfo info;
            private readonly IHidTransport transport;
            private readonly CancellationToken ct;
            private readonly byte[] prefix;
            private byte bridge;
            private int transactions;

            internal Cycle(HidInterfaceInfo info, IHidTransport transport, CancellationToken ct)
            {
                this.info = info;
                this.transport = transport;
                this.ct = ct;
                prefix = info.ProductId == 0x0AF7 ? new byte[] { 0x51 } : new byte[] { 0x50, 0x23 };
            }

            internal HidBatteryStatus Read()
            {
                try
                {
                    // Two ID lookups avoid enumerating an untrusted feature count or assuming model indexes.
                    bridge = FeatureIndex(Direct(0, new byte[] { 0, 3 }));
                    var battery = FeatureIndex(Bridged(0, new byte[] { 1, 4 }));
                    var data = Bridged(battery, Array.Empty<byte>());
                    RequireLength(data, 3);
                    if (data[0] > 100 || data[1] > 100 || data[2] > 3)
                        throw new InvalidDataException("Invalid Centurion battery state.");
                    ct.ThrowIfCancellationRequested();
                    return new HidBatteryStatus(data[0], data[2] == 1 || data[2] == 2);
                }
                catch (Offline)
                {
                    return null;
                }
                catch (TimeoutException)
                {
                    if (ProbeOffline())
                        return null;
                    throw;
                }
                catch (BridgeRejected)
                {
                    if (ProbeOffline())
                        return null;
                    throw;
                }
            }

            private static byte FeatureIndex(byte[] data)
            {
                RequireLength(data, 3); // index, flags, version
                if (data[0] == 0)
                    throw new NotSupportedException("Required Centurion feature is not available.");
                if (data[0] > MaxFeatureIndex)
                    throw new InvalidDataException("Centurion feature index exceeds the adapter budget.");
                return data[0];
            }

            private bool ProbeOffline()
            {
                if (bridge == 0)
                    return false;
                try
                {
                    var status = Direct(bridge, Array.Empty<byte>());
                    RequireLength(status, 5);
                    return status[2] == 0 && status[3] == 0;
                }
                catch (Offline) { return true; }
                catch (IOException) { return false; }
                catch (TimeoutException) { return false; }
            }

            private byte[] Direct(byte feature, byte[] parameters)
            {
                var function = SoftwareId;
                var request = new byte[2 + parameters.Length];
                request[0] = feature;
                request[1] = function;
                Array.Copy(parameters, 0, request, 2, parameters.Length);
                var reply = Exchange(request, payload =>
                {
                    if (IsRejection(payload, feature, function))
                    {
                        RequireLength(payload, 4);
                        throw new InvalidDataException("Centurion receiver rejected the read request.");
                    }
                    return payload[0] == feature && payload[1] == function;
                });
                return Slice(reply, 2);
            }

            private byte[] Bridged(byte feature, byte[] parameters)
            {
                var softwareId = SoftwareId;
                var function = (byte)(0x10 | softwareId);
                var request = new byte[7 + parameters.Length];
                request[0] = bridge;
                request[1] = function;
                request[3] = (byte)(3 + parameters.Length);
                request[5] = feature;
                request[6] = softwareId;
                Array.Copy(parameters, 0, request, 7, parameters.Length);
                var reply = Exchange(request, payload =>
                {
                    if (IsRejection(payload, bridge, function))
                    {
                        RequireLength(payload, 4);
                        throw new BridgeRejected();
                    }
                    if (payload[0] != bridge)
                        return false;
                    if (payload[1] == function)
                    {
                        RequireLength(payload, 2); // ACK is not a battery reply.
                        return false;
                    }
                    if (payload[1] != 0x10)
                        return false;
                    if (payload.Length >= 5 && ((payload[2] & 0xF0) != 0 || payload[4] == 0xFF))
                        return false; // Another bridge destination or an unsolicited sub-device notification.
                    if (payload.Length < 7 || payload[2] != 0
                        || payload[3] != payload.Length - 4 || payload[4] != 0)
                        throw new InvalidDataException("Malformed or fragmented Centurion bridge reply.");
                    if (payload[5] == 0xFF && payload[6] == feature)
                    {
                        RequireLength(payload, 9);
                        if (payload[7] == softwareId)
                            throw new BridgeRejected();
                        return false;
                    }
                    return payload[5] == feature && payload[6] == softwareId;
                });
                // A fully correlated MessageEvent may precede the ACK (also supported by Solaar).
                return Slice(reply, 7);
            }

            private byte[] Exchange(byte[] payload, Func<byte[], bool> matches)
            {
                // At most three normal transactions and one offline probe, each bounded by the shared exchange.
                if (++transactions > 4)
                    throw new InvalidOperationException("Centurion battery cycle exceeded its request budget.");
                var request = new byte[prefix.Length + 2 + payload.Length];
                // The prefix already contains the Windows report ID. Output pads on the right
                // to HIDP_CAPS.OutputReportByteLength; prepending a zero would shift the protocol.
                Array.Copy(prefix, request, prefix.Length);
                request[prefix.Length] = (byte)(payload.Length + 1);
                Array.Copy(payload, 0, request, prefix.Length + 2, payload.Length);
                byte[] result = null;
                HidBatteryExchange.Output(transport, info.OutputReportLength, request, report =>
                {
                    var body = Payload(report);
                    if (body == null)
                        return false;
                    if (bridge != 0 && body[0] == bridge && body[1] == 0)
                    {
                        RequireLength(body, 4);
                        if (body[2] != 0 || body[3] > 1)
                            throw new InvalidDataException("Invalid Centurion connection event.");
                        if (body[3] == 0)
                            throw new Offline();
                        return false;
                    }
                    if (!matches(body))
                        return false;
                    result = body;
                    return true;
                }, ct);
                ct.ThrowIfCancellationRequested();
                return result;
            }

            private byte[] Payload(byte[] report)
            {
                if (!HidBatteryExchange.StartsWith(report, prefix))
                    return null;
                if (report.Length < 64 || report.Length > info.InputReportLength)
                    throw new InvalidDataException("Invalid Centurion input report size.");
                var length = report[prefix.Length];
                if (length < 3 || prefix.Length + 1 + length > 64 || report[prefix.Length + 1] != 0)
                    throw new InvalidDataException("Invalid Centurion frame length or fragment flags.");
                var body = new byte[length - 1];
                Array.Copy(report, prefix.Length + 2, body, 0, body.Length);
                return body;
            }

            private static bool IsRejection(byte[] body, byte feature, byte function)
                => body.Length >= 3 && body[0] == 0xFF && body[1] == feature && body[2] == function;

            private static void RequireLength(byte[] data, int length)
            {
                if (data.Length != length)
                    throw new InvalidDataException("Unexpected Centurion reply length.");
            }

            private static byte[] Slice(byte[] data, int offset)
            {
                var result = new byte[data.Length - offset];
                Array.Copy(data, offset, result, 0, result.Length);
                return result;
            }
        }
    }
}
