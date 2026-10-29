using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Providers;
using BatteryBuddy.Core.Settings;
using BatteryBuddy.Providers;
using BatteryBuddy.Providers.Bluetooth;
using BatteryBuddy.Providers.Logitech;

namespace BatteryBuddy.App.Services
{
    /// <summary>
    /// Kern der Anwendung: hält Einstellungen, fragt regelmäßig alle Provider ab und meldet Ergebnisse und Warnungen.
    /// Alles läuft auf dem UI-Thread (DispatcherTimer); nur die Provider-Abfragen selbst laufen im Hintergrund.
    /// </summary>
    public sealed class AppHost : IDisposable
    {
        // Frühestens so oft darf eine durch Geräteänderungen ausgelöste Abfrage laufen (verhindert Schleifen).
        private static readonly TimeSpan MinEventRefreshGap = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan StartupFollowUpDelay = TimeSpan.FromSeconds(5);
        private readonly Dispatcher _ui = Dispatcher.CurrentDispatcher;

        private readonly SettingsStore _store;
        private readonly AlertStateStore _stateStore;
        private readonly BatteryMonitor _monitor;
        private readonly NotificationDecider _decider;
        private readonly DispatcherTimer _pollTimer = new DispatcherTimer();
        private readonly DispatcherTimer _eventTimer = new DispatcherTimer();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private DateTimeOffset _lastRefreshDone = DateTimeOffset.MinValue;
        private int _followUpsLeft;

        public AppSettings Settings { get; private set; }
        public MonitorSnapshot Latest { get; private set; }
        public bool IsFirstRun { get; }

        public event EventHandler SnapshotUpdated;
        public event EventHandler<IReadOnlyList<BatteryAlert>> AlertsRaised;
        public event Action<AlertProfile, DeviceCategory?> TestRequested;
        public event Func<AlertProfile, ChargeLevel, int, Task> SimulationRequested;

        /// <summary>"Testen"-Knopf im Einstellungsfenster: Das Tray löst die im Profil gewählten Benachrichtigungen einmal aus.</summary>
        public void RequestTest(AlertProfile profile, DeviceCategory? category = null)
            => TestRequested?.Invoke(profile, category);

        /// <summary>Fordert eine simulierte Warnung an; der Aufrufer kann auf das Ende von Toast und Ton warten.</summary>
        public Task RequestSimulation(AlertProfile profile, ChargeLevel level, int percent)
            => SimulationRequested?.Invoke(profile, level, percent) ?? Task.CompletedTask;

        public AppHost(string settingsPath)
        {
            IsFirstRun = !System.IO.File.Exists(settingsPath);
            _store = new SettingsStore(settingsPath);
            Settings = _store.Load();

            // Gemerkte Warnstufen liegen neben settings.json (siehe AlertStateStore).
            var folder = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(settingsPath));
            _stateStore = new AlertStateStore(System.IO.Path.Combine(folder, "alert-state.json"));
            _decider = new NotificationDecider(_stateStore.Load());

            IBatteryProvider[] providers =
            {
                new SystemBatteryProvider(),
                new BluetoothBatteryProvider(),
                new XInputBatteryProvider(),
                new LogitechBatteryProvider()
            };
            _monitor = new BatteryMonitor(providers, TimeSpan.FromSeconds(15));

            // Nur beim allerersten Durchlauf zeigen wir Zwischenstände, und nur mit mindestens einem Gerät:
            // Ein Provider ohne Treffer (z. B. kein interner Akku am PC) soll nicht schon "nichts gefunden" auslösen.
            _monitor.PartialSnapshot += (s, snap) => _ui.BeginInvoke(new Action(() =>
            {
                if (Latest != null || snap.Devices.Count == 0) return;
                Latest = snap;
                SnapshotUpdated?.Invoke(this, EventArgs.Empty);
            }));
            _pollTimer.Tick += async (s, e) => await RefreshAsync(isIntervalCheck: true);
            _eventTimer.Tick += async (s, e) => await OnEventTimerAsync();
            ApplyInterval();
        }

        public void Start() => _pollTimer.Start();

        /// <summary>
        /// Erste Abfrage beim Programmstart plus ein zweiter Durchlauf kurz danach: Schlafende Geräte (z. B. Funkmaus)
        /// und Bluetooth-Werte melden sich oft erst nach einigen Sekunden.
        /// Erst danach – wenn alle Provider des zweiten Durchlaufs fertig sind – folgt der Auslöser "beim Programmstart",
        /// damit langsame Quellen (z. B. Logitech über den Empfänger) schon Werte geliefert haben.
        /// </summary>
        public async Task InitialRefreshAsync()
        {
            await RefreshAsync();
            try { await Task.Delay(StartupFollowUpDelay, _cts.Token); }
            catch (OperationCanceledException) { return; }
            await RefreshAsync();

            if (_cts.IsCancellationRequested || Latest == null)
                return;
            var alerts = _decider.EvaluateStartup(Latest.Devices, Settings, DateTimeOffset.UtcNow);
            if (alerts.Count > 0)
                AlertsRaised?.Invoke(this, alerts);
        }

