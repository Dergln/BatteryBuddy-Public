using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace BatteryBuddy.App.Services
{
    /// <summary>Autostart beim Windows-Login über den Registry-Schlüssel des aktuellen Benutzers (keine Adminrechte nötig).</summary>
    public static class AutostartService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "BatteryBuddy";

        /// <summary>Ist ein Eintrag vorhanden, der auf DIESE EXE zeigt?</summary>
        public static bool IsEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunKey))
                    return (key?.GetValue(ValueName) as string)?.IndexOf(ExePath(), StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null)
                        return false;
                    if (enabled)
                        // Pfad in Anführungszeichen: sonst würden Leerzeichen im Pfad den Start verhindern (oder missbraucht).
                        key.SetValue(ValueName, "\"" + ExePath() + "\" --background");
                    else
                        key.DeleteValue(ValueName, false);
                }

                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException || ex is IOException)
            {
                return false;
            }
        }

        public static string ExePath() => Process.GetCurrentProcess().MainModule.FileName;
    }
}
