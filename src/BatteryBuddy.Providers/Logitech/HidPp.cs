using System;
using System.Collections.Generic;
using System.Text;

namespace BatteryBuddy.Providers.Logitech
{
    internal enum HidppStatus { Ok, Timeout, Error }

    /// <summary>Antwort eines Logitech-Geräts auf eine Anfrage.</summary>
    internal sealed class HidppReply
    {
        public HidppStatus Status { get; set; }
        public byte[] Payload { get; set; } = new byte[0];
        public byte ErrorCode { get; set; }
        public static readonly HidppReply Timeout = new HidppReply { Status = HidppStatus.Timeout };
    }

    internal sealed class BatteryReading
    {
        public int? Percent { get; set; }
        public bool Charging { get; set; }
    }

    /// <summary>
    /// Nachrichtenformat von Logitechs HID++ 2.0 (dokumentiert u. a. in den quelloffenen Projekten Solaar und libratbag).
    /// Eine "lange" Nachricht hat 20 Byte:
    ///   [0] Report-ID 0x11, [1] Geräteindex, [2] Feature-Index, [3] (Funktion &lt;&lt; 4) | Software-ID, [4..19] Parameter/Antwort.
    /// "Feature" = eine Fähigkeit des Geräts (z. B. Akku). Ihre Position im Gerät erfragt man über Feature 0 ("IRoot").
    /// Wir senden ausschließlich lesende Anfragen.
    /// </summary>
    internal static class HidPp
    {
        public const byte LongReportId = 0x11;
        public const int LongLength = 20;

        /// <summary>Wird in jeder Anfrage mitgeschickt und vom Gerät zurückgespiegelt. Damit erkennen wir UNSERE Antwort
        /// und ignorieren Nachrichten anderer Programme (z. B. Logi Options+).</summary>
        public const byte SoftwareId = 0x05;

        public const byte DirectDeviceIndex = 0xFF;

        // Feature-IDs (16 Bit)
        public const ushort FeatureRoot = 0x0000;
        public const ushort FeatureDeviceName = 0x0005;
        public const ushort FeatureBatteryStatus = 0x1000;
        public const ushort FeatureBatteryVoltage = 0x1001;
        public const ushort FeatureUnifiedBattery = 0x1004;

        public static byte[] BuildRequest(byte deviceIndex, byte featureIndex, byte function, params byte[] parameters)
        {
            if (function > 0x0F)
                throw new ArgumentOutOfRangeException(nameof(function));
            if (parameters != null && parameters.Length > LongLength - 4)
                throw new ArgumentException("Zu viele Parameter.", nameof(parameters));

            var data = new byte[LongLength];
            data[0] = LongReportId;
            data[1] = deviceIndex;
            data[2] = featureIndex;
            data[3] = (byte)((function << 4) | SoftwareId);
            if (parameters != null)
                Array.Copy(parameters, 0, data, 4, parameters.Length);
            return data;
        }

        /// <summary>Prüft, ob eine empfangene Nachricht die Antwort auf unsere Anfrage ist. Null = gehört nicht zu uns.</summary>
        public static HidppReply Match(byte[] data, byte deviceIndex, byte featureIndex, byte function)
        {
            if (data == null || data.Length < 6 || (data[0] != 0x10 && data[0] != LongReportId) || data[1] != deviceIndex)
                return null;

            var funcAndSw = (byte)((function << 4) | SoftwareId);

            if (data[2] == featureIndex && data[3] == funcAndSw)
            {
                var payload = new byte[data.Length - 4];
                Array.Copy(data, 4, payload, 0, payload.Length);
                return new HidppReply { Status = HidppStatus.Ok, Payload = payload };
            }

            // Fehlerantworten: HID++ 2.0 nutzt Index 0xFF, HID++ 1.0 den Sub-ID 0x8F. Beide wiederholen unsere Anfrage.
            if ((data[2] == 0xFF && data[3] == featureIndex && data[4] == funcAndSw)
                || (data[2] == 0x8F && data[3] == featureIndex && data[4] == funcAndSw))
            {
                return new HidppReply { Status = HidppStatus.Error, ErrorCode = data[5] };
            }

            return null;
        }

        // ---- Akku-Auswertung ----

