using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using BatteryBuddy.Core.Models;
using Microsoft.Win32;
using System.ComponentModel;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Hell-/Dunkel-Darstellung. Die Farben liegen als Pinsel in den Anwendungs-Ressourcen (Schlüssel "BB.*");
    /// die Vorlagen in Themes\Controls.xaml verweisen per DynamicResource darauf, ein Wechsel wirkt also sofort.
    /// </summary>
    public static class ThemeService
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static bool IsDark { get; private set; }
        public static bool IsHighContrast { get; private set; }
        public static event EventHandler Changed;
        private static string _theme;

        static ThemeService()
        {
            SystemParameters.StaticPropertyChanged += OnSystemSettingChanged;
        }

        private static void OnSystemSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SystemParameters.HighContrast))
                return;
            var app = Application.Current;
            if (app == null || app.Dispatcher.HasShutdownStarted)
                return;
            app.Dispatcher.InvokeAsync(() => Apply(_theme));
        }

        /// <summary>Wendet "light", "dark" oder "" (= wie Windows) an.</summary>
        public static void Apply(string theme)
        {
            _theme = theme;
            IsHighContrast = SystemParameters.HighContrast;
            IsDark = theme == "dark" || (theme != "light" && !WindowsAppsUseLightTheme());
            var res = Application.Current.Resources;

            Set(res, "BB.Window", IsDark ? "#202020" : "#F3F3F3");
            Set(res, "BB.Panel", IsDark ? "#2B2B2B" : "#FFFFFF");
            Set(res, "BB.Text", IsDark ? "#F2F2F2" : "#1F1F1F");
            Set(res, "BB.Muted", IsDark ? "#A8A8A8" : "#666666");
            Set(res, "BB.Control", IsDark ? "#3A3A3A" : "#FBFBFB");
            Set(res, "BB.ControlHover", IsDark ? "#4A4A4A" : "#E5F1FB");
            Set(res, "BB.ControlPressed", IsDark ? "#585858" : "#CCE4F7");
            Set(res, "BB.Border", IsDark ? "#939393" : "#777777");
            Set(res, "BB.Accent", IsDark ? "#4CC2FF" : "#0067C0");
            Set(res, "BB.Selected", IsDark ? "#283C4C" : "#CCE4F7");
            Set(res, "BB.Error", IsDark ? "#FF6B6B" : "#B00020");
            Set(res, "BB.Ok", IsDark ? "#6CE06C" : "#107C10");
            if (IsHighContrast)
                ApplyHighContrastPalette(res);

            foreach (Window w in Application.Current.Windows)
                StyleTitleBar(w);
            Changed?.Invoke(null, EventArgs.Empty);
        }

        internal static void ApplyHighContrastPalette(ResourceDictionary res)
        {
            foreach (var key in new[] { "BB.Window", "BB.Panel", "BB.Control", "BB.ControlHover", "BB.ControlPressed", "BB.Selected" })
                res[key] = SystemColors.WindowBrush;
            foreach (var key in new[] { "BB.Text", "BB.Muted", "BB.Border", "BB.Error", "BB.Ok", "BB.Accent" })
                res[key] = SystemColors.WindowTextBrush;
        }

        private static void Set(ResourceDictionary res, string key, string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            res[key] = brush;
        }

        /// <summary>Dunkle Titelleiste (Windows 10 ab 1809 / Windows 11). Auf älteren Versionen passiert einfach nichts.</summary>
        public static void StyleTitleBar(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                window.SourceInitialized += (s, e) => StyleTitleBar(window);
                return;
            }

            var on = IsDark ? 1 : 0;
            try
            {
                if (DwmSetWindowAttribute(handle, 20, ref on, sizeof(int)) != 0)
                    DwmSetWindowAttribute(handle, 19, ref on, sizeof(int)); // Attributnummer vor Windows 10 20H1
            }
            catch (Exception)
            {
                // Rein optisch: ohne dunkle Titelleiste funktioniert alles weiter.
            }
        }

        private static bool WindowsAppsUseLightTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    return !(key?.GetValue("AppsUseLightTheme") is int v) || v != 0;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }

    /// <summary>Farben der Ladestufen für die Fenster-Oberfläche (das Tray-Symbol hat eigene Farben, siehe TrayIconRenderer).</summary>
    public static class LevelColors
    {
        // Feste Farben für die kleinen Farbkästchen: in beiden Themes gleich, damit "burgunderrot" immer erkennbar ist.
        public static readonly Color BoxGreen = Color.FromRgb(16, 124, 16);
        public static readonly Color BoxYellow = Color.FromRgb(224, 160, 0);
        public static readonly Color BoxRed = Color.FromRgb(196, 43, 28);
        public static readonly Color BoxCritical = Color.FromRgb(128, 0, 32);

        /// <summary>Textfarbe einer Stufe, passend zum aktuellen Theme (im dunklen Theme heller, damit sie lesbar bleibt).</summary>
        public static Color Text(ChargeLevel level)
        {
            if (ThemeService.IsHighContrast)
                return SystemColors.WindowTextColor;
            switch (level)
            {
                case ChargeLevel.Green: return ThemeService.IsDark ? Color.FromRgb(108, 224, 108) : Color.FromRgb(9, 101, 9);
                case ChargeLevel.Yellow: return ThemeService.IsDark ? Color.FromRgb(255, 209, 64) : Color.FromRgb(140, 85, 0);
                case ChargeLevel.Red: return ThemeService.IsDark ? Color.FromRgb(255, 156, 144) : Color.FromRgb(160, 32, 16);
                case ChargeLevel.Critical: return ThemeService.IsDark ? Color.FromRgb(255, 140, 170) : Color.FromRgb(128, 0, 32);
                default: return ThemeService.IsDark ? Color.FromRgb(170, 170, 170) : Color.FromRgb(90, 90, 90);
            }
        }
    }
}
