using System.Windows;
using System.Windows.Controls;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App.Views
{
    /// <summary>
    /// Kleiner Ja/Nein(/Abbrechen)-Dialog im Look der App. Das Windows-MessageBox wäre englisch/in Systemsprache
    /// und im dunklen Theme hell; hier kommen Texte und Farben aus der App.
    /// </summary>
    public static class ConfirmDialog
    {
        /// <summary>true = erste Taste, false = zweite Taste, null = Abbrechen (Escape/X oder dritte Taste).</summary>
        public static bool? Ask(Window owner, string title, string text, string yes, string no, string cancel)
        {
            bool? result = null;
            var dialog = new Window
            {
                Title = title,
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Height,
                Width = 440,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
            };
            dialog.SetResourceReference(Control.BackgroundProperty, "BB.Window");
            dialog.SetResourceReference(Control.ForegroundProperty, "BB.Text");
            ThemeService.StyleTitleBar(dialog);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 18, 0, 0) };
            Button Make(string label, bool? value, bool isDefault, bool isCancel)
            {
                var b = new Button { Content = label, MinWidth = 90, Padding = new Thickness(14, 5, 14, 5), Margin = new Thickness(8, 0, 0, 0), IsDefault = isDefault, IsCancel = isCancel };
                b.Click += (s, e) => { result = value; dialog.DialogResult = true; };
                return b;
            }

            buttons.Children.Add(Make(yes, true, true, false));
            buttons.Children.Add(Make(no, false, false, cancel == null));
            if (cancel != null)
                buttons.Children.Add(Make(cancel, null, false, true));

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(buttons);
            dialog.Content = panel;

            dialog.ShowDialog();
            return result;
        }
    }
}