        /// <summary>Feature 0x1000 "BatteryStatus": [Ladestand %, nächste Stufe %, Status].</summary>
        public static BatteryReading ParseBatteryStatus(byte[] p)
        {
            if (p == null || p.Length < 3 || p[0] > 100)
                return null;

            // Status: 1 lädt, 2 fast voll, 4 lädt langsam; 0 entlädt, 3 voll
            var charging = p[2] == 1 || p[2] == 2 || p[2] == 4;
            return new BatteryReading { Percent = p[0], Charging = charging };
        }

        /// <summary>Feature 0x1004 "UnifiedBattery" (neuere Geräte): [Ladestand %, Stufen-Bits, Ladestatus].</summary>
        public static BatteryReading ParseUnifiedBattery(byte[] p)
        {
            if (p == null || p.Length < 3)
                return null;

            var charging = p[2] == 1 || p[2] == 2;
            int? percent = p[0] <= 100 && p[0] > 0 ? p[0] : (int?)null;

            // Manche Geräte liefern keinen Prozentwert, nur Stufen (kritisch/niedrig/gut/voll). Dann grobe Näherung.
            if (percent == null)
            {
                if ((p[1] & 0x08) != 0) percent = 100;
                else if ((p[1] & 0x04) != 0) percent = 50;
                else if ((p[1] & 0x02) != 0) percent = 20;
                else if ((p[1] & 0x01) != 0) percent = 5;
            }

            return percent == null ? null : new BatteryReading { Percent = percent, Charging = charging };
        }

        /// <summary>Feature 0x1001 "BatteryVoltage": [Spannung mV (2 Byte), Flags]. Prozent wird aus einer Entladekurve geschätzt.</summary>
        public static BatteryReading ParseBatteryVoltage(byte[] p)
        {
            if (p == null || p.Length < 3)
                return null;

            var millivolt = (p[0] << 8) | p[1];
            if (millivolt < 2500 || millivolt > 5000)
                return null;

            return new BatteryReading { Percent = VoltageToPercent(millivolt), Charging = (p[2] & 0x80) != 0 };
        }

        // Typische Entladekurve einer 1-Zellen-Lithium-Batterie (Näherung): Spannung in mV -> Prozent.
        private static readonly int[,] VoltageCurve =
        {
            { 4186, 100 }, { 4067, 90 }, { 3989, 80 }, { 3922, 70 }, { 3859, 60 }, { 3811, 50 },
            { 3778, 40 }, { 3751, 30 }, { 3714, 20 }, { 3665, 10 }, { 3600, 5 }, { 3300, 0 }
        };

        internal static int VoltageToPercent(int millivolt)
        {
            if (millivolt >= VoltageCurve[0, 0]) return 100;
            for (var i = 1; i < VoltageCurve.GetLength(0); i++)
            {
                if (millivolt >= VoltageCurve[i, 0])
                {
                    // Lineare Interpolation zwischen zwei Stützpunkten
                    double span = VoltageCurve[i - 1, 0] - VoltageCurve[i, 0];
                    double share = (millivolt - VoltageCurve[i, 0]) / span;
                    return (int)Math.Round(VoltageCurve[i, 1] + share * (VoltageCurve[i - 1, 1] - VoltageCurve[i, 1]));
                }
            }

            return 0;
        }

        /// <summary>Feature 0x0005: Gerätetyp-Code laut Spezifikation (3 = Maus, 0 = Tastatur, 8 = Headset ...).</summary>
        public static Core.Models.DeviceCategory CategoryFromDeviceType(int type)
        {
            switch (type)
            {
                case 0: case 2: return Core.Models.DeviceCategory.Keyboard;
                case 3: case 4: case 5: return Core.Models.DeviceCategory.Mouse;
                case 8: return Core.Models.DeviceCategory.Headset;
                case 11: case 12: return Core.Models.DeviceCategory.Gamepad;
                case 14: return Core.Models.DeviceCategory.Speaker;
                default: return Core.Models.DeviceCategory.Other;
            }
        }

        /// <summary>Wandelt die vom Gerät gelieferten Namens-Bytes in Text um (nur druckbare ASCII-Zeichen, alles andere wird verworfen).</summary>
        public static string DecodeName(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                if (b == 0) break;
                if (b >= 0x20 && b < 0x7F) sb.Append((char)b);
            }

            return sb.ToString().Trim();
        }
    }
}
