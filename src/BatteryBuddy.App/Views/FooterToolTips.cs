using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App.Views
{
    internal static class FooterToolTips
    {
        internal static ToolTip Text(params string[] paragraphs)
        {
            var content = new StackPanel();
            foreach (var paragraph in paragraphs)
                content.Children.Add(Paragraph(paragraph, content.Children.Count > 0));
            return Wrap(content);
        }

        internal static ToolTip Version(string version)
        {
            var content = new StackPanel();
            content.Children.Add(Paragraph(Loc.Format("origin.versionCurrent", version), false));
            var diagram = new Grid { Margin = new Thickness(0, 10, 0, 0), FlowDirection = FlowDirection.LeftToRight };
            diagram.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            diagram.ColumnDefinitions.Add(new ColumnDefinition());
            AddVersionRow(diagram, "x.x.x.x", string.Empty);
            AddVersionRow(diagram, "| | | +-->", Loc.Get("origin.versionRevision"));
            AddVersionRow(diagram, "| | +---->", Loc.Get("origin.versionPatch"));
            AddVersionRow(diagram, "| +------>", Loc.Get("origin.versionMinor"));
            AddVersionRow(diagram, "+-------->", Loc.Get("origin.versionMajor"));
            content.Children.Add(diagram);
            return Wrap(content);
        }

        private static void AddVersionRow(Grid grid, string arrow, string description)
        {
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var prefix = Paragraph(arrow, false);
            prefix.FontFamily = new FontFamily("Consolas");
            prefix.TextWrapping = TextWrapping.NoWrap;
            prefix.Margin = new Thickness(0, 0, 8, 4);
            Grid.SetRow(prefix, row);
            grid.Children.Add(prefix);
            var text = Paragraph(description, false);
            text.FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            text.Margin = new Thickness(0, 0, 0, 4);
            Grid.SetRow(text, row);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
        }

        private static TextBlock Paragraph(string text, bool separated)
            => new TextBlock
            {
                // Die kleinen Footer-Labels haben einen eigenen Stil; der gilt nicht fuer lange Infotexte.
                Style = null,
                Text = text,
                FontFamily = SystemFonts.MessageFontFamily,
                FontSize = SystemFonts.MessageFontSize,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, separated ? 10 : 0, 0, 0)
            };

        private static ToolTip Wrap(FrameworkElement content)
            => new ToolTip
            {
                FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                Content = new ScrollViewer
                {
                    Width = Math.Max(1, Math.Min(500, SystemParameters.WorkArea.Width - 48)),
                    MaxHeight = Math.Max(1, Math.Min(480, SystemParameters.WorkArea.Height - 48)),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = content
                }
            };

        internal static void Set(FrameworkElement target, ToolTip tooltip)
        {
            // Am Rahmen statt am Mauszeiger verankern: grosse Zeiger duerfen den ersten Text nicht verdecken.
            tooltip.PlacementTarget = target;
            tooltip.Placement = PlacementMode.Custom;
            tooltip.CustomPopupPlacementCallback = Placements;
            target.ToolTip = tooltip;
            ToolTipService.SetShowDuration(target, 120000);
            var scroll = (ScrollViewer)tooltip.Content;
            AutomationProperties.SetHelpText(target, string.Join(Environment.NewLine, Texts((Panel)scroll.Content)));
        }

        internal static CustomPopupPlacement[] Placements(Size popup, Size target, Point offset)
        {
            // Windows kann nach unten ausweichen, wenn oberhalb zu wenig Platz ist.
            // Auch dort genug Luft fuer die eingestellte Zeigergroesse lassen (WPF-Koordinaten sind DIP).
            var gap = Math.Max(16, SystemParameters.CursorHeight + 8);
            return new[]
            {
                new CustomPopupPlacement(new Point(0, -popup.Height - gap), PopupPrimaryAxis.Horizontal),
                new CustomPopupPlacement(new Point(0, target.Height + gap), PopupPrimaryAxis.Horizontal)
            };
        }

        private static IEnumerable<string> Texts(Panel panel)
            => panel.Children.Cast<UIElement>().SelectMany(child => child is TextBlock text
                ? new[] { text.Text } : child is Panel nested ? Texts(nested) : Enumerable.Empty<string>());
    }
}
