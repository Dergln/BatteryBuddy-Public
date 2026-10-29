using System;
using System.Collections.Generic;
using System.Linq;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.Core.Logic
{
    /// <summary>Eine auszulösende Benachrichtigung samt der für dieses Gerät gewählten Meldungsart.</summary>
    public sealed class BatteryAlert
    {
        public BatteryDevice Device { get; }
        public ChargeLevel Level { get; }
        public NotifyMethods Methods { get; }
        public string CustomSoundPath { get; }
        public string WindowsSoundName { get; }

        public BatteryAlert(BatteryDevice device, ChargeLevel level, NotifyMethods methods = NotifyMethods.WindowsToast, string customSoundPath = "", string windowsSoundName = "")
        {
            Device = device;
            Level = level;
            Methods = methods;
            CustomSoundPath = customSoundPath ?? string.Empty;
            WindowsSoundName = windowsSoundName ?? string.Empty;
        }
    }

    /// <summary>
    /// Entscheidet, wann eine Benachrichtigung nötig ist. Ziel: informieren, aber nicht nerven.
    /// Regeln:
    ///  1. Es wird nur beim Wechsel zu einer SCHLECHTEREN Stufe benachrichtigt (grün→gelb→rot→kritisch, auch Sprünge).
    ///  2. Hysterese: Eine Stufe wird erst verlassen, wenn der Wert die Schwelle um <see cref="Hysteresis"/> Punkte übersteigt.
    ///     Sonst würde ein Gerät, das zwischen 20 % und 21 % schwankt, ständig melden.
    ///  3. Während das Gerät lädt, wird nichts gemeldet und der Zustand zurückgesetzt (nächstes Entladen meldet wieder).
    ///  4. Taucht ein Gerät nach dem Neuverbinden mit niedrigem Stand wieder auf, wird erneut gemeldet,
    ///     aber höchstens einmal pro <see cref="MinRepeat"/> je Gerät und Stufe.
    ///  5. Auslöser "Warnstufe erreicht" (<see cref="DeviceSettings.NotifyOnLevelChange"/>): Beim ersten Kontakt nach dem
    ///     Programmstart wird mit der zuletzt gemerkten Stufe verglichen (siehe <see cref="AlertStateStore"/>).
    ///     So wird eine Verschlechterung gemeldet, die passiert ist, während der PC aus war – ein unveränderter Stand nicht.
    ///  6. Auslöser "beim Programmstart" (<see cref="EvaluateStartup"/>): einmalig jedes Gerät, das gerade in einer
    ///     gewählten Warnstufe ist – unabhängig davon, ob sich etwas verändert hat. Schon gemeldete Stufen werden nicht doppelt gemeldet.
    ///  7. "Benachrichtigen für [x] Prüfintervalle": Jede neue Warnung startet eine Episode (Zähler 1). Jede reguläre
    ///     Intervall-Prüfung mit unveränderter Stufe meldet erneut, bis x erreicht ist. Laden, Stufenwechsel oder
    ///     "Gesehen" (<see cref="Acknowledge"/>) beenden die Wiederholungen. Die Zähler leben nur im Speicher (Start = 0).
    /// </summary>
    public sealed class NotificationDecider
    {
        public const int Hysteresis = 3;
        public static readonly TimeSpan MinRepeat = TimeSpan.FromMinutes(30);

        private sealed class Alerted
        {
            public ChargeLevel Level;
            public DateTimeOffset At;
        }

        private readonly Dictionary<string, ChargeLevel> _state = new Dictionary<string, ChargeLevel>();
        private readonly Dictionary<string, Alerted> _lastAlert = new Dictionary<string, Alerted>();

        /// <summary>
        /// Laufende "Warn-Episode" je Gerät: Seit wann (welche Geräte-Stufe) wird gemeldet, mit welcher Warnstufe,
        /// und wie oft schon. Liegt nur im Speicher, beginnt also nach jedem Programmstart wieder bei 0.
        /// </summary>
        private sealed class Episode
        {
            public ChargeLevel DeviceLevel;   // Stufe des Geräts beim Start der Episode
            public ChargeLevel AlertLevel;    // gemeldete Warnstufe (bestimmt die gewünschte Anzahl)
            public int Count;                 // bisher ausgelöste Meldungen
            public bool Acknowledged;         // vom Nutzer bestätigt ("Gesehen") = als fertig durchgelaufen behandeln
        }

        private readonly Dictionary<string, Episode> _episodes = new Dictionary<string, Episode>();

        // Gemerkte Stufen über Neustarts hinweg (auch für gerade nicht verbundene Geräte) und die in dieser Sitzung gesehenen Geräte.
        private readonly Dictionary<string, ChargeLevel> _remembered;
        private readonly HashSet<string> _seenThisSession = new HashSet<string>();

        /// <param name="remembered">Beim letzten Programmlauf gemerkte Stufen (leer/null = erster Start).</param>
        public NotificationDecider(IDictionary<string, ChargeLevel> remembered = null)
        {
            _remembered = remembered == null
                ? new Dictionary<string, ChargeLevel>()
                : new Dictionary<string, ChargeLevel>(remembered);
        }

        /// <summary>Aktuell gemerkte Stufen zum Speichern.</summary>
        public IReadOnlyDictionary<string, ChargeLevel> RememberedLevels => _remembered;

        /// <summary>True, wenn sich gemerkte Stufen seit dem letzten <see cref="MarkStateSaved"/> geändert haben.</summary>
        public bool HasUnsavedState { get; private set; }

        public void MarkStateSaved() => HasUnsavedState = false;

        /// <summary>True, solange mindestens eine Warnung noch Wiederholungen offen hat (für "Alle Warnungen bestätigen").</summary>
        public bool HasOpenRepeats(AppSettings settings)
            => _episodes.Any(kv => !kv.Value.Acknowledged && kv.Value.Count < RepeatFor(settings.GetDevice(kv.Key), kv.Value.AlertLevel));

        /// <summary>
        /// "Gesehen": Die laufende Warnung des Geräts gilt als fertig durchgelaufen, es folgen keine Wiederholungen mehr.
        /// Wird das Gerät später noch schlechter (neue Stufe), wird wieder normal gemeldet. Rückgabe: ob es etwas zu bestätigen gab.
        /// </summary>
        public bool Acknowledge(string deviceId)
        {
            if (deviceId == null || !_episodes.TryGetValue(deviceId, out var episode) || episode.Acknowledged)
                return false;
            episode.Acknowledged = true;
            return true;
        }

        /// <summary>Bestätigt alle laufenden Warnungen (Tray-Menü). Rückgabe: Anzahl der bestätigten Geräte.</summary>
        public int AcknowledgeAll()
        {
            var count = 0;
            foreach (var episode in _episodes.Values.Where(e => !e.Acknowledged))
            {
                episode.Acknowledged = true;
                count++;
            }
            return count;
        }

        /// <param name="isIntervalCheck">
        /// True nur für die reguläre Hintergrundprüfung im eingestellten Intervall. Nur sie zählt für
        /// "Benachrichtigen für [x] Prüfintervalle"; Abfragen nach Geräteänderungen oder "Jetzt aktualisieren" wiederholen nichts.
        /// </param>
        public IReadOnlyList<BatteryAlert> Evaluate(IEnumerable<BatteryDevice> devices, AppSettings settings, DateTimeOffset now, bool isIntervalCheck = false)
        {
            var alerts = new List<BatteryAlert>();
            var present = new HashSet<string>();

            foreach (var device in devices)
            {
                present.Add(device.Id);
                var cfg = settings.GetDevice(device.Id);
                if (cfg.Ignore || !device.Level.HasValue)
                    continue;

                var firstContact = _seenThisSession.Add(device.Id);

                if (device.IsCharging)
                {
                    _state[device.Id] = ChargeLevel.Green;
                    Remember(device.Id, ChargeLevel.Green);
                    _episodes.Remove(device.Id); // Laden beendet die laufende Warnung samt Wiederholungen
                    continue;
                }

                // Vergleichsbasis: Stand dieser Sitzung; beim ersten Kontakt seit Programmstart der gemerkte Stand (Regel 5).
                ChargeLevel previous;
                if (!_state.TryGetValue(device.Id, out previous))
                    previous = firstContact && _remembered.TryGetValue(device.Id, out var r) ? r : ChargeLevel.Green;

                var current = ApplyHysteresis(previous, device.Level.Value, cfg);
                _state[device.Id] = current;
                Remember(device.Id, current);

                // Jeder Stufenwechsel (besser oder schlechter) beendet die bisherige Episode.
                if (_episodes.TryGetValue(device.Id, out var running) && running.DeviceLevel != current)
                    _episodes.Remove(device.Id);

                if (current > previous && cfg.NotifyOnLevelChange)
                {
                    var alertLevel = ChooseAlertLevel(previous, current, cfg);
                    if (alertLevel != ChargeLevel.Unknown && !IsSuppressed(device.Id, alertLevel, now))
                    {
                        alerts.Add(Fire(device, cfg, current, alertLevel, now));
                        continue;
                    }
                }

                // Regel 7: Wiederholung nur bei der regulären Intervall-Prüfung, solange die Stufe gleich bleibt.
                if (isIntervalCheck && _episodes.TryGetValue(device.Id, out var episode) && !episode.Acknowledged
                    && IsEnabled(cfg, episode.AlertLevel) && episode.Count < RepeatFor(cfg, episode.AlertLevel))
                {
                    episode.Count++;
                    _lastAlert[device.Id] = new Alerted { Level = episode.AlertLevel, At = now };
                    alerts.Add(new BatteryAlert(device, episode.AlertLevel, cfg.Methods, cfg.CustomSoundPath, cfg.WindowsSoundName));
                }
            }

            // Verschwundene Geräte vergessen: Beim Neuverbinden wird neu bewertet (siehe Regel 4).
            foreach (var gone in _state.Keys.Where(k => !present.Contains(k)).ToList())
                _state.Remove(gone);

            // Alte Einträge entfernen, damit die Liste nicht unbegrenzt wächst.
            foreach (var old in _lastAlert.Where(kv => now - kv.Value.At > TimeSpan.FromDays(1)).Select(kv => kv.Key).ToList())
                _lastAlert.Remove(old);

            return alerts;
        }

        /// <summary>
        /// Auslöser "beim Programmstart": meldet einmal jedes Gerät, das gerade in einer gewählten Warnstufe ist.
        /// Sollte erst aufgerufen werden, wenn die Startabfragen fertig sind (langsame Provider wie Logitech brauchen einige Sekunden).
        /// Stufen, die in dieser Sitzung bereits gemeldet wurden (z. B. durch "Warnstufe erreicht"), werden nicht doppelt gemeldet.
        /// </summary>
        public IReadOnlyList<BatteryAlert> EvaluateStartup(IEnumerable<BatteryDevice> devices, AppSettings settings, DateTimeOffset now)
        {
            var alerts = new List<BatteryAlert>();
            foreach (var device in devices)
            {
                var cfg = settings.GetDevice(device.Id);
                if (cfg.Ignore || !cfg.NotifyOnStartup || !device.Level.HasValue || device.IsCharging)
                    continue;

                // Bereits ausgewerteter Stand (mit Hysterese) hat Vorrang; sonst direkt aus den Schwellen.
                var current = _state.TryGetValue(device.Id, out var s) ? s : ThresholdEvaluator.Evaluate(device.Level.Value, cfg);
                var alertLevel = ChooseAlertLevel(ChargeLevel.Green, current, cfg);
                if (alertLevel == ChargeLevel.Unknown || IsSuppressed(device.Id, alertLevel, now))
                    continue;

                alerts.Add(Fire(device, cfg, current, alertLevel, now));
            }
            return alerts;
        }

        /// <summary>Erste Meldung einer neuen Warnung: merkt sie für die Unterdrückung (Regel 4) und startet die Episode (Zähler = 1).</summary>
        private BatteryAlert Fire(BatteryDevice device, DeviceSettings cfg, ChargeLevel deviceLevel, ChargeLevel alertLevel, DateTimeOffset now)
        {
            _lastAlert[device.Id] = new Alerted { Level = alertLevel, At = now };
            _episodes[device.Id] = new Episode { DeviceLevel = deviceLevel, AlertLevel = alertLevel, Count = 1 };
            return new BatteryAlert(device, alertLevel, cfg.Methods, cfg.CustomSoundPath, cfg.WindowsSoundName);
        }

        private static int RepeatFor(DeviceSettings cfg, ChargeLevel alertLevel)
        {
            switch (alertLevel)
            {
                case ChargeLevel.Yellow: return AlertProfile.ClampRepeat(cfg.RepeatYellow);
                case ChargeLevel.Red: return AlertProfile.ClampRepeat(cfg.RepeatRed);
                case ChargeLevel.Critical: return AlertProfile.ClampRepeat(cfg.RepeatCritical);
                default: return AlertProfile.MinRepeat;
            }
        }

        /// <summary>Nur Stufen wiederholen, die (noch) gemeldet werden sollen – der Nutzer kann sie inzwischen abgeschaltet haben.</summary>
        private static bool IsEnabled(DeviceSettings cfg, ChargeLevel alertLevel)
            => alertLevel == ChargeLevel.Yellow ? cfg.NotifyYellow
             : alertLevel == ChargeLevel.Red ? cfg.NotifyRed
             : alertLevel == ChargeLevel.Critical && cfg.NotifyCritical;

        private void Remember(string id, ChargeLevel level)
        {
            if (_remembered.TryGetValue(id, out var old) && old == level)
                return;
            _remembered[id] = level;
            HasUnsavedState = true;
        }

        private static ChargeLevel ApplyHysteresis(ChargeLevel previous, int percent, DeviceSettings cfg)
        {
            var raw = ThresholdEvaluator.Evaluate(percent, cfg);
            if (raw >= previous)
                return raw; // gleich oder schlechter: sofort übernehmen

            // Besser geworden: Stufe nur verlassen, wenn der Wert deutlich über der Schwelle liegt.
            if (previous == ChargeLevel.Critical && cfg.CriticalAt > 0 && percent <= cfg.CriticalAt + Hysteresis)
                return ChargeLevel.Critical;
            if (previous == ChargeLevel.Red && percent <= cfg.RedAt + Hysteresis)
                return ChargeLevel.Red;
            if (previous == ChargeLevel.Yellow && percent <= cfg.YellowAt + Hysteresis)
                return ChargeLevel.Yellow;

            return raw;
        }

        private static ChargeLevel ChooseAlertLevel(ChargeLevel previous, ChargeLevel current, DeviceSettings cfg)
        {
            if (current == ChargeLevel.Critical)
            {
                if (cfg.NotifyCritical)
                    return ChargeLevel.Critical;
                // Kritisch nicht gewünscht, Rot aber schon: beim direkten Sprung wenigstens als "niedrig" melden.
                if (cfg.NotifyRed && previous < ChargeLevel.Red)
                    return ChargeLevel.Red;
            }

            if (current == ChargeLevel.Red && cfg.NotifyRed)
                return ChargeLevel.Red;

            // Wer "mittel" wünscht, soll auch beim direkten Sprung nach Rot/Kritisch informiert werden – aber nur einmal.
            if (cfg.NotifyYellow && previous == ChargeLevel.Green && current >= ChargeLevel.Yellow)
                return current;

            return ChargeLevel.Unknown;
        }

        private bool IsSuppressed(string id, ChargeLevel level, DateTimeOffset now)
            => _lastAlert.TryGetValue(id, out var last) && last.Level >= level && now - last.At < MinRepeat;
    }
}
