using System;
using System.Collections.Generic;
using System.Linq;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>Ein eigener Tray-Eintrag: Kategorie-Bild plus Wert eines Geraets.</summary>
    public sealed class TrayEntry
    {
        public DeviceCategory Category { get; }
        public int Percent { get; }
        public ChargeLevel Level { get; }
        public bool IsCharging { get; }
        public string DeviceName { get; }
        public string Key { get; }

        public TrayEntry(DeviceCategory category, int percent, ChargeLevel level, bool isCharging, string deviceName, string key = null)
        {
            Category = category;
            Percent = percent;
            Level = level;
            IsCharging = isCharging;
            DeviceName = deviceName;
            Key = key ?? "category:" + category;
        }
    }

    public static class TraySummaryBuilder
    {
        /// <summary>
        /// Pro Kategorie zählt nur das am niedrigsten geladene Gerät (auch bei mehreren Headsets).
        /// Im Geraetemodus zaehlt stattdessen jedes explizit ausgewaehlte Geraet.
        /// Ausgeblendete Geräte, Geräte ohne Wert und Geräte, die sich länger als
        /// <see cref="AppSettings.TrayActiveMinutes"/> nicht gemeldet haben (veralteter Wert), werden ignoriert.
        /// </summary>
        public static IReadOnlyList<TrayEntry> Build(IEnumerable<BatteryDevice> devices, AppSettings settings, DateTimeOffset? now = null)
        {
            if (settings.TrayMode == AdditionalTrayMode.None)
                return new List<TrayEntry>();
            var oldestAllowed = (now ?? DateTimeOffset.Now) - TimeSpan.FromMinutes(settings.TrayActiveMinutes);
            var eligible = devices
                .Select(d => new { Device = d, Cfg = settings.GetDevice(d.Id) })
                .Where(x => !x.Cfg.Ignore && x.Device.Level.HasValue && x.Device.LastContact >= oldestAllowed)
                // Warnschwellen gelten je Geraet, unabhaengig von dessen Benachrichtigungsschaltern.
                .Where(x => settings.AlwaysShowAdditionalTrayIcons
                    || ThresholdEvaluator.Evaluate(x.Device.Level, x.Cfg) >= ChargeLevel.Yellow)
                .OrderBy(x => x.Device.Id, StringComparer.Ordinal)
                .GroupBy(x => x.Device.Id, StringComparer.Ordinal).Select(g => g.First()).ToList();
            if (settings.TrayMode == AdditionalTrayMode.PerDevice)
                return eligible.Where(x => x.Cfg.ShowInTray)
                    .Select(x => new TrayEntry(ThresholdEvaluator.EffectiveCategory(x.Device, x.Cfg),
                        x.Device.Level.Value, ThresholdEvaluator.Evaluate(x.Device.Level, x.Cfg),
                        x.Device.IsCharging, x.Device.Name, "device:" + x.Device.Id))
                    .OrderBy(e => e.Key, StringComparer.Ordinal).ToList();

            return eligible.GroupBy(x => ThresholdEvaluator.EffectiveCategory(x.Device, x.Cfg))
                .Select(g =>
                {
                    var lowest = g.OrderBy(x => x.Device.Level.Value).ThenBy(x => x.Device.Id, StringComparer.Ordinal).First();
                    return new TrayEntry(
                        g.Key,
                        lowest.Device.Level.Value,
                        ThresholdEvaluator.Evaluate(lowest.Device.Level, lowest.Cfg),
                        lowest.Device.IsCharging,
                        lowest.Device.Name);
                })
                // Stabile Reihenfolge: bei schwankenden Werten springen die Symbole nicht staendig umher.
                .OrderBy(e => e.Category)
                .ToList();
        }
    }
}
