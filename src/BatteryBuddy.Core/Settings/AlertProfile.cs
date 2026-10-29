using System;

namespace BatteryBuddy.Core.Settings
{
    /// <summary>Wie eine Warnung gemeldet wird. Mehrere Arten lassen sich kombinieren ([Flags] = Bit-Felder).</summary>
    [Flags]
    public enum NotifyMethods
    {
        None = 0,
        /// <summary>Windows-Benachrichtigung (Toast).</summary>
        WindowsToast = 1,
        /// <summary>Eigener Standardton von BatteryBuddy.</summary>
        SoundDefault = 2,
        /// <summary>Ein vom Nutzer gewählter Windows-Systemton (aus dem Ordner Windows\Media).</summary>
        SoundWindows = 4,
        /// <summary>Eine vom Nutzer gewählte WAV- oder MP3-Datei.</summary>
        SoundCustom = 8,
        All = WindowsToast | SoundDefault | SoundWindows | SoundCustom
    }

    /// <summary>
    /// Alle Warn-Einstellungen, die ein Gerät hat und die als globaler Standard vorgegeben werden können.
    /// Das Einstellungsfenster bearbeitet Geräte und Standard über diese gemeinsame Klasse.
    /// Schwellen gelten als "bis einschließlich": Wert &lt;= CriticalAt → kritisch, &lt;= RedAt → rot, &lt;= YellowAt → gelb, sonst grün.
    /// </summary>
    public sealed class AlertProfile
    {
        public const int DefaultYellowAt = 60;
        public const int DefaultRedAt = 35;
        public const int DefaultCriticalAt = 10;
        public const NotifyMethods DefaultMethods = NotifyMethods.WindowsToast | NotifyMethods.SoundDefault;

        /// <summary>Die drei Tonarten schließen sich gegenseitig aus (es klingt immer nur ein Ton); die Windows-Meldung ist unabhängig davon.</summary>
        public const NotifyMethods SoundMask = NotifyMethods.SoundDefault | NotifyMethods.SoundWindows | NotifyMethods.SoundCustom;

        public int YellowAt { get; set; } = DefaultYellowAt;
        public int RedAt { get; set; } = DefaultRedAt;
        public int CriticalAt { get; set; } = DefaultCriticalAt;
        public bool ShowInTray { get; set; }
        public bool NotifyYellow { get; set; }
        public bool NotifyRed { get; set; } = true;
        public bool NotifyCritical { get; set; } = true;
        /// <summary>Beim Programmstart einmal alle Geräte melden, die sich gerade in einer gewählten Warnstufe befinden.</summary>
        public bool NotifyOnStartup { get; set; } = true;

        /// <summary>Melden, sobald ein Gerät eine schlechtere Warnstufe erreicht (Hintergrundprüfung, auch über Neustarts hinweg).</summary>
        public bool NotifyOnLevelChange { get; set; } = true;

        /// <summary>Gültiger Bereich für "Benachrichtigen für [x] Prüfintervalle" (1 = nur beim ersten Erkennen).</summary>
        public const int MinRepeat = 1;
        public const int MaxRepeat = 10;

        /// <summary>
        /// Wie oft je Warnstufe insgesamt gemeldet wird: einmal beim Erkennen, danach je reguläre Intervall-Prüfung erneut,
        /// bis die Anzahl erreicht ist (oder der Nutzer die Warnung bestätigt).
        /// </summary>
        public int RepeatYellow { get; set; } = MinRepeat;
        public int RepeatRed { get; set; } = MinRepeat;
        public int RepeatCritical { get; set; } = MinRepeat;

        /// <summary>Begrenzt einen (evtl. manipulierten) Wert auf <see cref="MinRepeat"/>..<see cref="MaxRepeat"/>.</summary>
        public static int ClampRepeat(int value) => value < MinRepeat ? MinRepeat : value > MaxRepeat ? MaxRepeat : value;

        public NotifyMethods Methods { get; set; } = DefaultMethods;
        public string CustomSoundPath { get; set; } = string.Empty;

        /// <summary>Dateiname eines Windows-Systemtons (Ordner Windows\Media), z. B. "Windows Notify.wav".</summary>
        public string WindowsSoundName { get; set; } = string.Empty;

        public AlertProfile Copy() => (AlertProfile)MemberwiseClone();
    }
}
