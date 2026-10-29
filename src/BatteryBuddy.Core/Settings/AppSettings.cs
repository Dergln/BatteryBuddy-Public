using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.Core.Settings
{
    public enum AdditionalTrayMode { None, LowestPerCategory, PerDevice }

    /// <summary>
    /// Einstellungen eines einzelnen Geräts. Wird über die Geräte-Id zugeordnet.
    /// [DataContract]/[DataMember] legen fest, was in der JSON-Datei steht.
    /// </summary>
    [DataContract]
    public sealed class DeviceSettings
    {
        [DataMember(Name = "id")] public string Id { get; set; }

        /// <summary>Zuletzt bekannter Name; nur damit man in der Datei erkennt, welches Gerät gemeint ist.</summary>
        [DataMember(Name = "name")] public string Name { get; set; }

        /// <summary>Gerät komplett ausblenden (kein Tray, keine Benachrichtigung).</summary>
        [DataMember(Name = "ignore")] public bool Ignore { get; set; }
        [DataMember(Name = "showInTray")] public bool ShowInTray { get; set; }

        /// <summary>Kategorie manuell überschreiben, falls die automatische Erkennung falsch liegt.</summary>
        [DataMember(Name = "category")] public DeviceCategory? CategoryOverride { get; set; }

        /// <summary>Bis zu diesem Wert (einschließlich) gilt der Stand als "mittel" (gelb).</summary>
        [DataMember(Name = "yellowAt")] public int YellowAt { get; set; }

        /// <summary>Bis zu diesem Wert gilt der Stand als "niedrig" (rot).</summary>
        [DataMember(Name = "redAt")] public int RedAt { get; set; }

        /// <summary>Bis zu diesem Wert gilt der Stand als "kritisch" (burgunderrot). 0 = nicht in der Datei, wird beim Prüfen ergänzt.</summary>
        [DataMember(Name = "criticalAt")] public int CriticalAt { get; set; }

        [DataMember(Name = "notifyYellow")] public bool NotifyYellow { get; set; }

        [DataMember(Name = "notifyRed")] public bool NotifyRed { get; set; }

        [DataMember(Name = "notifyCritical")] public bool NotifyCritical { get; set; } = true;

        /// <summary>Auslöser "beim Programmstart" (siehe <see cref="AlertProfile.NotifyOnStartup"/>).</summary>
        [DataMember(Name = "notifyOnStartup")] public bool NotifyOnStartup { get; set; } = true;

        /// <summary>Auslöser "Warnstufe erreicht" (siehe <see cref="AlertProfile.NotifyOnLevelChange"/>).</summary>
        [DataMember(Name = "notifyOnLevelChange")] public bool NotifyOnLevelChange { get; set; } = true;

        /// <summary>"Benachrichtigen für [x] Prüfintervalle" je Stufe (siehe <see cref="AlertProfile.RepeatYellow"/>).</summary>
        [DataMember(Name = "repeatYellow")] public int RepeatYellow { get; set; } = AlertProfile.MinRepeat;
        [DataMember(Name = "repeatRed")] public int RepeatRed { get; set; } = AlertProfile.MinRepeat;
        [DataMember(Name = "repeatCritical")] public int RepeatCritical { get; set; } = AlertProfile.MinRepeat;

        /// <summary>Art der Benachrichtigung (Toast/Töne) für dieses Gerät.</summary>
        [DataMember(Name = "notifyVia")] public NotifyMethods Methods { get; set; } = AlertProfile.DefaultMethods;

        /// <summary>Pfad der eigenen WAV-/MP3-Datei (nur lokal, siehe <see cref="AppSettings.SanitizeSoundPath"/>).</summary>
        [DataMember(Name = "customSound")] public string CustomSoundPath { get; set; } = string.Empty;

        /// <summary>Dateiname des gewählten Windows-Systemtons (nur der Name, siehe <see cref="AppSettings.SanitizeWindowsSoundName"/>).</summary>
        [DataMember(Name = "windowsSound")] public string WindowsSoundName { get; set; } = string.Empty;

        // Siehe AppSettings.ApplyDefaults: Beim Einlesen läuft kein Konstruktor, die Vorgaben müssen hier gesetzt werden.
        [OnDeserializing]
        private void ApplyDefaults(StreamingContext context)
        {
            NotifyCritical = true;
            // Ältere Dateien kennen beide Auslöser nicht: beide an entspricht dem bisherigen Verhalten.
            NotifyOnStartup = true;
            NotifyOnLevelChange = true;
            // Ältere Dateien kennen die Wiederholung nicht: 1 = nur einmal melden (bisheriges Verhalten).
            RepeatYellow = RepeatRed = RepeatCritical = AlertProfile.MinRepeat;
            Methods = AlertProfile.DefaultMethods;
            CustomSoundPath = string.Empty;
            WindowsSoundName = string.Empty;
        }

        public AlertProfile ToProfile() => new AlertProfile
        {
            ShowInTray = ShowInTray,
            YellowAt = YellowAt, RedAt = RedAt, CriticalAt = CriticalAt,
            NotifyYellow = NotifyYellow, NotifyRed = NotifyRed, NotifyCritical = NotifyCritical,
            NotifyOnStartup = NotifyOnStartup, NotifyOnLevelChange = NotifyOnLevelChange,
            RepeatYellow = RepeatYellow, RepeatRed = RepeatRed, RepeatCritical = RepeatCritical,
            Methods = Methods, CustomSoundPath = CustomSoundPath, WindowsSoundName = WindowsSoundName
        };

        /// <summary>Übernimmt alle Warn-Einstellungen aus dem Profil (z. B. beim Zurücksetzen auf den Standard).</summary>
        public void ApplyProfile(AlertProfile p)
        {
            ShowInTray = p.ShowInTray;
            YellowAt = p.YellowAt; RedAt = p.RedAt; CriticalAt = p.CriticalAt;
            NotifyYellow = p.NotifyYellow; NotifyRed = p.NotifyRed; NotifyCritical = p.NotifyCritical;
            NotifyOnStartup = p.NotifyOnStartup; NotifyOnLevelChange = p.NotifyOnLevelChange;
            RepeatYellow = p.RepeatYellow; RepeatRed = p.RepeatRed; RepeatCritical = p.RepeatCritical;
            Methods = p.Methods; CustomSoundPath = p.CustomSoundPath; WindowsSoundName = p.WindowsSoundName;
        }
    }

    /// <summary>Gesamte Programmeinstellungen. Die Datei ist von außen veränderbar und wird deshalb immer mit <see cref="Sanitize"/> geprüft.</summary>
    [DataContract]
    public sealed class AppSettings
    {
        public const int MaxDevices = 500;
        public const int MinPollMinutes = 1;
        public const int MaxPollMinutes = 240;

        // Tray: nur Geräte, die sich in den letzten X Minuten gemeldet haben (Regler 5..480 in 5er-Schritten).
        public const int MinTrayActiveMinutes = 5;
        public const int MaxTrayActiveMinutes = 480;
        public const int TrayActiveStep = 5;
        public const int DefaultTrayActiveMinutes = 30;
        public const int DefaultPollMinutes = 10;

        public const int CurrentVersion = 2;

        /// <summary>Dateiformat-Version: 2 = Stufe "kritisch" und neue Standardschwellen (60/35/10).</summary>
        [DataMember(Name = "version")] public int Version { get; set; } = CurrentVersion;

        /// <summary>Sprachcode (z. B. "de", "zh-CN") oder leer = Windows-Sprache.</summary>
        [DataMember(Name = "language")] public string Language { get; set; } = string.Empty;

        /// <summary>Farbschema: leer = wie Windows, "light" oder "dark".</summary>
        [DataMember(Name = "theme")] public string Theme { get; set; } = string.Empty;

        /// <summary>Abfrage-Intervall in Minuten.</summary>
        [DataMember(Name = "pollMinutes")] public int PollMinutes { get; set; } = DefaultPollMinutes;

        /// <summary>Nur Geräte mit Kontakt innerhalb dieser Minuten fließen in die Tray-Anzeige ein.</summary>
        [DataMember(Name = "trayActiveMinutes")] public int TrayActiveMinutes { get; set; } = DefaultTrayActiveMinutes;
        [DataMember(Name = "additionalTrayMode")] public AdditionalTrayMode TrayMode { get; set; }
        [DataMember(Name = "alwaysShowAdditionalTrayIcons")] public bool AlwaysShowAdditionalTrayIcons { get; set; }
        [DataMember(Name = "defaultShowInTray")] public bool DefaultShowInTray { get; set; }

        /// <summary>Standardwerte für Geräte, die noch keine eigenen Einstellungen haben.</summary>
        [DataMember(Name = "defaultYellowAt")] public int DefaultYellowAt { get; set; } = AlertProfile.DefaultYellowAt;
        [DataMember(Name = "defaultRedAt")] public int DefaultRedAt { get; set; } = AlertProfile.DefaultRedAt;
        [DataMember(Name = "defaultCriticalAt")] public int DefaultCriticalAt { get; set; } = AlertProfile.DefaultCriticalAt;
        [DataMember(Name = "defaultNotifyYellow")] public bool DefaultNotifyYellow { get; set; }
        [DataMember(Name = "defaultNotifyRed")] public bool DefaultNotifyRed { get; set; } = true;
        [DataMember(Name = "defaultNotifyCritical")] public bool DefaultNotifyCritical { get; set; } = true;
        [DataMember(Name = "defaultNotifyOnStartup")] public bool DefaultNotifyOnStartup { get; set; } = true;
        [DataMember(Name = "defaultNotifyOnLevelChange")] public bool DefaultNotifyOnLevelChange { get; set; } = true;
        [DataMember(Name = "defaultRepeatYellow")] public int DefaultRepeatYellow { get; set; } = AlertProfile.MinRepeat;
        [DataMember(Name = "defaultRepeatRed")] public int DefaultRepeatRed { get; set; } = AlertProfile.MinRepeat;
        [DataMember(Name = "defaultRepeatCritical")] public int DefaultRepeatCritical { get; set; } = AlertProfile.MinRepeat;
        [DataMember(Name = "defaultNotifyVia")] public NotifyMethods DefaultMethods { get; set; } = AlertProfile.DefaultMethods;
        [DataMember(Name = "defaultCustomSound")] public string DefaultCustomSoundPath { get; set; } = string.Empty;
        [DataMember(Name = "defaultWindowsSound")] public string DefaultWindowsSoundName { get; set; } = string.Empty;

        [DataMember(Name = "devices")] public List<DeviceSettings> Devices { get; set; } = new List<DeviceSettings>();

        /// <summary>
        /// Der JSON-Deserialisierer ruft den Konstruktor NICHT auf, die Standardwerte oben würden also fehlen.
        /// Diese Methode läuft stattdessen vor dem Einlesen und setzt sie. Felder, die in der Datei stehen, überschreiben sie.
        /// Version = 0 bedeutet "ältere Datei ohne Versionsfeld" und löst in <see cref="Sanitize"/> die Migration aus.
        /// </summary>
        [OnDeserializing]
        private void ApplyDefaults(StreamingContext context)
        {
            Version = 0;
            Language = string.Empty;
            Theme = string.Empty;
            PollMinutes = DefaultPollMinutes;
            TrayActiveMinutes = DefaultTrayActiveMinutes;
            DefaultYellowAt = AlertProfile.DefaultYellowAt;
            DefaultRedAt = AlertProfile.DefaultRedAt;
            DefaultCriticalAt = AlertProfile.DefaultCriticalAt;
            DefaultNotifyYellow = false;
            DefaultNotifyRed = true;
            DefaultNotifyCritical = true;
            DefaultNotifyOnStartup = true;
            DefaultNotifyOnLevelChange = true;
            DefaultRepeatYellow = DefaultRepeatRed = DefaultRepeatCritical = AlertProfile.MinRepeat;
            DefaultMethods = AlertProfile.DefaultMethods;
            DefaultCustomSoundPath = string.Empty;
            DefaultWindowsSoundName = string.Empty;
            Devices = new List<DeviceSettings>();
        }

        /// <summary>Der aktuelle globale Standard als Profil.</summary>
        public AlertProfile GetDefaultProfile() => new AlertProfile
        {
            ShowInTray = DefaultShowInTray,
            YellowAt = DefaultYellowAt, RedAt = DefaultRedAt, CriticalAt = DefaultCriticalAt,
            NotifyYellow = DefaultNotifyYellow, NotifyRed = DefaultNotifyRed, NotifyCritical = DefaultNotifyCritical,
            NotifyOnStartup = DefaultNotifyOnStartup, NotifyOnLevelChange = DefaultNotifyOnLevelChange,
            RepeatYellow = DefaultRepeatYellow, RepeatRed = DefaultRepeatRed, RepeatCritical = DefaultRepeatCritical,
            Methods = DefaultMethods, CustomSoundPath = DefaultCustomSoundPath, WindowsSoundName = DefaultWindowsSoundName
        };

        public void SetDefaultProfile(AlertProfile p)
        {
            DefaultShowInTray = p.ShowInTray;
            DefaultYellowAt = p.YellowAt; DefaultRedAt = p.RedAt; DefaultCriticalAt = p.CriticalAt;
            DefaultNotifyYellow = p.NotifyYellow; DefaultNotifyRed = p.NotifyRed; DefaultNotifyCritical = p.NotifyCritical;
            DefaultNotifyOnStartup = p.NotifyOnStartup; DefaultNotifyOnLevelChange = p.NotifyOnLevelChange;
            DefaultRepeatYellow = p.RepeatYellow; DefaultRepeatRed = p.RepeatRed; DefaultRepeatCritical = p.RepeatCritical;
            DefaultMethods = p.Methods; DefaultCustomSoundPath = p.CustomSoundPath; DefaultWindowsSoundName = p.WindowsSoundName;
        }

        /// <summary>Setzt alle Geräte auf den aktuellen Standard zurück (Name, Ignorieren und Kategorie bleiben).</summary>
        public void ResetAllDevicesToDefault()
        {
            var profile = GetDefaultProfile();
            foreach (var d in Devices)
                d.ApplyProfile(profile);
        }

        /// <summary>Tiefe Kopie (z. B. damit das Einstellungsfenster ändern kann, ohne dass "Schließen" etwas verändert).</summary>
        public AppSettings Clone()
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(AppSettings));
            using (var ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, this);
                ms.Position = 0;
                var copy = (AppSettings)serializer.ReadObject(ms);
                copy.Sanitize();
                return copy;
            }
        }

        /// <summary>Liefert die Einstellungen eines Geräts oder eine Kopie der Standardwerte (ohne sie zu speichern).</summary>
        public DeviceSettings GetDevice(string id)
        {
            var found = Devices?.FirstOrDefault(d => d.Id == id);
            if (found != null)
                return found;

            var created = new DeviceSettings { Id = id };
            created.ApplyProfile(GetDefaultProfile());
            return created;
        }

        /// <summary>Legt Einstellungen für ein Gerät an (falls noch nicht vorhanden) und gibt sie zum Bearbeiten zurück.</summary>
        public DeviceSettings GetOrAddDevice(string id, string name)
        {
            var existing = Devices.FirstOrDefault(d => d.Id == id);
            if (existing != null)
            {
                existing.Name = name;
                return existing;
            }

            var created = GetDevice(id);
            created.Name = name;
            if (Devices.Count < MaxDevices)
                Devices.Add(created);
            return created;
        }

        /// <summary>
        /// Macht beliebige (auch manipulierte) Werte gültig: Bereiche begrenzen, Texte kürzen,
        /// Doppelte und Ungültiges entfernen. Nach dem Aufruf ist das Objekt sicher verwendbar.
        /// </summary>
        public void Sanitize()
        {
            if (!Enum.IsDefined(typeof(AdditionalTrayMode), TrayMode))
                TrayMode = AdditionalTrayMode.None;
            Language = IsValidLanguage(Language) ? Language : string.Empty;
            Theme = Theme == "light" || Theme == "dark" ? Theme : string.Empty;
            PollMinutes = Clamp(PollMinutes, MinPollMinutes, MaxPollMinutes);
            // Auf 5er-Schritte runden, damit manipulierte Werte (z. B. 7) zum Regler passen.
            TrayActiveMinutes = Clamp((int)Math.Round(TrayActiveMinutes / (double)TrayActiveStep) * TrayActiveStep,
                MinTrayActiveMinutes, MaxTrayActiveMinutes);

            // Migration älterer Dateien: Wer noch die alten Standardwerte 40/20 hatte (also nie etwas verstellt hat),
            // bekommt die neuen Standardwerte; selbst gewählte Werte bleiben unverändert.
            var migrate = Version < CurrentVersion;
            if (migrate && IsLegacyDefault(DefaultYellowAt, DefaultRedAt))
            {
                DefaultYellowAt = AlertProfile.DefaultYellowAt;
                DefaultRedAt = AlertProfile.DefaultRedAt;
                DefaultCriticalAt = AlertProfile.DefaultCriticalAt;
            }

            var defaults = FixThresholds(DefaultYellowAt, DefaultRedAt, DefaultCriticalAt,
                AlertProfile.DefaultYellowAt, AlertProfile.DefaultRedAt, AlertProfile.DefaultCriticalAt);
            DefaultYellowAt = defaults.Item1;
            DefaultRedAt = defaults.Item2;
            DefaultCriticalAt = defaults.Item3;
            DefaultCustomSoundPath = SanitizeSoundPath(DefaultCustomSoundPath);
            DefaultWindowsSoundName = SanitizeWindowsSoundName(DefaultWindowsSoundName);
            DefaultMethods = NormalizeMethods(DefaultMethods, DefaultWindowsSoundName, DefaultCustomSoundPath);
            DefaultRepeatYellow = AlertProfile.ClampRepeat(DefaultRepeatYellow);
            DefaultRepeatRed = AlertProfile.ClampRepeat(DefaultRepeatRed);
            DefaultRepeatCritical = AlertProfile.ClampRepeat(DefaultRepeatCritical);

            var seen = new HashSet<string>();
            var clean = new List<DeviceSettings>();
            foreach (var d in Devices ?? new List<DeviceSettings>())
            {
                if (d == null || !seen.Add(d.Id ?? string.Empty))
                    continue;

                d.Id = BatteryDevice.Sanitize(d.Id, BatteryDevice.MaxIdLength);
                if (d.Id.Length == 0)
                    continue;

                d.Name = BatteryDevice.Sanitize(d.Name, BatteryDevice.MaxNameLength);
                if (d.CategoryOverride.HasValue && !System.Enum.IsDefined(typeof(DeviceCategory), d.CategoryOverride.Value))
                    d.CategoryOverride = null;

                if (migrate && IsLegacyDefault(d.YellowAt, d.RedAt))
                {
                    d.YellowAt = AlertProfile.DefaultYellowAt;
                    d.RedAt = AlertProfile.DefaultRedAt;
                    d.CriticalAt = AlertProfile.DefaultCriticalAt;
                }

                var t = FixThresholds(d.YellowAt, d.RedAt, d.CriticalAt, DefaultYellowAt, DefaultRedAt, DefaultCriticalAt);
                d.YellowAt = t.Item1;
                d.RedAt = t.Item2;
                d.CriticalAt = t.Item3;
                d.CustomSoundPath = SanitizeSoundPath(d.CustomSoundPath);
                d.WindowsSoundName = SanitizeWindowsSoundName(d.WindowsSoundName);
                d.Methods = NormalizeMethods(d.Methods, d.WindowsSoundName, d.CustomSoundPath);
                d.RepeatYellow = AlertProfile.ClampRepeat(d.RepeatYellow);
                d.RepeatRed = AlertProfile.ClampRepeat(d.RepeatRed);
                d.RepeatCritical = AlertProfile.ClampRepeat(d.RepeatCritical);

                clean.Add(d);
                if (clean.Count >= MaxDevices)
                    break;
            }

            Devices = clean;
            Version = CurrentVersion;
        }

        private static bool IsLegacyDefault(int yellow, int red) => (yellow == 40 && red == 20) || (yellow <= 0 && red <= 0);

        /// <summary>
        /// Begrenzt Schwellen auf sinnvolle Werte. Fehlt ein Wert (0 = Feld war nicht in der Datei), werden die Fallbacks genutzt.
        /// Es gilt immer: kritisch &lt; rot &lt; gelb &lt;= 100, sonst gäbe es keine eigenen Bereiche je Stufe.
        /// </summary>
        private static Tuple<int, int, int> FixThresholds(int yellow, int red, int critical, int fbYellow, int fbRed, int fbCritical)
        {
            if (yellow <= 0)
            {
                yellow = fbYellow;
                red = fbRed;
                critical = fbCritical;
            }

            yellow = Clamp(yellow, 3, 100);
            red = Clamp(red <= 0 ? fbRed : red, 2, yellow - 1);
            // Älteres Format ohne "kritisch": höchstens der Standard, aber immer unter Rot.
            critical = Clamp(critical <= 0 ? fbCritical : critical, 1, red - 1);
            return Tuple.Create(yellow, red, critical);
        }

        /// <summary>
        /// Prüft den Pfad einer eigenen Tondatei: nur lokaler, absoluter Pfad (kein Netzwerkpfad \\server\..., damit Windows
        /// keine Anmeldedaten an fremde Rechner sendet), nur .wav/.mp3, begrenzte Länge, keine Steuerzeichen. Sonst leer.
        /// </summary>
        public static string SanitizeSoundPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            path = path.Trim();
            if (path.Length > 260 || path.Any(char.IsControl) || path.StartsWith(@"\\", StringComparison.Ordinal))
                return string.Empty;

            try
            {
                if (!System.IO.Path.IsPathRooted(path) || path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    return string.Empty;

                var ext = System.IO.Path.GetExtension(path);
                return string.Equals(ext, ".wav", StringComparison.OrdinalIgnoreCase) || string.Equals(ext, ".mp3", StringComparison.OrdinalIgnoreCase)
                    ? path
                    : string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Prüft den Dateinamen eines Windows-Systemtons: nur ein reiner Name auf ".wav" (keine Ordner, kein "..", keine Steuer-
        /// oder Sonderzeichen), damit er nie aus dem Ordner Windows\Media herausführen kann. Ungültig = leer.
        /// </summary>
        public static string SanitizeWindowsSoundName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            name = name.Trim();
            if (name.Length > 100 || name.Any(char.IsControl) || name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0
                || name.Contains("..") || !name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return name;
        }

        /// <summary>
        /// Macht die Benachrichtigungsarten konsistent: unbekannte Bits weg, eine Tonart ohne gültige Datei/Auswahl wird entfernt
        /// (z. B. "eigener Ton" ohne Pfad), und es bleibt höchstens EINE Tonart übrig (Vorrang: eigene Datei, Windows-Ton, Standardton).
        /// Die Windows-Meldung bleibt unberührt.
        /// </summary>
        public static NotifyMethods NormalizeMethods(NotifyMethods methods, string windowsSoundName, string customPath)
        {
            methods &= NotifyMethods.All;
            if (string.IsNullOrEmpty(windowsSoundName))
                methods &= ~NotifyMethods.SoundWindows;
            if (string.IsNullOrEmpty(customPath))
                methods &= ~NotifyMethods.SoundCustom;

            var keep = (methods & NotifyMethods.SoundCustom) != 0 ? NotifyMethods.SoundCustom
                : (methods & NotifyMethods.SoundWindows) != 0 ? NotifyMethods.SoundWindows
                : (methods & NotifyMethods.SoundDefault) != 0 ? NotifyMethods.SoundDefault
                : NotifyMethods.None;
            return (methods & ~AlertProfile.SoundMask) | keep;
        }

        /// <summary>Whitelist-Prüfung für Sprachcodes wie "de" oder "zh-Hans-CN" (verhindert Müll in der Datei).</summary>
        public static bool IsValidLanguage(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length > 20)
                return string.IsNullOrEmpty(code);

            foreach (var c in code)
            {
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '-')
                    return false;
            }

            return code[0] != '-' && code[code.Length - 1] != '-';
        }
    }
}
