using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using BatteryBuddy.App.Services;
using BatteryBuddy.Core.Settings;
using Microsoft.Win32;

namespace BatteryBuddy.App.Views
{
    public sealed class SimulationProgressRow : INotifyPropertyChanged
    {
        private string _status;

        public string Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public string Phase { get; set; }
        public string Threshold { get; set; }
        public string Estimate { get; set; }
        public Brush LevelBrush { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Bearbeitet ein <see cref="AlertProfile"/> (Schwellen, Benachrichtigungen, "Benachrichtigen über", Reset).
    /// Gleiche Oberfläche für ein einzelnes Gerät und für die globalen Standardwerte.
    /// </summary>
    public partial class AlertPanel : UserControl
    {
        private bool _loading = true; // Beim Aufbau lösen die Regler schon Ereignisse aus (Wertbegrenzung); die sollen ignoriert werden.

        /// <summary>Der Nutzer hat irgendetwas geändert.</summary>
        public event EventHandler Changed;

        /// <summary>Der Nutzer hat den Reset-Knopf gedrückt (der Besitzer entscheidet, was zurückgesetzt wird).</summary>
        public event EventHandler ResetRequested;

        /// <summary>Nur die globale Ansicht: "BatteryBuddy-Standardwerte wiederherstellen".</summary>
        public event EventHandler RestoreRequested;

        /// <summary>Der Nutzer hat "Testen" gedrückt; der Besitzer löst die gewählten Benachrichtigungen aus.</summary>
        public event EventHandler<AlertProfile> TestRequested;

        /// <summary>Nur die globale Ansicht: startet die simulierte Entladungsfolge.</summary>
        public event EventHandler SimulationRequested;

        public event EventHandler SimulationDismissRequested;

        public AlertPanel()
        {
            InitializeComponent();
            WinSoundBox.ItemsSource = SoundService.ListWindowsSounds();
            YellowBox.Background = Brush(LevelColors.BoxYellow);
            RedBox.Background = Brush(LevelColors.BoxRed);
            CriticalBox.Background = Brush(LevelColors.BoxCritical);
            NotifyYellowBox.Background = YellowBox.Background;
            NotifyRedBox.Background = RedBox.Background;
            NotifyCriticalBox.Background = CriticalBox.Background;
            // Auswahl 1..10 für "Benachrichtigen für [x] Prüfintervalle".
            var counts = Enumerable.Range(AlertProfile.MinRepeat, AlertProfile.MaxRepeat - AlertProfile.MinRepeat + 1).ToArray();
            foreach (var box in new[] { RepeatYellow, RepeatRed, RepeatCritical })
            {
                box.ItemsSource = counts;
                box.SelectedItem = AlertProfile.MinRepeat;
            }
            _loading = false;
        }

        private static Brush Brush(Color c)
        {
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        public void SetTexts(string resetText, string restoreText = null, string thresholdHint = null)
        {
            // Nur die globale Ansicht hat eine Legende; sie bleibt direkt bei den Schwellenreglern.
            ThresholdHint.Text = thresholdHint ?? string.Empty;
            ThresholdHint.Visibility = string.IsNullOrEmpty(thresholdHint) ? Visibility.Collapsed : Visibility.Visible;
            ShowInTray.Content = Loc.Get("device.showInTray");
            YellowLabel.Text = Loc.Get("device.yellowAt");
            RedLabel.Text = Loc.Get("device.redAt");
            CriticalLabel.Text = Loc.Get("device.criticalAt");
            NotifyYellow.Content = Loc.Get("device.notifyYellow");
            NotifyRed.Content = Loc.Get("device.notifyRed");
            NotifyCritical.Content = Loc.Get("device.notifyCritical");
            // "Notify for {0} check intervals": Text vor und nach dem Platzhalter umschließt die Auswahlbox.
            var repeat = Loc.Get("notify.repeat");
            var split = repeat.IndexOf("{0}", StringComparison.Ordinal);
            var pre = split < 0 ? repeat : repeat.Substring(0, split).TrimEnd();
            var post = split < 0 ? string.Empty : repeat.Substring(split + 3).TrimStart();
            foreach (var t in new[] { RepeatYellowPre, RepeatRedPre, RepeatCriticalPre }) t.Text = pre;
            foreach (var t in new[] { RepeatYellowPost, RepeatRedPost, RepeatCriticalPost }) t.Text = post;
            var repeatName = repeat.Replace("{0}", "x");
            foreach (var box in new[] { RepeatYellow, RepeatRed, RepeatCritical })
            {
                AutomationProperties.SetName(box, repeatName);
                box.ToolTip = Loc.Get("notify.repeatHint");
            }
            WhenLabel.Text = Loc.Get("notify.when");
            WhenStartup.Content = Loc.Get("notify.when.startup");
            WhenLevel.Content = Loc.Get("notify.when.level");
            ViaLabel.Text = Loc.Get("notify.via");
            ViaToast.Content = Loc.Get("notify.via.toast");
            ViaSound.Content = Loc.Get("notify.via.sound");
            ViaWindows.Content = Loc.Get("notify.via.windows");
            ViaCustom.Content = Loc.Get("notify.via.custom");
            BrowseButton.Content = Loc.Get("notify.browse");
            TestButton.Content = Loc.Get("notify.test");
            SimulationButton.Content = Loc.Get("simulation.start");
            SimulationHeader.Text = Loc.Get("simulation.title");
            SimulationHint.Text = Loc.Get("simulation.hint");
            SimulationHideButton.Content = Loc.Get("simulation.hide");
            SimulationStatusHeader.Text = Loc.Get("simulation.columnStatus");
            SimulationPhaseHeader.Text = Loc.Get("simulation.columnPhase");
            SimulationThresholdHeader.Text = Loc.Get("simulation.columnThreshold");
            SimulationEstimateHeader.Text = Loc.Get("simulation.columnEstimate");
            PathWarning.Text = Loc.Get("notify.pathInvalid");
            ResetButton.Content = resetText;
            RestoreButton.Visibility = restoreText == null ? Visibility.Collapsed : Visibility.Visible;
            RestoreButton.Content = restoreText;
            SoundHint.Text = Loc.Get("notify.soundHint");

            // Screenreader: Regler und Pfadfeld brauchen einen Namen.
            AutomationProperties.SetName(WinSoundBox, Loc.Get("notify.via.windows"));
            AutomationProperties.SetName(YellowSlider, Loc.Get("device.yellowAt"));
            AutomationProperties.SetName(RedSlider, Loc.Get("device.redAt"));
            AutomationProperties.SetName(CriticalSlider, Loc.Get("device.criticalAt"));
            AutomationProperties.SetName(CustomPath, Loc.Get("notify.via.custom"));
            AutomationProperties.SetName(SimulationButton, Loc.Get("simulation.start"));
        }

        public void EnableSimulation(bool enabled)
            => SimulationButton.Visibility = enabled ? Visibility.Visible : Visibility.Collapsed;

        public void ShowSimulationRows(System.Collections.Generic.IEnumerable<SimulationProgressRow> rows)
        {
            SimulationSteps.ItemsSource = rows;
            SimulationPanel.Visibility = Visibility.Visible;
        }

        public void SetSimulationRunning(bool running)
        {
            SettingsFields.IsEnabled = !running;
            TestButton.IsEnabled = !running;
            SimulationButton.IsEnabled = !running;
            ActionButtons.IsEnabled = !running;
            SimulationHideButton.IsEnabled = !running;
            if (!running)
                UpdateEnabled();
        }

        public void SetSimulationButtonEnabled(bool enabled)
            => SimulationButton.IsEnabled = enabled;

        public void HideSimulation()
            => SimulationPanel.Visibility = Visibility.Collapsed;

        public void Load(AlertProfile p)
        {
            _loading = true;
            try
            {
                YellowSlider.Value = p.YellowAt;
                ShowInTray.IsChecked = p.ShowInTray;
                RedSlider.Value = p.RedAt;
                CriticalSlider.Value = p.CriticalAt;
                ShowValues();
                NotifyYellow.IsChecked = p.NotifyYellow;
                NotifyRed.IsChecked = p.NotifyRed;
                NotifyCritical.IsChecked = p.NotifyCritical;
                RepeatYellow.SelectedItem = AlertProfile.ClampRepeat(p.RepeatYellow);
                RepeatRed.SelectedItem = AlertProfile.ClampRepeat(p.RepeatRed);
                RepeatCritical.SelectedItem = AlertProfile.ClampRepeat(p.RepeatCritical);
                WhenStartup.IsChecked = p.NotifyOnStartup;
                WhenLevel.IsChecked = p.NotifyOnLevelChange;
                ViaToast.IsChecked = (p.Methods & NotifyMethods.WindowsToast) != 0;
                CustomPath.Text = p.CustomSoundPath ?? string.Empty;
                WinSoundBox.SelectedItem = ((string[])WinSoundBox.ItemsSource).FirstOrDefault(n => string.Equals(n, p.WindowsSoundName, StringComparison.OrdinalIgnoreCase));

                // Gespeicherte Werte sind bereits konsistent (siehe NormalizeMethods); zur Sicherheit gilt trotzdem: nur eine Tonart.
                var m = AppSettings.NormalizeMethods(p.Methods, (WinSoundBox.SelectedItem as string) ?? string.Empty, CustomPath.Text);
                ViaSound.IsChecked = (m & NotifyMethods.SoundDefault) != 0;
                ViaWindows.IsChecked = (m & NotifyMethods.SoundWindows) != 0;
                ViaCustom.IsChecked = (m & NotifyMethods.SoundCustom) != 0;
                UpdateEnabled();
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>Liest die Oberfläche als Profil; der Tonpfad wird dabei geprüft (ungültig = leer).</summary>
        public AlertProfile Read()
        {
            var methods = NotifyMethods.None;
            if (ViaToast.IsChecked == true) methods |= NotifyMethods.WindowsToast;
            if (ViaSound.IsChecked == true) methods |= NotifyMethods.SoundDefault;
            if (ViaWindows.IsChecked == true) methods |= NotifyMethods.SoundWindows;
            if (ViaCustom.IsChecked == true) methods |= NotifyMethods.SoundCustom;

            var windowsName = (WinSoundBox.SelectedItem as string) ?? string.Empty;
            var customPath = AppSettings.SanitizeSoundPath(CustomPath.Text);
            methods = AppSettings.NormalizeMethods(methods, windowsName, customPath);

            return new AlertProfile
            {
                ShowInTray = ShowInTray.IsChecked == true,
                YellowAt = (int)YellowSlider.Value,
                RedAt = (int)RedSlider.Value,
                CriticalAt = (int)CriticalSlider.Value,
                NotifyYellow = NotifyYellow.IsChecked == true,
                NotifyRed = NotifyRed.IsChecked == true,
                NotifyCritical = NotifyCritical.IsChecked == true,
                RepeatYellow = SelectedRepeat(RepeatYellow),
                RepeatRed = SelectedRepeat(RepeatRed),
                RepeatCritical = SelectedRepeat(RepeatCritical),
                NotifyOnStartup = WhenStartup.IsChecked == true,
                NotifyOnLevelChange = WhenLevel.IsChecked == true,
                Methods = methods,
                CustomSoundPath = customPath,
                WindowsSoundName = AppSettings.SanitizeWindowsSoundName(windowsName)
            };
        }

        private static int SelectedRepeat(ComboBox box)
            => box.SelectedItem is int n ? AlertProfile.ClampRepeat(n) : AlertProfile.MinRepeat;

        private void OnRepeatChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading)
                return;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void ShowValues()
        {
            YellowValue.Text = (int)YellowSlider.Value + "%";
            RedValue.Text = (int)RedSlider.Value + "%";
            CriticalValue.Text = (int)CriticalSlider.Value + "%";
        }

        private void UpdateEnabled()
        {
            var custom = ViaCustom.IsChecked == true;
            CustomPath.IsEnabled = custom;
            BrowseButton.IsEnabled = custom;
            WinSoundBox.IsEnabled = ViaWindows.IsChecked == true;
            // Die Anzahl ist nur wählbar, wenn die Stufe überhaupt gemeldet wird.
            RepeatYellow.IsEnabled = NotifyYellow.IsChecked == true;
            RepeatRed.IsEnabled = NotifyRed.IsChecked == true;
            RepeatCritical.IsEnabled = NotifyCritical.IsChecked == true;
            var invalid = custom && CustomPath.Text.Trim().Length > 0 && AppSettings.SanitizeSoundPath(CustomPath.Text).Length == 0;
            PathWarning.Visibility = invalid ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Hält die Reihenfolge kritisch &lt; rot &lt; gelb ein: Der bewegte Regler schiebt die anderen mit.</summary>
        private void OnSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_loading)
                return;
            _loading = true;
            try
            {
                if (sender == YellowSlider)
                {
                    if (RedSlider.Value >= YellowSlider.Value) RedSlider.Value = YellowSlider.Value - 1;
                    if (CriticalSlider.Value >= RedSlider.Value) CriticalSlider.Value = RedSlider.Value - 1;
                }
                else if (sender == RedSlider)
                {
                    if (YellowSlider.Value <= RedSlider.Value) YellowSlider.Value = RedSlider.Value + 1;
                    if (CriticalSlider.Value >= RedSlider.Value) CriticalSlider.Value = RedSlider.Value - 1;
                }
                else
                {
                    if (RedSlider.Value <= CriticalSlider.Value) RedSlider.Value = CriticalSlider.Value + 1;
                    if (YellowSlider.Value <= RedSlider.Value) YellowSlider.Value = RedSlider.Value + 1;
                }
                ShowValues();
            }
            finally
            {
                _loading = false;
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnChanged(object sender, RoutedEventArgs e)
        {
            if (_loading)
                return;
            UpdateEnabled();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnPathChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading)
                return;
            UpdateEnabled();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private bool PickFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = Loc.Get("notify.pickTitle"),
                Filter = Loc.Get("notify.fileFilter") + "|*.wav;*.mp3",
                CheckFileExists = true
            };
            if (dialog.ShowDialog() != true)
                return false;
            CustomPath.Text = dialog.FileName; // löst OnPathChanged aus
            return true;
        }

        private void OnBrowse(object sender, RoutedEventArgs e) => PickFile();

        /// <summary>
        /// Die drei Tonarten schließen sich aus: Wer eine ankreuzt, nimmt die anderen heraus (alle drei aus = kein eigener Ton).
        /// Eine Tonart ohne Auswahl (kein Windows-Ton gewählt, keine Datei angegeben) wird sofort wieder abgewählt.
        /// </summary>
        private void OnSoundClicked(object sender, RoutedEventArgs e)
        {
            if (_loading)
                return;

            var clicked = (CheckBox)sender;
            if (clicked.IsChecked == true)
            {
                foreach (var other in new[] { ViaSound, ViaWindows, ViaCustom })
                    if (!ReferenceEquals(other, clicked))
                        other.IsChecked = false;
            }
            UpdateEnabled();

            if (clicked.IsChecked == true && ReferenceEquals(clicked, ViaWindows) && WinSoundBox.SelectedItem == null)
                WinSoundBox.IsDropDownOpen = true; // Auswahl anbieten; ohne Auswahl wird der Haken in OnWinSoundClosed entfernt.
            else if (clicked.IsChecked == true && ReferenceEquals(clicked, ViaCustom) && AppSettings.SanitizeSoundPath(CustomPath.Text).Length == 0)
            {
                if (!PickFile())
                    ViaCustom.IsChecked = false;
                UpdateEnabled();
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnWinSoundSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_loading)
                return;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnWinSoundClosed(object sender, EventArgs e)
        {
            if (_loading || ViaWindows.IsChecked != true || WinSoundBox.SelectedItem != null)
                return;
            ViaWindows.IsChecked = false;
            UpdateEnabled();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnPathLostFocus(object sender, RoutedEventArgs e)
        {
            if (_loading || ViaCustom.IsChecked != true || CustomPath.Text.Trim().Length > 0)
                return;
            ViaCustom.IsChecked = false;
            UpdateEnabled();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnTest(object sender, RoutedEventArgs e) => TestRequested?.Invoke(this, Read());

        private void OnSimulation(object sender, RoutedEventArgs e) => SimulationRequested?.Invoke(this, EventArgs.Empty);

        private void OnHideSimulation(object sender, RoutedEventArgs e) => SimulationDismissRequested?.Invoke(this, EventArgs.Empty);

        private void OnReset(object sender, RoutedEventArgs e) => ResetRequested?.Invoke(this, EventArgs.Empty);

        private void OnRestore(object sender, RoutedEventArgs e) => RestoreRequested?.Invoke(this, EventArgs.Empty);
    }
}
