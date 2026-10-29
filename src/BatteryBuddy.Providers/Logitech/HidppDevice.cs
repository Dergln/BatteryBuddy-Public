using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BatteryBuddy.Providers.Native;

namespace BatteryBuddy.Providers.Logitech
{
    /// <summary>
    /// Ein Logitech-Gerät (direkt oder hinter einem Empfänger) mit HID++ 2.0.
    /// Kapselt den Frage-Antwort-Ablauf. Der Transport ist austauschbar (echtes HID oder Fake im Test).
    /// </summary>
    internal sealed class HidppDevice
    {
        private const int PingTimeoutMs = 350;
        private readonly IHidTransport _transport;
        private readonly byte _index;
        private readonly int _timeoutMs;
        private readonly Dictionary<ushort, byte?> _featureCache = new Dictionary<ushort, byte?>();

        public HidppDevice(IHidTransport transport, byte deviceIndex, int timeoutMs)
        {
            _transport = transport;
            _index = deviceIndex;
            _timeoutMs = timeoutMs;
        }

        public byte Index => _index;

        /// <summary>Sendet eine Anfrage und wartet auf die passende Antwort (andere Nachrichten werden übersprungen).</summary>
        public HidppReply Call(byte featureIndex, byte function, params byte[] parameters)
            => CallWithTimeout(_timeoutMs, featureIndex, function, parameters);

        private HidppReply CallWithTimeout(int timeoutMs, byte featureIndex, byte function, params byte[] parameters)
        {
            var request = HidPp.BuildRequest(_index, featureIndex, function, parameters);
            if (!_transport.Write(request))
                return HidppReply.Timeout;

            var clock = Stopwatch.StartNew();
            while (clock.ElapsedMilliseconds < timeoutMs)
            {
                var remaining = (int)Math.Max(1, timeoutMs - clock.ElapsedMilliseconds);
                var data = _transport.Read(remaining);
                if (data == null)
                    break;

                var reply = HidPp.Match(data, _index, featureIndex, function);
                if (reply != null)
                    return reply;
            }

            return HidppReply.Timeout;
        }

        /// <summary>
        /// Fragt die HID++-Protokollversion ab (Root-Feature, Funktion 1). Spricht das Gerät HID++ 2.0, ist die Hauptversion &gt;= 2.
        /// Rückgabe: Hauptversion, 0 bei HID++ 1.0-/Fehlerantwort (z. B. Empfänger selbst), null bei Zeitüberschreitung.
        /// </summary>
        public int? Ping()
        {
            const byte pingData = 0xA5;
            // Kürzeres Limit als bei normalen Anfragen: Leere Empfänger-Slots antworten nie, und davon gibt es bis zu 6.
            var reply = CallWithTimeout(Math.Min(_timeoutMs, PingTimeoutMs), 0x00, 1, 0, 0, pingData);
            if (reply.Status == HidppStatus.Timeout)
                return null;

            if (reply.Status != HidppStatus.Ok || reply.Payload.Length < 3 || reply.Payload[2] != pingData)
                return 0;

            return reply.Payload[0];
        }

        /// <summary>Position eines Features im Gerät (cachet das Ergebnis). Null = Gerät hat das Feature nicht.</summary>
        public byte? FeatureIndex(ushort featureId)
        {
            if (_featureCache.TryGetValue(featureId, out var cached))
                return cached;

            var reply = Call(0x00, 0, (byte)(featureId >> 8), (byte)(featureId & 0xFF));
            byte? index = reply.Status == HidppStatus.Ok && reply.Payload.Length > 0 && reply.Payload[0] != 0
                ? reply.Payload[0]
                : (byte?)null;
            _featureCache[featureId] = index;
            return index;
        }

        public string ReadName()
        {
            var feature = FeatureIndex(HidPp.FeatureDeviceName);
            if (feature == null)
                return null;

            var count = Call(feature.Value, 0);
            if (count.Status != HidppStatus.Ok || count.Payload.Length < 1)
                return null;

            // Der Name kommt in Stücken zu je max. 16 Zeichen; Obergrenze schützt vor fehlerhaften Geräten.
            var total = Math.Min((int)count.Payload[0], 64);
            var bytes = new List<byte>();
            while (bytes.Count < total)
            {
                var part = Call(feature.Value, 1, (byte)bytes.Count);
                if (part.Status != HidppStatus.Ok || part.Payload.Length == 0)
                    break;

                bytes.AddRange(part.Payload.Take(total - bytes.Count));
            }

            var name = HidPp.DecodeName(bytes);
            return name.Length == 0 ? null : name;
        }

        public int? ReadDeviceType()
        {
            var feature = FeatureIndex(HidPp.FeatureDeviceName);
            if (feature == null)
                return null;

            var reply = Call(feature.Value, 2);
            return reply.Status == HidppStatus.Ok && reply.Payload.Length > 0 ? reply.Payload[0] : (int?)null;
        }

        /// <summary>Probiert die Akku-Features vom neuesten zum ältesten durch. Null = Gerät meldet keinen Akkustand.</summary>
        public BatteryReading ReadBattery()
        {
            var unified = FeatureIndex(HidPp.FeatureUnifiedBattery);
            if (unified != null)
            {
                var reply = Call(unified.Value, 1);
                if (reply.Status == HidppStatus.Ok)
                {
                    var reading = HidPp.ParseUnifiedBattery(reply.Payload);
                    if (reading != null)
                        return reading;
                }
            }

            var status = FeatureIndex(HidPp.FeatureBatteryStatus);
            if (status != null)
            {
                var reply = Call(status.Value, 0);
                if (reply.Status == HidppStatus.Ok)
                {
                    var reading = HidPp.ParseBatteryStatus(reply.Payload);
                    if (reading != null)
                        return reading;
                }
            }

            var voltage = FeatureIndex(HidPp.FeatureBatteryVoltage);
            if (voltage != null)
            {
                var reply = Call(voltage.Value, 0);
                if (reply.Status == HidppStatus.Ok)
                    return HidPp.ParseBatteryVoltage(reply.Payload);
            }

            return null;
        }
    }
}
