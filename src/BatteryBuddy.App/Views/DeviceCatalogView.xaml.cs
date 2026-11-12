using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Navigation;
using BatteryBuddy.App.Services;
using BatteryBuddy.Core.Providers;

namespace BatteryBuddy.App.Views
{
    public sealed class DeviceCatalogRow
    {
        public string Name { get; set; }
        public string Connection { get; set; }
        public string Verification { get; set; }
        public IReadOnlyList<DeviceCatalogHardwareVerification> HardwareVerifications { get; set; }
        public string SourceName { get; set; }
        public string SourceUrl { get; set; }
        public IReadOnlyList<DeviceCatalogSource> Sources { get; set; }
        public string Comments { get; set; }
        public string Contributions => string.Join(", ", Sources.Select(source => source.DisplayName));
        public string VerificationSummary => HardwareVerifications.Count == 0
            ? Verification : string.Join(", ", HardwareVerifications.Select(verification => verification.Model));
        public string Summary => string.Join(", ", Name, Connection, VerificationSummary,
            Contributions, Comments);
    }

    public partial class DeviceCatalogView : UserControl
    {
        private string _sortProperty;
        private bool _descending;

        public DeviceCatalogView() => InitializeComponent();

        public void SetTexts()
        {
            FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            Intro.Text = Loc.Get("devicelist.intro");
            DeviceHeader.Text = Loc.Get("tab.devices");
            ConnectionHeader.Text = Loc.Get("devicelist.connection");
            VerificationHeader.Text = Loc.Get("devicelist.verified");
            ContributionHeader.Text = Loc.Get("tab.contrib");
            CommentsHeader.Text = Loc.Get("devicelist.comments");
            var selectedName = (CatalogList.SelectedItem as DeviceCatalogRow)?.Name;
            CatalogList.ItemsSource = DeviceCatalog.All.Select(e => new DeviceCatalogRow
            {
                Name = e.Name,
                Connection = e.Connection,
                Verification = e.HardwareVerifications.Count == 0 ? Loc.Get("devicelist.unverified") : string.Empty,
                HardwareVerifications = e.HardwareVerifications,
                SourceName = e.SourceName,
                SourceUrl = e.SourceUrl,
                Sources = e.Sources,
                Comments = e.CommentsKey == null ? "\u2014" : Loc.Get(e.CommentsKey)
            }).ToArray();
            ApplySort(selectedName);
            AutomationProperties.SetName(CatalogList, "DeviceList");
        }

        private void OnSort(object sender, RoutedEventArgs e)
        {
            var property = (string)((Button)sender).Tag;
            _descending = _sortProperty == property && !_descending;
            _sortProperty = property;
            ApplySort((CatalogList.SelectedItem as DeviceCatalogRow)?.Name);
        }

        private void ApplySort(string selectedName)
        {
            if (_sortProperty != null)
            {
                var comparer = StringComparer.Create(CultureInfo.GetCultureInfo(Loc.Code), true);
                var rows = CatalogList.Items.Cast<DeviceCatalogRow>();
                CatalogList.ItemsSource = (_descending
                    ? rows.OrderByDescending(SortValue, comparer)
                    : rows.OrderBy(SortValue, comparer))
                    .ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase).ToArray();
            }
            if (selectedName != null)
                CatalogList.SelectedItem = CatalogList.Items.Cast<DeviceCatalogRow>()
                    .FirstOrDefault(row => row.Name == selectedName);

            foreach (var button in new[] { DeviceSort, ConnectionSort, VerificationSort, ContributionSort, CommentsSort })
            {
                var panel = (DockPanel)button.Content;
                var indicator = (TextBlock)panel.Children[0];
                var caption = (TextBlock)panel.Children[1];
                var active = (string)button.Tag == _sortProperty;
                indicator.Text = active ? (_descending ? "\u2193" : "\u2191") : string.Empty;
                AutomationProperties.SetName(button, caption.Text);
                AutomationProperties.SetItemStatus(button, active ? (_descending ? "Z \u2192 A" : "A \u2192 Z") : string.Empty);
            }
        }

        private string SortValue(DeviceCatalogRow row)
        {
            switch (_sortProperty)
            {
                case nameof(DeviceCatalogRow.Name): return row.Name;
                case nameof(DeviceCatalogRow.Connection): return row.Connection;
                case nameof(DeviceCatalogRow.Verification): return row.VerificationSummary;
                case nameof(DeviceCatalogRow.Contributions): return row.Contributions;
                case nameof(DeviceCatalogRow.Comments): return row.Comments;
                default: throw new InvalidOperationException("Unknown DeviceList sort column.");
            }
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            ExternalLinkService.Open(e.Uri);
            e.Handled = true;
        }
    }
}
