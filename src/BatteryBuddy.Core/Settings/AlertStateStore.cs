using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.Core.Settings
{
    /// <summary>
    /// Merkt sich die zuletzt bewertete Warnstufe je Gerät über Programmneustarts hinweg (Datei "alert-state.json").
    /// Damit erkennt der Auslöser "Warnstufe erreicht" auch Verschlechterungen, die passiert sind, während der PC aus war,
    /// und meldet einen unveränderten Stand nach dem Neustart nicht erneut.
    /// Getrennt von settings.json, weil sich dieser Zustand laufend ändert und keine Einstellung des Nutzers ist.
    /// Sicherheit: Wie bei den Einstellungen gelten Größenlimit, strikte Typen und eine Prüfung jedes Eintrags.
    /// Ist die Datei defekt, startet BatteryBuddy einfach ohne gemerkten Zustand (wie beim allerersten Start).
    /// </summary>
    public sealed class AlertStateStore
    {
        public const long MaxFileBytes = 256 * 1024;

        [DataContract]
        private sealed class Entry
        {
            [DataMember(Name = "id")] public string Id { get; set; }
            [DataMember(Name = "level")] public int Level { get; set; }
        }

        private readonly string _path;
        private readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(List<Entry>));

        public AlertStateStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Pfad fehlt.", nameof(path));
            _path = path;
        }

        /// <summary>Liefert die gemerkten Stufen; fehlende oder defekte Datei = leeres Ergebnis.</summary>
        public Dictionary<string, ChargeLevel> Load()
        {
            var result = new Dictionary<string, ChargeLevel>();
            try
            {
                var info = new FileInfo(_path);
                if (!info.Exists || info.Length > MaxFileBytes)
                    return result;

                using (var stream = File.OpenRead(_path))
                {
                    var entries = _serializer.ReadObject(stream) as List<Entry>;
                    foreach (var e in entries ?? new List<Entry>())
                    {
                        if (e == null || result.Count >= AppSettings.MaxDevices)
                            continue;
                        var id = BatteryDevice.Sanitize(e.Id, BatteryDevice.MaxIdLength);
                        // Nur echte Stufen übernehmen; "Unknown" oder manipulierte Zahlen werden verworfen.
                        if (id.Length == 0 || e.Level < (int)ChargeLevel.Green || e.Level > (int)ChargeLevel.Critical)
                            continue;
                        result[id] = (ChargeLevel)e.Level;
                    }
                }
            }
            catch (Exception ex) when (ex is SerializationException
                                       || ex is InvalidCastException
                                       || ex is IOException
                                       || ex is UnauthorizedAccessException
                                       || ex is ArgumentException
                                       || ex is FormatException)
            {
                result.Clear();
            }
            return result;
        }

        /// <summary>Speichert atomar (Temp-Datei, dann ersetzen). Höchstens <see cref="AppSettings.MaxDevices"/> Einträge.</summary>
        public void Save(IReadOnlyDictionary<string, ChargeLevel> levels)
        {
            if (levels == null)
                throw new ArgumentNullException(nameof(levels));

            var entries = levels
                .Where(kv => !string.IsNullOrEmpty(kv.Key) && kv.Value >= ChargeLevel.Green && kv.Value <= ChargeLevel.Critical)
                .Take(AppSettings.MaxDevices)
                .Select(kv => new Entry { Id = kv.Key, Level = (int)kv.Value })
                .ToList();

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path)));
            var temp = _path + ".tmp";
            using (var stream = File.Create(temp))
            {
                _serializer.WriteObject(stream, entries);
            }

            if (File.Exists(_path))
                File.Replace(temp, _path, null);
            else
                File.Move(temp, _path);
        }
    }
}
