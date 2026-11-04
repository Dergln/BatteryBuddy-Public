using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BatteryBuddy.App.Views
{
    internal static class AccessibleUi
    {
        internal static void Announce(UIElement target)
        {
            AutomationProperties.SetLiveSetting(target, AutomationLiveSetting.Polite);
            var peer = UIElementAutomationPeer.FromElement(target)
                ?? UIElementAutomationPeer.CreatePeerForElement(target);
            peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }

        internal static string FocusedItemId<T>(ListView list, Func<T, string> id)
        {
            var focused = Keyboard.FocusedElement as DependencyObject;
            var item = focused == null ? null : ItemsControl.ContainerFromElement(list, focused) as ListViewItem;
            return item?.Content is T row ? id(row) : null;
        }

        internal static void RestoreListFocus<T>(ListView list, string id, Func<T, string> key, Control fallback)
        {
            if (id == null)
                return;
            var row = list.Items.OfType<T>().FirstOrDefault(item => key(item) == id)
                ?? list.Items.OfType<T>().FirstOrDefault();
            if (row == null || !list.IsVisible)
            {
                fallback.Focus();
                return;
            }
            list.SelectedItem = row;
            list.ScrollIntoView(row);
            list.UpdateLayout();
            (list.ItemContainerGenerator.ContainerFromItem(row) as ListViewItem)?.Focus();
        }

        internal static void Name(Control control, string name, string help = null)
        {
            AutomationProperties.SetName(control, name);
            if (help != null)
                AutomationProperties.SetHelpText(control, help);
        }

        internal static void OpenInformation(Button source, string title, string text)
        {
            var owner = Window.GetWindow(source);
            var close = new Button
            {
                Content = Services.Loc.Get("button.close"),
                IsCancel = true,
                IsDefault = true,
                MinWidth = 90,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var body = new TextBox
            {
                Text = text,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(8),
                MinHeight = 100
            };
            Name(body, title);
            var panel = new DockPanel { Margin = new Thickness(16) };
            DockPanel.SetDock(close, Dock.Bottom);
            panel.Children.Add(close);
            panel.Children.Add(body);
            var area = SystemParameters.WorkArea;
            var dialog = new Window
            {
                Owner = owner,
                Title = title,
                Width = Math.Min(560, area.Width),
                Height = Math.Min(460, area.Height),
                MaxWidth = area.Width,
                MaxHeight = area.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                FlowDirection = source.FlowDirection,
                Content = panel,
                FontFamily = SystemFonts.MessageFontFamily,
                FontSize = SystemFonts.MessageFontSize
            };
            dialog.SetResourceReference(Control.BackgroundProperty, "BB.Window");
            dialog.SetResourceReference(Control.ForegroundProperty, "BB.Text");
            close.Click += (s, e) => dialog.Close();
            dialog.ContentRendered += (s, e) => body.Focus();
            Services.ThemeService.StyleTitleBar(dialog);
            dialog.ShowDialog();
            source.Focus();
        }
    }
}
