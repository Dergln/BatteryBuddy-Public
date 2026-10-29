using System;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>
    /// Ordnet Geräte einer Kategorie zu. Zwei Quellen:
    ///  1. Bluetooth "Class of Device" (CoD): eine 24-Bit-Zahl, die jedes BT-Gerät meldet (Major-/Minor-Klasse).
    ///  2. Namens-Heuristik als Rückfall (z. B. BLE-Geräte ohne CoD).
    /// Reine Logik ohne Windows-Aufrufe, daher gut testbar.
    /// </summary>
    public static class CategoryClassifier
    {
        /// <summary>CoD cannot distinguish in-ear from over-ear audio; only refine compatible audio/unknown metadata.</summary>
        public static DeviceCategory FromBluetooth(uint? cod, string name)
        {
            var category = cod.HasValue ? FromClassOfDevice(cod.Value) : DeviceCategory.Other;
            var namedCategory = FromName(name);
            if ((category == DeviceCategory.Headset || category == DeviceCategory.Other)
                && namedCategory == DeviceCategory.Earbuds)
                return DeviceCategory.Earbuds;
            return category == DeviceCategory.Other ? namedCategory : category;
        }

        /// <summary>Bluetooth-CoD: Bits 8–12 = Major-Klasse, Bits 2–7 = Minor-Klasse.</summary>
        public static DeviceCategory FromClassOfDevice(uint cod)
        {
            var major = (cod >> 8) & 0x1F;
            var minor = (cod >> 2) & 0x3F;

            switch (major)
            {
                case 0x02: // Phone
                    return DeviceCategory.Phone;

                case 0x04: // Audio/Video
                    // 5 = Lautsprecher, 7 = Portable Audio, 8 = Car Audio, 10 = HiFi, 4 = Mikrofon (zählt nicht als Headset)
                    if (minor == 5 || minor == 7 || minor == 8 || minor == 10)
                        return DeviceCategory.Speaker;
                    return DeviceCategory.Headset; // Headset, Freisprecher, Kopfhörer, Earbuds ...

                case 0x05: // Peripheral (Eingabegeräte)
                    var low = minor & 0x0F;          // Joystick/Gamepad/Digitizer ...
                    var kind = (minor >> 4) & 0x03;  // 1 = Tastatur, 2 = Zeigegerät, 3 = Kombi
                    if (low == 1 || low == 2) return DeviceCategory.Gamepad;
                    if (low == 5 || low == 7) return DeviceCategory.Pen;
                    if (kind == 2) return DeviceCategory.Mouse;
                    if (kind == 1 || kind == 3) return DeviceCategory.Keyboard;
                    return DeviceCategory.Other;

                default:
                    return DeviceCategory.Other;
            }
        }

        /// <summary>Einfache Stichwort-Suche im Gerätenamen (Groß-/Kleinschreibung egal).</summary>
        public static DeviceCategory FromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return DeviceCategory.Other;

            if (Has(name, "mouse", "maus", "mx master", "mx anywhere", "trackball", "g pro x superlight", "g502", "g305"))
                return DeviceCategory.Mouse;
            if (Has(name, "keyboard", "tastatur", "keys", "mx keys", "k380", "k850"))
                return DeviceCategory.Keyboard;
            // AirPods Max are over-ear; generic "pods"/"earphones" are too ambiguous to infer in-ear construction.
            if (Has(name, "airpods max"))
                return DeviceCategory.Headset;
            if (Has(name, "earbud", "ear bud", "in-ear", "in ear", "airpods", "galaxy buds", "pixel buds", "wf-1000"))
                return DeviceCategory.Earbuds;
            if (Has(name, "headset", "headphone", "buds", "earphone", "pods", "ath-", "wh-1000"))
                return DeviceCategory.Headset;
            if (Has(name, "speaker", "soundbar", "boom", "megaboom", "lautsprecher"))
                return DeviceCategory.Speaker;
            if (Has(name, "controller", "gamepad", "dualsense", "dualshock", "xbox", "joy-con", "joystick"))
                return DeviceCategory.Gamepad;
            if (Has(name, "stylus", "pencil", " pen"))
                return DeviceCategory.Pen;
            if (Has(name, "iphone", "phone", "galaxy", "pixel"))
                return DeviceCategory.Phone;

            return DeviceCategory.Other;
        }

        private static bool Has(string text, params string[] needles)
        {
            foreach (var n in needles)
            {
                if (text.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
