using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BatteryBuddy.App.Services;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.App.Views
{
    public sealed class DiagnosticRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public ImageSource CategoryImage { get; set; }
        public string Glyph { get; set; }
        public string Connection { get; set; }
        public string Battery { get; set; }
        public string Explanation { get; set; }
        public string Summary => string.Join(", ", CategoryName, Name, Connection, Battery, Explanation);
    }

    public partial class DeviceBuddyView : UserControl
    {
        public event EventHandler RefreshRequested;

        public DeviceBuddyView()
        {
            InitializeComponent();
        }

        public void SetTexts()
        {
            FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            Heading.Text = Loc.Get("devicebuddy.heading");
            var intro = Loc.Get("devicebuddy.intro").Split(new[] { "{0}" }, StringSplitOptions.None);
            IntroBefore.Text = intro[0];
            IntroEmphasis.Text = Loc.Get("devicebuddy.intro.emphasis");
            IntroAfter.Text = intro.Length > 1 ? intro[1] : string.Empty;
            PurposeText.Text = Loc.Get("devicebuddy.purpose");
            ReportText.Text = Loc.Format("devicebuddy.report", Loc.Get("footer.bug"));
            RefreshButton.Content = Loc.Get("devices.refresh");
            NameHeader.Text = Loc.Get("devices.colName");
            ConnectionHeader.Text = Loc.Get("devicebuddy.connection");
            BatteryHeader.Text = Loc.Get("devicebuddy.battery");
            HintHeader.Text = Loc.Get("devicebuddy.hint");
            System.Windows.Automation.AutomationProperties.SetName(DiagnosticList, "DeviceBuddy");
        }

        public void UpdateSnapshot(MonitorSnapshot snapshot)
        {
            var focusedId = AccessibleUi.FocusedItemId<DiagnosticRow>(DiagnosticList, r => r.Id);
            var selectedId = (DiagnosticList.SelectedItem as DiagnosticRow)?.Id;
            var listHadFocus = DiagnosticList.IsKeyboardFocusWithin;
            var previousFailure = FailedText.Text;
            var complete = snapshot?.IsComplete == true;
            var rows = (snapshot?.Diagnostics ?? new DeviceDiagnostic[0]).OrderBy(d => d.Category).ThenBy(d => d.Name)
                .Select(d => new DiagnosticRow
                {
                    Id = d.Id,
                    Name = d.Id.StartsWith("xinput:", StringComparison.Ordinal) &&
                        uint.TryParse(d.Id.Substring(7), out var slot)
                        ? Loc.Format("devicebuddy.controller", slot + 1) : d.Name,
                    CategoryName = Loc.Get("category." + d.Category),
                    CategoryImage = CategoryImages.Get(d.Category),
                    Glyph = CategoryGlyphs.Get(d.Category),
                    Connection = Loc.Get("devicebuddy.connection." + d.Connection),
                    Battery = Loc.Get(d.Reason == BatteryDiagnosticReason.ReportedWired ? "devicebuddy.reportedWired" :
                        d.Reason == BatteryDiagnosticReason.BatteryQueryFailed ? "devicebuddy.readFailed" : "devicebuddy.noReading"),
                    Explanation = Loc.Get("devicebuddy.reason." + d.Reason)
                }).ToList();
            DiagnosticList.ItemsSource = rows;
            DiagnosticList.SelectedItem = rows.FirstOrDefault(row => row.Id == selectedId);
            UpdatedText.Text = complete ? Loc.Format("devices.updated", snapshot.Time.LocalDateTime.ToString("T")) :
                Loc.Get("devicebuddy.scanning");
            var failed = snapshot?.FailedProviders;
            var hasFailures = failed != null && failed.Count > 0;
            FailedText.Text = hasFailures ? Loc.Format("devicebuddy.failed", string.Join(", ", failed)) : string.Empty;
            FailedText.Visibility = hasFailures ? Visibility.Visible : Visibility.Collapsed;
            EmptyText.Text = Loc.Get(!complete ? "devicebuddy.scanning" : hasFailures ? "devicebuddy.incomplete" : "devicebuddy.empty");
            EmptyText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DiagnosticList.Visibility = rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            AccessibleUi.RestoreListFocus<DiagnosticRow>(DiagnosticList, focusedId, r => r.Id, RefreshButton);
            if (listHadFocus && rows.Count == 0)
                RefreshButton.Focus();
            if (hasFailures && previousFailure != FailedText.Text)
                AccessibleUi.Announce(FailedText);
        }

        private void OnRefresh(object sender, RoutedEventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
    }
}
