using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;
using Microsoft.Win32;

namespace BatteryBuddy.App.Services
{
    /// <summary>Das Symbol im Infobereich der Taskleiste samt Kontextmenü und Benachrichtigungen.</summary>
    public sealed class TrayController : IDisposable
    {
        private const int MaxTooltip = 127;   // Grenze von NotifyIcon.Text
        private const int MaxBalloonTitle = 63;
        private const int MaxBalloonText = 255;
        private const int BalloonTipTimeoutMilliseconds = 10000;
        private static readonly TimeSpan SimulationToastFallback = TimeSpan.FromSeconds(12);

        private readonly AppHost _host;
        private readonly Action _openSettings;
        private readonly Action _exit;
        private readonly NativeTrayIcon _notifyIcon = new NativeTrayIcon("main");
        private readonly ContextMenuStrip _menu = new ContextMenuStrip();
        private readonly SemaphoreSlim _notificationGate = new SemaphoreSlim(1, 1);
        private readonly ToastService _toasts;
        private readonly AdditionalTrayIcons _additional;
        private readonly SynchronizationContext _ui;
        private bool _disposed;
        private Icon _currentIcon;
        private ToolStripMenuItem _ackAll;

        public TrayController(AppHost host, Action openSettings, Action exit)
        {
            _host = host;
            _openSettings = openSettings;
            _exit = exit;

            // Klicks auf "Gesehen" kommen von Windows auf einem Hintergrund-Thread; der Kontext bringt sie zurück auf den UI-Thread.
            _ui = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
            _additional = new AdditionalTrayIcons(_menu, _openSettings);
            _toasts = new ToastService(_ui, ids => { foreach (var id in ids) _host.Acknowledge(id); });
            if (!_toasts.TryInitialize())
                System.Diagnostics.Trace.TraceWarning("Windows toasts unavailable; falling back to tray balloon tips.");

            _notifyIcon.ContextMenuStrip = _menu;
            _menu.Opening += (s, e) => { if (_ackAll != null) _ackAll.Enabled = _host.HasOpenAlerts; };
            _notifyIcon.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) _openSettings(); };
            _host.SnapshotUpdated += OnSnapshot;
            _host.AlertsRaised += OnAlerts;
            _host.TestRequested += ShowTest;
            _host.SimulationRequested += ShowSimulation;
            SystemEvents.UserPreferenceChanged += OnThemeChanged;

