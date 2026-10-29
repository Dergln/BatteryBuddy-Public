using System;
using System.Linq;

namespace BatteryBuddy.Core.Models
{
    /// <summary>Gerätekategorie. Bestimmt das Symbol im Tray und in der Oberfläche.</summary>
    public enum DeviceCategory
    {
        Other = 0,
        Headset = 1,
        Mouse = 2,
        Keyboard = 3,
        Gamepad = 4,
        Phone = 5,
        Speaker = 6,
        InternalBattery = 7,
        Pen = 8,
        // Append new values: existing numeric category overrides must keep their meaning.
        Earbuds = 9
    }

    /// <summary>
    /// Ein Gerät mit Akku und seinem aktuellen Stand. Unveränderlich (immutable):
    /// Nach dem Erzeugen kann nichts mehr geändert werden, das verhindert Seiteneffekte zwischen Threads.
    /// Alle Texte kommen von Treibern/Geräten und gelten als nicht vertrauenswürdig, daher werden sie hier bereinigt.
    /// </summary>
    public sealed class BatteryDevice
    {
        public const int MaxIdLength = 200;
        public const int MaxNameLength = 80;

        /// <summary>Stabile, eindeutige Kennung (z. B. "bt:AABBCCDDEEFF"). Dient als Schlüssel für Einstellungen.</summary>
        public string Id { get; }

        public string Name { get; }

        public DeviceCategory Category { get; }

        /// <summary>Ladestand 0–100 oder null, wenn das Gerät keinen Wert liefert.</summary>
        public int? Level { get; }

        public bool IsCharging { get; }

        /// <summary>Name des Providers, der das Gerät gemeldet hat (nur zur Diagnose).</summary>
        public string Source { get; }

        /// <summary>
        /// Wann das Gerät zuletzt wirklich geantwortet hat. Bei frisch gelesenen Geräten "jetzt"; bei zwischengespeicherten
        /// (z. B. schlafende Funkmaus) der Zeitpunkt der letzten echten Antwort.
        /// </summary>
        public DateTimeOffset LastContact { get; }

        public BatteryDevice(string id, string name, DeviceCategory category, int? level, bool isCharging, string source, DateTimeOffset? lastContact = null)
        {
            LastContact = lastContact ?? DateTimeOffset.Now;
            Id = Sanitize(id, MaxIdLength);
            if (Id.Length == 0)
                throw new ArgumentException("Die Geräte-Id darf nicht leer sein.", nameof(id));

            var cleanName = Sanitize(name, MaxNameLength);
            Name = cleanName.Length == 0 ? Id : cleanName;
            Category = Enum.IsDefined(typeof(DeviceCategory), category) ? category : DeviceCategory.Other;

            // Werte außerhalb 0..100 sind ein Treiberfehler; lieber "unbekannt" als ein falscher Wert.
            Level = level.HasValue && level.Value >= 0 && level.Value <= 100 ? level : null;
            IsCharging = isCharging;
            Source = Sanitize(source, 40);
        }

        /// <summary>Entfernt Steuerzeichen (z. B. Zeilenumbrüche), trimmt und kürzt den Text.</summary>
        internal static string Sanitize(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var cleaned = new string(value.Where(c => !char.IsControl(c)).ToArray()).Trim();
            return cleaned.Length > maxLength ? cleaned.Substring(0, maxLength).Trim() : cleaned;
        }
    }

    /// <summary>Ampelstufe eines Ladestands. Höherer Wert = schlechter (wird so für "wurde schlechter" verglichen).</summary>
    public enum ChargeLevel
    {
        Unknown = 0,
        Green = 1,
        Yellow = 2,
        Red = 3,
        Critical = 4
    }
}