        /// <summary>Übernimmt geänderte Einstellungen, speichert sie und wertet den letzten Stand neu aus.</summary>
        public bool Apply(AppSettings updated)
        {
            updated.Sanitize();
            Settings = updated;
            ApplyInterval();
            var saved = true;
            try
            {
                _store.Save(updated);
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                saved = false;
            }

            SnapshotUpdated?.Invoke(this, EventArgs.Empty);
            return saved;
        }

        /// <summary>Gerätelisten ändern sich (Verbinden/Trennen): kurz warten, bis Windows fertig ist, dann abfragen.</summary>
        public void RequestRefreshSoon()
        {
            _followUpsLeft = 1;
            _eventTimer.Interval = TimeSpan.FromSeconds(4);
            _eventTimer.Stop();
            _eventTimer.Start();
        }

        /// <param name="isIntervalCheck">Nur der Intervall-Timer setzt true: Nur diese Prüfungen zählen für Wiederholungen.</param>
        public async Task RefreshAsync(bool isIntervalCheck = false)
        {
            try
            {
                var snapshot = await _monitor.RefreshAsync(_cts.Token);
                Latest = snapshot;
                _lastRefreshDone = DateTimeOffset.UtcNow;
                RememberDevices(snapshot);
                SnapshotUpdated?.Invoke(this, EventArgs.Empty);

                var alerts = _decider.Evaluate(snapshot.Devices, Settings, DateTimeOffset.UtcNow, isIntervalCheck);
                SaveAlertState();
                if (alerts.Count > 0)
                    AlertsRaised?.Invoke(this, alerts);
            }
            catch (OperationCanceledException)
            {
                // Programm wird beendet.
            }
        }

        /// <summary>Gibt es Warnungen, die noch wiederholt würden? (Tray-Menüpunkt "Alle Warnungen bestätigen" nur dann aktiv.)</summary>
        public bool HasOpenAlerts => _decider.HasOpenRepeats(Settings);

        /// <summary>"Gesehen" für ein Gerät (Knopf in der Windows-Meldung). Nur auf dem UI-Thread aufrufen.</summary>
        public bool Acknowledge(string deviceId) => _decider.Acknowledge(deviceId);

        /// <summary>"Alle Warnungen bestätigen" (Tray-Menü). Nur auf dem UI-Thread aufrufen.</summary>
        public int AcknowledgeAll() => _decider.AcknowledgeAll();

        /// <summary>Speichert gemerkte Warnstufen nur bei Änderungen (selten), Fehler sind unkritisch.</summary>
        private void SaveAlertState()
        {
            if (!_decider.HasUnsavedState)
                return;
            try
            {
                _stateStore.Save(_decider.RememberedLevels);
                _decider.MarkStateSaved();
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException) { }
        }

        private async Task OnEventTimerAsync()
        {
            _eventTimer.Stop();
            if (DateTimeOffset.UtcNow - _lastRefreshDone >= MinEventRefreshGap)
                await RefreshAsync();

            // Ein zweiter Durchlauf etwas später: Akkuwerte von Bluetooth-Geräten erscheinen oft erst nach einigen Sekunden.
            if (_followUpsLeft-- > 0)
            {
                _eventTimer.Interval = TimeSpan.FromSeconds(20);
                _eventTimer.Start();
            }
        }

        /// <summary>Neue Geräte mit Namen in den Einstellungen vormerken, damit die Datei lesbar bleibt.</summary>
        private void RememberDevices(MonitorSnapshot snapshot)
        {
            var changed = false;
            foreach (var d in snapshot.Devices)
            {
                var known = Settings.Devices.FirstOrDefault(x => x.Id == d.Id);
                if (known == null || known.Name != d.Name)
                {
                    Settings.GetOrAddDevice(d.Id, d.Name);
                    changed = true;
                }
            }

            if (changed)
            {
                try { _store.Save(Settings); }
                catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException) { }
            }
        }

        private void ApplyInterval() => _pollTimer.Interval = TimeSpan.FromMinutes(Settings.PollMinutes);

        public void Dispose()
        {
            _pollTimer.Stop();
            _eventTimer.Stop();
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    /// <summary>Anzeigenamen: Der Name des internen Akkus kommt aus den Sprachdateien.</summary>
    public static class DeviceNames
    {
        public static string Display(BatteryDevice d)
            => d.Id == SystemBatteryProvider.DeviceId ? Loc.Get("device.internalBattery") : d.Name;
    }
}