            BuildMenu();
            Update();
            _notifyIcon.Visible = true;
        }

        /// <summary>Menütexte neu setzen (nach Sprachwechsel).</summary>
        public void BuildMenu()
        {
            _menu.Items.Clear();
            _menu.RightToLeft = Loc.IsRtl ? RightToLeft.Yes : RightToLeft.No;
            var open = new ToolStripMenuItem(Loc.Get("tray.open")) { Font = new Font(_menu.Font, FontStyle.Bold) };
            open.Click += (s, e) => _openSettings();
            var refresh = new ToolStripMenuItem(Loc.Get("tray.refresh"));
            refresh.Click += async (s, e) => await _host.RefreshAsync();
            // Bestätigt alle offenen Warnungen: keine weiteren Wiederholungen, bis sich die Stufe ändert.
            _ackAll = new ToolStripMenuItem(Loc.Get("tray.ackAll"));
            _ackAll.Click += (s, e) => _host.AcknowledgeAll();
            var exit = new ToolStripMenuItem(Loc.Get("tray.exit"));
            exit.Click += (s, e) => _exit();
            _menu.Items.AddRange(new ToolStripItem[] { open, refresh, _ackAll, new ToolStripSeparator(), exit });
            _notifyIcon.Text = Trim(Loc.Get("app.name"), MaxTooltip);
            if (_currentIcon != null)
                Update();
        }

        private void OnSnapshot(object sender, EventArgs e) => Update();
        private void OnAlerts(object sender, IReadOnlyList<BatteryAlert> alerts) => ShowAlerts(alerts);

        private void Update()
        {
            if (_disposed)
                return;
            var entries = _host.Latest == null
                ? new List<TrayEntry>()
                : TraySummaryBuilder.Build(_host.Latest.Devices, _host.Settings).ToList();

            // Das Hauptsymbol bleibt auch ohne Geraete oder zusaetzliche Auswahl immer erreichbar.
            if (_currentIcon == null)
            {
                _currentIcon = AppAssets.TrayIcon()
                    ?? throw new InvalidOperationException("The embedded main tray icon is missing.");
                _notifyIcon.Icon = _currentIcon;
            }

            _notifyIcon.Text = Trim(Loc.Get("app.name"), MaxTooltip);
            _additional.Update(entries, TrayIconRenderer.TaskbarIsLight());
        }

        private async void ShowAlerts(IReadOnlyList<BatteryAlert> alerts)
        {
            var worst = alerts.Max(a => a.Level);
            var anyCritical = worst == ChargeLevel.Critical;

            // Ein Ton pro Runde, nicht je Gerät: Es zählt das Gerät mit der schlechtesten Stufe, das einen Ton eingestellt hat.
            var soundAlert = alerts.Where(a => (a.Methods & AlertProfile.SoundMask) != 0).OrderByDescending(a => a.Level).FirstOrDefault();
            var toasts = alerts.Where(a => (a.Methods & NotifyMethods.WindowsToast) != 0).ToList();
            if (soundAlert == null && toasts.Count == 0)
                return;

            await _notificationGate.WaitAsync();
            try
            {
                var sound = soundAlert == null ? Task.CompletedTask
                    : SoundService.Play(soundAlert.Methods, soundAlert.CustomSoundPath, soundAlert.WindowsSoundName, anyCritical);

                if (toasts.Count == 0)
                {
                    await sound;
                    return;
                }

                var primary = toasts.OrderByDescending(a => a.Level).First();
                var toastWorst = primary.Level;
                var title = Loc.Get(toastWorst == ChargeLevel.Critical ? "notify.critical.title"
                    : toastWorst == ChargeLevel.Red ? "notify.red.title" : "notify.yellow.title");
                var body = string.Join("\n", toasts
                    .OrderByDescending(a => a.Level)
                    .Take(4)
                    .Select(a => Loc.Format("notify.body", DeviceNames.Display(a.Device), a.Device.Level)));

                var category = ThresholdEvaluator.EffectiveCategory(primary.Device, _host.Settings.GetDevice(primary.Device.Id));
                // "Gesehen" bestätigt alle Geräte dieser Meldung; der Tag ersetzt ältere Meldungen zum selben Hauptgerät.
                await Notify(title, body, () => _toasts.CategoryImage(category),
                    () => TrayIconRenderer.RenderNotification(category),
                    "device:" + primary.Device.Id, toasts.Take(4).Select(a => a.Device.Id).ToList());
                await sound;
            }
            finally
            {
                _notificationGate.Release();
            }
        }

        /// <summary>"Testen"-Knopf: löst genau die Benachrichtigungen aus, die im Profil eingestellt sind (Meldung und/oder Ton).</summary>
        private async void ShowTest(AlertProfile profile, DeviceCategory? category)
        {
            await _notificationGate.WaitAsync();
            try
            {
                var sound = SoundService.Play(profile.Methods, profile.CustomSoundPath, profile.WindowsSoundName, false);
                if ((profile.Methods & NotifyMethods.WindowsToast) != 0)
                {
                    await Notify(Loc.Get("notify.test.title"), Loc.Get("notify.test.body"),
                        () => category.HasValue ? _toasts.CategoryImage(category.Value) : _toasts.ConfusedImage(),
                        () => category.HasValue ? TrayIconRenderer.RenderNotification(category.Value) : AppAssets.NotificationIcon(),
                        "test", null);
                }
                await sound;
            }
            finally
            {
                _notificationGate.Release();
            }
        }

        private async Task ShowSimulation(AlertProfile profile, ChargeLevel level, int percent)
        {
            await _notificationGate.WaitAsync();
            try
            {
                var sound = SoundService.Play(profile.Methods, profile.CustomSoundPath, profile.WindowsSoundName,
                    level == ChargeLevel.Critical);
                if ((profile.Methods & NotifyMethods.WindowsToast) != 0)
                {
                    var title = Loc.Get(level == ChargeLevel.Critical ? "notify.critical.title"
                        : level == ChargeLevel.Red ? "notify.red.title" : "notify.yellow.title");
                    var levelName = Loc.Get(level == ChargeLevel.Yellow ? "simulation.medium"
                        : level == ChargeLevel.Red ? "simulation.low" : "simulation.critical");
                    var body = Loc.Format("simulation.notificationBody", levelName, percent);
                    await Notify(title, body, _toasts.ConfusedImage, AppAssets.NotificationIcon, "simulation", null);
                }
                await sound;
            }
            finally
            {
                _notificationGate.Release();
            }
        }

        /// <summary>
        /// Zeigt eine Meldung als Windows-Toast (bleibt in der Benachrichtigungszentrale) und wartet, bis sie verschwunden ist.
        /// Falls Toasts nicht verfügbar sind, wird die alte Tray-Sprechblase verwendet.
        /// Aufrufer halten bereits <see cref="_notificationGate"/>, damit sich Meldungen nicht überlagern.
        /// </summary>
        private async Task Notify(string title, string body, Func<string> toastImage, Func<Icon> balloonIcon,
            string tagSource, IReadOnlyList<string> ackDeviceIds)
        {
            if (_toasts.IsAvailable)
            {
                // Vom Nutzer in den Windows-Einstellungen abgeschaltet: respektieren, keine Ersatz-Sprechblase.
                if (_toasts.IsDisabledByUser)
                    return;
                try
                {
                    string image = null;
                    try { image = toastImage(); }
                    catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("Toast image could not be written: " + ex.Message);
                    }
                    await _toasts.ShowAsync(title, body, image, tagSource, ackDeviceIds, SimulationToastFallback);
                    return;
                }
                catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException || ex is ArgumentException
                    || ex is InvalidOperationException || ex is UnauthorizedAccessException)
                {
                    System.Diagnostics.Trace.TraceWarning("Toast failed, using balloon tip: " + ex.Message);
                }
            }

            using (var icon = balloonIcon())
                await ShowBalloonTip(BalloonTipTimeoutMilliseconds, Trim(title, MaxBalloonTitle), Trim(body, MaxBalloonText), icon);
        }

        private Task ShowBalloonTip(int timeout, string title, string body, Icon icon)
        {
            var closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler onClosed = null;
            onClosed = (s, e) =>
            {
                _notifyIcon.BalloonTipClosed -= onClosed;
                closed.TrySetResult(true);
            };
            _notifyIcon.BalloonTipClosed += onClosed;

            try
            {
                _notifyIcon.ShowBalloonTip(timeout, title, body, icon);
            }
            catch
            {
                _notifyIcon.BalloonTipClosed -= onClosed;
                throw;
            }

            return WaitForBalloonTipClosedAsync(closed.Task, onClosed);
        }

        private async Task WaitForBalloonTipClosedAsync(Task closed, EventHandler onClosed)
        {
            // Windows may ignore the requested timeout and use the user's accessibility setting instead.
            await Task.WhenAny(closed, Task.Delay(SimulationToastFallback));
            _notifyIcon.BalloonTipClosed -= onClosed;
        }

        private void OnThemeChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
                _ui.Post(s => Update(), null);
        }

        private static string Trim(string text, int max) => text.Length <= max ? text : text.Substring(0, max - 1) + "…";

        public void Dispose()
        {
            _disposed = true;
            SystemEvents.UserPreferenceChanged -= OnThemeChanged;
            _host.SnapshotUpdated -= OnSnapshot;
            _host.AlertsRaised -= OnAlerts;
            _host.TestRequested -= ShowTest;
            _host.SimulationRequested -= ShowSimulation;
            _additional.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _menu.Dispose();
            _currentIcon?.Dispose();
        }
    }
}
