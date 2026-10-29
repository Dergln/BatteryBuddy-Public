using System;
using System.IO;
using System.Linq;
using System.Windows;
using BatteryBuddy.App.Services;
using BatteryBuddy.App.Views;

namespace BatteryBuddy.App
{
    /// <summary>Start und Ende der Anwendung: einmalige Instanz, Tray, Überwachung, Konfigurationsfenster.</summary>
    public partial class AppMain : Application
    {
        private SingleInstance _single;
        private AppHost _host;
        private TrayController _tray;
        private DeviceChangeWatcher _watcher;
        private SettingsWindow _window;

        private static string DataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BatteryBuddy");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var background = e.Args.Any(a => string.Equals(a, "--background", StringComparison.OrdinalIgnoreCase));

            _single = new SingleInstance();
            if (!_single.IsFirst)
            {
                // Läuft schon: laufende Instanz bitten, das Fenster zu zeigen (außer beim Autostart).
                if (!background)
                    _single.SignalFirstInstance();
                Shutdown();
                return;
            }

            // Ein einzelner Fehler soll das Tray-Programm nicht beenden; er wird nur ins Protokoll geschrieben.
            DispatcherUnhandledException += (s, ex) => { LogError(ex.Exception); ex.Handled = true; };

            _host = new AppHost(Path.Combine(DataFolder, "settings.json"));
            Services.Loc.Init(_host.Settings.Language);
            ThemeService.Apply(_host.Settings.Theme);

            _tray = new TrayController(_host, ShowSettings, ExitApp);
            _watcher = new DeviceChangeWatcher();
            _watcher.DevicesChanged += (s, ev) => _host.RequestRefreshSoon();
            _single.ListenForSecondStart(() => Dispatcher.BeginInvoke(new Action(ShowSettings)));
            // Beim Abmelden/Herunterfahren darf die Rückfrage "Änderungen speichern?" das Beenden nicht blockieren.
            SessionEnding += (s, ev) => _window?.SkipClosePrompt();

            if (_host.IsFirstRun)
            {
                // Erststart: Startmenü-Eintrag und Autostart beim Anmelden sind standardmäßig an (im Einstellungsfenster abschaltbar).
                StartMenuService.SetEnabled(true);
                AutostartService.SetEnabled(true);
            }

            _host.Start();
            _ = _host.InitialRefreshAsync();

            if (!background || _host.IsFirstRun)
                ShowSettings();
        }

        private void ShowSettings()
        {
            if (_window == null)
            {
                _window = new SettingsWindow(_host, () => _tray.BuildMenu());
                _window.Closed += (s, e) => _window = null;
                _window.Show();
            }

            if (_window.WindowState == WindowState.Minimized)
                _window.WindowState = WindowState.Normal;
            _window.Activate();
        }

        private void ExitApp()
        {
            _window?.Close();
            if (_window != null)
                return; // Nutzer hat in der Rückfrage "Abbrechen" gewählt: App läuft weiter.
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _watcher?.Dispose();
            _tray?.Dispose();
            _host?.Dispose();
            _single?.Dispose();
            base.OnExit(e);
        }

        /// <summary>Schreibt nur Fehlertyp und -text (keine Gerätedaten) in eine kleine Protokolldatei.</summary>
        private static void LogError(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(DataFolder);
                var file = Path.Combine(DataFolder, "error.log");
                if (File.Exists(file) && new FileInfo(file).Length > 200 * 1024)
                    File.Delete(file);
                var detail = ex.GetType().Name + ": " + ex.Message;
                for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
                    detail += " -> " + inner.GetType().Name + ": " + inner.Message; // Die eigentliche Ursache steckt oft in der inneren Ausnahme.
                File.AppendAllText(file, DateTime.Now.ToString("s") + " " + detail + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
            catch (Exception)
            {
                // Protokollieren ist nur ein Hilfsmittel; scheitert es, wird nichts weiter unternommen.
            }
        }
    }
}
