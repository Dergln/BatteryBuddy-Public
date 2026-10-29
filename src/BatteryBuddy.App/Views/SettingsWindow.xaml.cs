using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Threading;
using BatteryBuddy.App.Services;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using BatteryBuddy.Core.Settings;

namespace BatteryBuddy.App.Views
{
    /// <summary>Eine Zeile in der Geräteliste (reine Anzeige-Daten).</summary>
    public sealed class DeviceRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Glyph { get; set; }
        public ImageSource CategoryImage { get; set; }
        public DeviceCategory Category { get; set; }
        public double Percent { get; set; }
        public string LevelText { get; set; }
        public Brush Brush { get; set; }
        public double Opacity { get; set; }
        public string Summary { get; set; }
        public string ContactText { get; set; }
        public string ContactTip { get; set; }
        public string CategoryName { get; set; }
        public int CategorySort { get; set; }
    }

    internal sealed class ChoiceItem
    {
        public string Code { get; set; }
        public DeviceCategory? Category { get; set; }
        public string Text { get; set; }
        public override string ToString() => Text;
    }

    /// <summary>Konfigurationsfenster mit den Tabs "Geräte" und "Einstellungen". Alle Texte kommen aus <see cref="Loc"/>.</summary>
    public partial class SettingsWindow : Window
    {
        private static readonly int[] IntervalChoices = { 1, 2, 5, 10, 15, 30, 60, 120, 240 };

        private readonly AppHost _host;
        private readonly Action _languageChanged;
        private AppSettings _work;
        private bool _loading = true; // Während des Aufbaus lösen Regler schon Ereignisse aus; die sollen ignoriert werden.
        private bool _dirty;          // Es gibt ungespeicherte Änderungen (steuert die Rückfrage beim Schließen).
        private string _selectedId;
        private string _panelDeviceId; // Für welches Gerät die Eingabefelder gerade gefüllt sind.
        private bool _simulationRunning;
        private bool _skipClosePrompt;
        private CancellationTokenSource _simulationHideCancellation;
        private readonly DispatcherTimer _savedStatusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };

        public SettingsWindow(AppHost host, Action languageChanged)
        {
            InitializeComponent();
            ThemeService.StyleTitleBar(this);
            // Four numeric parts match the executable's file-version field shown in Windows properties.
            VersionText.Text = "v" + typeof(SettingsWindow).Assembly.GetName().Version.ToString(4);
            _host = host;
            _languageChanged = languageChanged;
            _work = host.Settings.Clone();
            _savedStatusTimer.Tick += (s, e) => ShowStatus(string.Empty);

            _host.SnapshotUpdated += OnSnapshot;
            Closed += (s, e) => _host.SnapshotUpdated -= OnSnapshot;

            DevAlerts.Changed += (s, e) => OnDeviceChanged(s, null);
            DevAlerts.ResetRequested += OnResetDevice;
            DevAlerts.TestRequested += (s, p) =>
            {
                // Use the displayed working-copy category, including an unsaved category override.
                if (DeviceList.SelectedItem is DeviceRow row)
                    _host.RequestTest(p, row.Category);
            };
            DefAlerts.Changed += (s, e) => MarkDirty();
            DefAlerts.ResetRequested += OnResetAll;
            DefAlerts.RestoreRequested += OnRestoreDefaults;
            DefAlerts.TestRequested += (s, p) => _host.RequestTest(p);
            DefAlerts.SimulationRequested += OnSimulationRequested;
            DefAlerts.SimulationDismissRequested += OnSimulationDismissRequested;
            DefAlerts.EnableSimulation(true);
            Icon = AppAssets.WindowIcon();

            ApplyTexts();
            LoadGeneralControls();
            _loading = false;
            ReloadRows();
            Closed += (s, e) =>
            {
                _simulationHideCancellation?.Cancel();
                _savedStatusTimer.Stop();
            };
        }

        private void OnSnapshot(object sender, EventArgs e) => ReloadRows();

        private void MarkDirty()
        {
            if (_loading)
                return;
            _dirty = true;
            ShowStatus(string.Empty);
        }

        private void ShowStatus(string text, bool saved = false)
        {
            // Nur Erfolgsmeldungen laufen ab; neue Fehler/Simulationen duerfen kein alter Timer ausblenden.
            _savedStatusTimer.Stop();
            StatusText.Text = text;
            if (saved)
                _savedStatusTimer.Start();
        }

        /// <summary>Wird beim Abmelden/Herunterfahren aufgerufen: Dort darf keine Rückfrage das Beenden blockieren.</summary>
        public void SkipClosePrompt()
        {
            _dirty = false;
            _skipClosePrompt = true;
            _simulationHideCancellation?.Cancel();
        }

        // ---------- Texte ----------

        private void ApplyTexts()
        {
            FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            Title = Loc.Get("app.name");
            DevicesTab.Header = Loc.Get("tab.devices");
            SettingsTab.Header = Loc.Get("tab.settings");
            RefreshButton.Content = Loc.Get("devices.refresh");
            SelectHint.Text = Loc.Get("devices.select");
            DevIgnore.Content = Loc.Get("device.ignore");
            DevCategoryLabel.Text = Loc.Get("device.category");
            DevAlerts.SetTexts(Loc.Get("device.reset"));

            GeneralHeader.Text = Loc.Get("settings.general");
            Autostart.Content = Loc.Get("settings.autostart");
            StartMenu.Content = Loc.Get("settings.startmenu");
            TrayBehaviorLabel.Text = Loc.Get("settings.additionalTray");
            TrayByCategoryText.Text = Loc.Get("settings.trayByCategory");
            TrayByDeviceText.Text = Loc.Get("settings.trayByDevice");
            TrayThresholdHint.Text = Loc.Get("settings.trayThresholdHint") + " ";
            TrayAlwaysShow.Content = Loc.Get("settings.trayAlwaysShow");
            AutomationProperties.SetName(TrayByCategory, TrayByCategoryText.Text);
            AutomationProperties.SetName(TrayByDevice, TrayByDeviceText.Text);
            IntervalLabel.Text = Loc.Get("settings.interval");
            ActiveLabel.Text = Loc.Get("settings.trayActive");
            ActiveHint.Text = Loc.Get("settings.trayActiveHint");
            ActiveValue.Text = Loc.Format("settings.minutes", (int)ActiveSlider.Value);
            LanguageLabel.Text = Loc.Get("settings.language");
            ThemeLabel.Text = Loc.Get("settings.theme");
            DefaultsHeader.Text = Loc.Get("settings.defaults");
            DefAlerts.SetTexts(Loc.Get("settings.resetAll"), Loc.Get("settings.restore"), Loc.Get("settings.legend"));
            LicenseTab.Header = Loc.Get("tab.license");
            LicenseContent.SetTexts();
            ProVersionTab.Header = Loc.Get("tab.proVersion");
            ProVersionContent.SetTexts();
            ContribTab.Header = Loc.Get("tab.contrib");
            ContribHeader.Text = Loc.Get("tab.contrib");
            ContribIntro.Text = Loc.Get("contrib.intro");
            ContributionsView.Build(ContribGrid);
            BugReportIcon.Source = AppAssets.InfoImage("bug");
            BugReportText.Text = Loc.Get("footer.bug");
            var bugHint = Loc.Get("footer.bugTooltip") + Environment.NewLine + Loc.Get("footer.bugClickHint");
            FooterToolTips.Set(BugReportButton, FooterToolTips.Text(bugHint));
            AutomationProperties.SetName(BugReportButton, Loc.Get("footer.bug"));
            AutomationProperties.SetHelpText(BugReportButton, bugHint);
            ColName.Text = Loc.Get("devices.colName");
            ColCharge.Text = Loc.Get("devices.colCharge");
            ColContact.Text = Loc.Get("devices.colContact");
            SaveButton.Content = Loc.Get("button.save");
            CloseButton.Content = Loc.Get("button.close");
            OriginInfo.SetTexts();
            FooterToolTips.Set(VersionInfo, FooterToolTips.Version(VersionText.Text));

            // Bedienhilfen: Screenreader lesen den Namen der Auswahlfelder vor.
            AutomationProperties.SetName(Interval, Loc.Get("settings.interval"));
            AutomationProperties.SetName(LanguageBox, Loc.Get("settings.language"));
            AutomationProperties.SetName(ThemeBox, Loc.Get("settings.theme"));
            AutomationProperties.SetName(DevCategory, Loc.Get("device.category"));
        }

        // ---------- Tab "Einstellungen" ----------

        private void LoadGeneralControls()
        {
            var wasLoading = _loading;
            _loading = true;
            try
            {
                Autostart.IsChecked = AutostartService.IsEnabled();
                StartMenu.IsChecked = StartMenuService.Exists();
                TrayByCategory.IsChecked = _work.TrayMode == AdditionalTrayMode.LowestPerCategory;
                TrayByDevice.IsChecked = _work.TrayMode == AdditionalTrayMode.PerDevice;
                TrayAlwaysShow.IsChecked = _work.AlwaysShowAdditionalTrayIcons;

                var choices = IntervalChoices.Concat(new[] { _work.PollMinutes }).Distinct().OrderBy(x => x).ToList();
                Interval.ItemsSource = choices;
                Interval.SelectedItem = _work.PollMinutes;
                ActiveSlider.Value = _work.TrayActiveMinutes;
                ActiveValue.Text = Loc.Format("settings.minutes", _work.TrayActiveMinutes);
                AutomationProperties.SetName(ActiveSlider, Loc.Get("settings.trayActive"));

                var languages = new List<ChoiceItem> { new ChoiceItem { Code = string.Empty, Text = Loc.Get("settings.languageAuto") } };
                languages.AddRange(Loc.Available().Select(c => new ChoiceItem { Code = c, Text = Loc.NativeName(c) }));
                LanguageBox.ItemsSource = languages;
                LanguageBox.SelectedItem = languages.FirstOrDefault(l => string.Equals(l.Code, _work.Language, StringComparison.OrdinalIgnoreCase)) ?? languages[0];

                var themes = new List<ChoiceItem>
                {
                    new ChoiceItem { Code = string.Empty, Text = Loc.Get("theme.system") },
                    new ChoiceItem { Code = "light", Text = Loc.Get("theme.light") },
                    new ChoiceItem { Code = "dark", Text = Loc.Get("theme.dark") }
                };
                ThemeBox.ItemsSource = themes;
                ThemeBox.SelectedItem = themes.First(t => t.Code == _work.Theme);

                DefAlerts.Load(_work.GetDefaultProfile());
            }
            finally
            {
                _loading = wasLoading;
            }
        }

        private void OnGeneralChanged(object sender, RoutedEventArgs e) => MarkDirty();

        private void OnTrayModeChanged(object sender, RoutedEventArgs e)
        {
            if (_loading)
                return;
            if (ReferenceEquals(sender, TrayByCategory) && TrayByCategory.IsChecked == true)
                TrayByDevice.IsChecked = false;
            if (ReferenceEquals(sender, TrayByDevice) && TrayByDevice.IsChecked == true)
                TrayByCategory.IsChecked = false;
            MarkDirty();
        }

        /// <summary>
        /// Werkseinstellungen: Autostart an, Intervall 10 Min., Tray-Zeitfenster 30 Min., keine Zusatzsymbole,
        /// Sprache/Darstellung wie Windows sowie Standardprofile; gespeichert wird erst mit "Speichern".
        /// </summary>
        private void OnRestoreDefaults(object sender, EventArgs e)
        {
            Autostart.IsChecked = true;
            Interval.ItemsSource = IntervalChoices.Concat(new[] { AppSettings.DefaultPollMinutes }).Distinct().OrderBy(x => x).ToList();
            Interval.SelectedItem = AppSettings.DefaultPollMinutes;
            ActiveSlider.Value = AppSettings.DefaultTrayActiveMinutes;
            LanguageBox.SelectedIndex = 0; // "Windows-Sprache" steht immer an erster Stelle
            ThemeBox.SelectedIndex = 0;    // "Wie Windows"
            TrayByCategory.IsChecked = TrayByDevice.IsChecked = false;
            TrayAlwaysShow.IsChecked = false;
            DefAlerts.Load(new AlertProfile());
            MarkDirty();
        }

        /// <summary>Setzt ALLE Geräte auf die aktuell eingestellten Standardwerte (mit Rückfrage, weil das viele Einstellungen betrifft).</summary>
        private void OnResetAll(object sender, EventArgs e)
        {
            if (ConfirmDialog.Ask(this, Loc.Get("settings.resetAll"), Loc.Get("settings.resetAllConfirm"), Loc.Get("button.yes"), Loc.Get("button.no"), null) != true)
                return;
            _work.SetDefaultProfile(DefAlerts.Read());
            _work.ResetAllDevicesToDefault();
            MarkDirty();
            _panelDeviceId = null;
            ReloadRows();
        }

        private async void OnSimulationRequested(object sender, EventArgs e)
        {
            if (_simulationRunning)
                return;

            var profile = PrepareSimulationProfile();
            if (profile == null)
                return;

            _simulationHideCancellation?.Cancel();
            _simulationHideCancellation?.Dispose();
            var hideCancellation = new CancellationTokenSource();
            _simulationHideCancellation = hideCancellation;

            var plan = DischargeSimulationPlan.Create(profile);
            var hasNotificationMethod = (profile.Methods & (NotifyMethods.WindowsToast | AlertProfile.SoundMask)) != 0;
            var rows = plan.Select(step =>
            {
                var brush = new SolidColorBrush(LevelColors.Text(step.Level));
                brush.Freeze();
                var estimateSeconds = step.Notify && hasNotificationMethod ? 12 : 0;
                return new SimulationProgressRow
                {
                    Status = Loc.Get(step.Level == ChargeLevel.Green ? "simulation.completed" : "simulation.queued"),
                    Phase = Loc.Get("simulation." + (step.Level == ChargeLevel.Yellow ? "medium"
                        : step.Level == ChargeLevel.Red ? "low"
                        : step.Level == ChargeLevel.Critical ? "critical" : "green")),
                    Threshold = Loc.Format("simulation.percent", step.Percent),
                    Estimate = Loc.Format("simulation.seconds", estimateSeconds),
                    LevelBrush = brush
                };
            }).ToList();

            DefAlerts.ShowSimulationRows(rows);
            // After the panel becomes visible, bring its full contents into view without relying on fixed pixel offsets.
            await Dispatcher.InvokeAsync(() =>
            {
                SettingsScrollViewer.UpdateLayout();
                SettingsScrollViewer.ScrollToBottom();
            }, DispatcherPriority.Loaded);
            DefAlerts.SetSimulationRunning(true);
            SetSimulationUiEnabled(false);
            _simulationRunning = true;

            var currentStepIndex = -1;
            try
            {
                for (var i = 1; i < plan.Count; i++)
                {
                    var step = plan[i];
                    currentStepIndex = i;
                    if (!step.Notify || !hasNotificationMethod)
                    {
                        rows[i].Status = Loc.Get("simulation.disabled");
                        continue;
                    }

                    rows[i].Status = Loc.Get("simulation.running");
                    await _host.RequestSimulation(profile, step.Level, step.Percent);
                    rows[i].Status = Loc.Get("simulation.completed");
                }
            }
            catch (Exception ex)
            {
                if (currentStepIndex >= 0)
                    rows[currentStepIndex].Status = Loc.Get("simulation.failedStatus");
                ShowStatus(Loc.Format("simulation.failed", ex.Message));
                StatusText.SetResourceReference(TextBlock.ForegroundProperty, "BB.Error");
            }
            finally
            {
                _simulationRunning = false;
                DefAlerts.SetSimulationRunning(false);
                SetSimulationUiEnabled(true);
                DefAlerts.SetSimulationButtonEnabled(false);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), hideCancellation.Token);
                DefAlerts.HideSimulation();
                DefAlerts.SetSimulationButtonEnabled(true);
            }
            catch (OperationCanceledException)
            {
                // A manual dismiss or a new run owns the progress panel now.
            }
            finally
            {
                if (ReferenceEquals(_simulationHideCancellation, hideCancellation))
                {
                    _simulationHideCancellation.Dispose();
                    _simulationHideCancellation = null;
                }
            }
        }

        private AlertProfile PrepareSimulationProfile()
        {
            if (!_dirty)
                return _host.Settings.GetDefaultProfile();

            var save = ConfirmDialog.Ask(this, Loc.Get("app.name"), Loc.Get("simulation.unsaved"),
                Loc.Get("button.yes"), Loc.Get("button.no"), Loc.Get("button.cancel"));
            if (save == null)
                return null;

            if (save == true)
                return SaveSettings() ? _host.Settings.GetDefaultProfile() : null;

            // Discard the whole working copy so a simulation never mixes saved and unsaved device/global values.
            _work = _host.Settings.Clone();
            _panelDeviceId = null;
            LoadGeneralControls();
            ReloadRows();
            return _host.Settings.GetDefaultProfile();
        }

        private void SetSimulationUiEnabled(bool enabled)
        {
            DevicesTab.IsEnabled = enabled;
            ContribTab.IsEnabled = enabled;
            Autostart.IsEnabled = enabled;
            StartMenu.IsEnabled = enabled;
            Interval.IsEnabled = enabled;
            ActiveSlider.IsEnabled = enabled;
            TrayBehaviorOptions.IsEnabled = enabled;
            TrayAlwaysShow.IsEnabled = enabled;
            LanguageBox.IsEnabled = enabled;
            ThemeBox.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;
            CloseButton.IsEnabled = enabled;
        }

        private void OnSimulationDismissRequested(object sender, EventArgs e)
        {
            _simulationHideCancellation?.Cancel();
            DefAlerts.HideSimulation();
            DefAlerts.SetSimulationButtonEnabled(true);
        }

        // ---------- Tab "Geräte" ----------

        private void ReloadRows()
        {
            var devices = _host.Latest?.Devices ?? new List<BatteryDevice>();
            var now = DateTimeOffset.Now;
            var rows = devices.Select(d =>
            {
                var ds = _work.GetDevice(d.Id);
                var category = ThresholdEvaluator.EffectiveCategory(d, ds);
                var level = ThresholdEvaluator.Evaluate(d.Level, ds);
                var levelText = d.Level.HasValue ? Loc.Format("devices.level", d.Level.Value) : Loc.Get("devices.unknown");
                if (d.IsCharging)
                    levelText += " ⚡";
                var brush = new SolidColorBrush(LevelColors.Text(level));
                brush.Freeze();
                var age = now - d.LastContact;
                var contact = FormatContact(age);
                var inactive = age > TimeSpan.FromMinutes(_work.TrayActiveMinutes); // zählt nicht für den Tray
                return new DeviceRow
                {
                    Id = d.Id,
                    Name = DeviceNames.Display(d),
                    Glyph = CategoryGlyphs.Get(category),
                    CategoryImage = CategoryImages.Get(category),
                    Category = category,
                    Percent = d.Level ?? 0,
                    LevelText = levelText,
                    Brush = brush,
                    Opacity = ds.Ignore ? 0.45 : inactive ? 0.7 : 1.0,
                    Summary = DeviceNames.Display(d) + ", " + levelText + ", " + contact,
                    ContactText = contact,
                    ContactTip = Loc.Format("contact.tip", contact),
                    CategoryName = Loc.Get("category." + category),
                    CategorySort = (int)category
                };
            }).ToList();

            var wasLoading = _loading;
            _loading = true;
            try
            {
                var view = new ListCollectionView(rows);
                view.SortDescriptions.Add(new SortDescription(nameof(DeviceRow.CategorySort), ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription(nameof(DeviceRow.Name), ListSortDirection.Ascending));
                view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DeviceRow.CategoryName)));
                DeviceList.ItemsSource = view;
                DeviceList.SelectedItem = rows.FirstOrDefault(r => r.Id == _selectedId);
            }
            finally
            {
                _loading = wasLoading;
            }

            // Leerer Bereich: solange noch kein Provider ein Gerät gemeldet hat (Latest == null), zeigen wir das Ladebild,
            // danach (alle Abfragen fertig, nichts gefunden) das "verwirrte Batterie"-Bild mit Hinweis.
            var empty = rows.Count == 0;
            var stillLoading = empty && _host.Latest == null;
            StatePanel.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            DeviceList.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
            if (empty)
            {
                StateImage.Source = stillLoading ? AppAssets.LoadingImage() : AppAssets.ConfusedBatteryImage();
                EmptyText.Text = Loc.Get(stillLoading ? "devices.loading" : "devices.empty");
                EmptyHint.Text = stillLoading ? string.Empty : Loc.Get("devices.emptyHint");
                EmptyHint.Visibility = stillLoading ? Visibility.Collapsed : Visibility.Visible;
                AutomationProperties.SetName(StateImage, EmptyText.Text);
            }

            var snap = _host.Latest;
            UpdatedText.Text = snap == null ? string.Empty : Loc.Format("devices.updated", snap.Time.LocalDateTime.ToString("T"));
            var failed = snap?.FailedProviders;
            FailedText.Visibility = failed != null && failed.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            FailedText.Text = failed != null && failed.Count > 0 ? Loc.Format("devices.failed", string.Join(", ", failed)) : string.Empty;

            ShowDetails();
        }

        private void OnRefresh(object sender, RoutedEventArgs e) => _ = _host.RefreshAsync();

        private void OnBugReport(object sender, RoutedEventArgs e)
            => ExternalLinkService.Open(new Uri("https://github.com/Dergln/BatteryBuddy-Public/issues", UriKind.Absolute));

        /// <summary>Beschriftung für "letzter Kontakt": "&lt;5m", "&lt;2h" oder "&gt;24h" (Einheiten kommen aus den Sprachdateien).</summary>
        private static string FormatContact(TimeSpan age)
        {
            var t = ContactAge.Describe(age);
            switch (t.Kind)
            {
                case ContactAgeKind.LessThanMinutes: return Loc.Format("contact.lessMin", t.Value);
                case ContactAgeKind.LessThanHours: return Loc.Format("contact.lessHour", t.Value);
                default: return Loc.Format("contact.moreDay", t.Value);
            }
        }

        private void OnActiveChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ActiveValue.Text = Loc.Format("settings.minutes", (int)ActiveSlider.Value);
            MarkDirty();
        }

        private void OnDeviceSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_loading)
                return;
            _selectedId = (DeviceList.SelectedItem as DeviceRow)?.Id;
            ShowDetails();
        }

        private void ShowDetails()
        {
            var device = _host.Latest?.Devices.FirstOrDefault(d => d.Id == _selectedId);
            DetailPanel.Visibility = device == null ? Visibility.Collapsed : Visibility.Visible;
            SelectHint.Visibility = device == null ? Visibility.Visible : Visibility.Collapsed;
            if (device == null)
            {
                _panelDeviceId = null;
                return;
            }

            DetailTitle.Text = Loc.Format("devices.settingsFor", DeviceNames.Display(device));
            // Die Eingabefelder nur beim Gerätewechsel neu füllen, sonst springt z. B. der Cursor im Pfadfeld.
            if (_panelDeviceId == device.Id)
                return;
            _panelDeviceId = device.Id;

            var ds = _work.GetDevice(device.Id);
            var wasLoading = _loading;
            _loading = true;
            try
            {
                DevIgnore.IsChecked = ds.Ignore;

                var items = new List<ChoiceItem> { new ChoiceItem { Category = null, Text = Loc.Get("device.categoryAuto") } };
                items.AddRange(Enum.GetValues(typeof(DeviceCategory)).Cast<DeviceCategory>()
                    .Select(c => new ChoiceItem { Category = c, Text = Loc.Get("category." + c) }));
                DevCategory.ItemsSource = items;
                DevCategory.SelectedItem = items.First(i => i.Category == ds.CategoryOverride);

                DevAlerts.Load(ds.ToProfile());
                DevOptions.IsEnabled = !ds.Ignore;
            }
            finally
            {
                _loading = wasLoading;
            }
        }

        /// <summary>Schreibt die Werte der Steuerelemente in die Arbeitskopie der Einstellungen (gespeichert wird erst mit "Speichern").</summary>
        private void OnDeviceChanged(object sender, RoutedEventArgs e)
        {
            if (_loading || _selectedId == null)
                return;
            var device = _host.Latest?.Devices.FirstOrDefault(d => d.Id == _selectedId);
            if (device == null)
                return;

            var ds = _work.GetOrAddDevice(device.Id, device.Name);
            ds.Ignore = DevIgnore.IsChecked == true;
            ds.CategoryOverride = (DevCategory.SelectedItem as ChoiceItem)?.Category;
            ds.ApplyProfile(DevAlerts.Read());
            DevOptions.IsEnabled = !ds.Ignore;
            MarkDirty();
            ReloadRows();
        }

        /// <summary>Setzt nur das ausgewählte Gerät auf die Standardwerte zurück (Ignorieren und Kategorie bleiben).</summary>
        private void OnResetDevice(object sender, EventArgs e)
        {
            var device = _host.Latest?.Devices.FirstOrDefault(d => d.Id == _selectedId);
            if (device == null)
                return;
            var ds = _work.GetOrAddDevice(device.Id, device.Name);
            ds.ApplyProfile(_work.GetDefaultProfile());
            ds.CategoryOverride = null; // Standard ist "Automatisch" (Kategorie wird wieder vom Anbieter erkannt)
            MarkDirty();
            _panelDeviceId = null; // Felder neu füllen
            ReloadRows();
        }

        // ---------- Speichern / Schließen ----------

        private bool SaveSettings()
        {
            var oldLanguage = _work.Language;
            _work.PollMinutes = Interval.SelectedItem is int m ? m : _work.PollMinutes;
            _work.TrayActiveMinutes = (int)ActiveSlider.Value;
            _work.TrayMode = TrayByCategory.IsChecked == true ? AdditionalTrayMode.LowestPerCategory
                : TrayByDevice.IsChecked == true ? AdditionalTrayMode.PerDevice : AdditionalTrayMode.None;
            _work.AlwaysShowAdditionalTrayIcons = TrayAlwaysShow.IsChecked == true;
            _work.Language = (LanguageBox.SelectedItem as ChoiceItem)?.Code ?? string.Empty;
            _work.Theme = (ThemeBox.SelectedItem as ChoiceItem)?.Code ?? string.Empty;
            _work.SetDefaultProfile(DefAlerts.Read());

            var ok = _host.Apply(_work);
            if (AutostartService.IsEnabled() != (Autostart.IsChecked == true))
                ok &= AutostartService.SetEnabled(Autostart.IsChecked == true);
            if (StartMenuService.Exists() != (StartMenu.IsChecked == true))
                ok &= StartMenuService.SetEnabled(StartMenu.IsChecked == true);

            // Das Hauptobjekt gehört jetzt dem Host; wir arbeiten mit einer frischen Kopie weiter.
            _work = _host.Settings.Clone();
            _panelDeviceId = null;
            ThemeService.Apply(_work.Theme);

            if (!string.Equals(oldLanguage, _work.Language, StringComparison.Ordinal))
            {
                Loc.Init(_work.Language);
                _languageChanged();
                ApplyTexts();
            }

            LoadGeneralControls();
            ReloadRows();

            if (ok)
                _dirty = false;
            ShowStatus(Loc.Get(ok ? "status.saved" : "status.saveFailed"), ok);
            StatusText.SetResourceReference(TextBlock.ForegroundProperty, ok ? "BB.Ok" : "BB.Error");
            return ok;
        }

        private void OnSave(object sender, RoutedEventArgs e) => SaveSettings();

        private void OnClose(object sender, RoutedEventArgs e) => Close();

        /// <summary>Bei ungespeicherten Änderungen: speichern / verwerfen / im Fenster bleiben.</summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!_skipClosePrompt && _simulationRunning)
            {
                e.Cancel = true;
                ShowStatus(Loc.Get("simulation.running"));
                return;
            }
            if (!_dirty)
                return;

            var answer = ConfirmDialog.Ask(this, Loc.Get("app.name"), Loc.Get("close.unsaved"),
                Loc.Get("close.save"), Loc.Get("close.discard"), Loc.Get("button.cancel"));
            if (answer == null)
                e.Cancel = true;
            else if (answer == true && !SaveSettings())
                e.Cancel = true; // Speichern gescheitert: Fenster offen lassen, damit nichts verloren geht.
        }
    }
}
