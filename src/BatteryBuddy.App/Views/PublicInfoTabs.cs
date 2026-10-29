using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using BatteryBuddy.App.Services;

namespace BatteryBuddy.App.Views
{
    public sealed class LicenseTabView : FlowDocumentScrollViewer
    {
        public LicenseTabView()
        {
            IsToolBarVisible = false;
            IsSelectionEnabled = true;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Background = Brushes.Transparent;
            SizeChanged += (_, args) => { if (args.WidthChanged) SetTexts(); };
            SetTexts();
        }

        public void SetTexts() => Document = PublicInfoDocuments.License(ActualWidth);
    }

    public sealed class ProVersionTabView : FlowDocumentScrollViewer
    {
        public ProVersionTabView()
        {
            IsToolBarVisible = false;
            IsSelectionEnabled = true;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            Background = Brushes.Transparent;
            SetTexts();
        }

        public void SetTexts() => Document = PublicInfoDocuments.ProVersion();
    }

    internal static class PublicInfoDocuments
    {
        private const string ProfileUrl = "https://github.com/Dergln";
        private const string JokeUrl = "https://www.youtube.com/watch?v=zE7PKRjrid4";
        private const string PayPalUrl = "https://www.paypal.com/paypalme/Dergln";

        public static FlowDocument License(double availableWidth = 0)
        {
            var document = CreateDocument();
            var section = new Section();
            var statement = GplStatementParagraph();
            statement.Inlines.InsertBefore(statement.Inlines.FirstInline, LicenseArtwork(availableWidth));
            section.Blocks.Add(statement);
            section.Blocks.Add(Heading("license.copyright.title"));
            section.Blocks.Add(CopyrightParagraph());
            section.Blocks.Add(Heading("license.tldr.title"));
            section.Blocks.Add(SummaryList());
            section.Blocks.Add(Paragraph("license.thirdparty"));
            document.Blocks.Add(section);
            // Separate sections prevent the first floating image from leaking into the next text block.
            var explanation = new Section
            {
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0, 16, 0, 0),
                Margin = new Thickness(0, 16, 0, 0)
            };
            explanation.SetResourceReference(Block.BorderBrushProperty, "BB.Border");
            var purpose = Paragraph("license.purpose");
            purpose.Inlines.InsertBefore(purpose.Inlines.FirstInline, PrinciplesArtwork(availableWidth));
            explanation.Blocks.Add(purpose);
            explanation.Blocks.Add(Paragraph("license.explore"));
            explanation.Blocks.Add(Paragraph("license.audit"));
            document.Blocks.Add(explanation);
            return document;
        }

        public static FlowDocument ProVersion()
        {
            var document = CreateDocument();
            var section = new Section();
            var headline = Heading("pro.headline");
            headline.Inlines.InsertBefore(headline.Inlines.FirstInline, ProArtwork());
            headline.Margin = new Thickness(0, 0, 0, 4);
            section.Blocks.Add(headline);
            section.Blocks.Add(Paragraph("pro.intro"));
            section.Blocks.Add(Heading("pro.why.title"));
            section.Blocks.Add(WhyParagraph());
            section.Blocks.Add(Heading("pro.doubt.title"));
            section.Blocks.Add(Paragraph("pro.doubt.body"));
            section.Blocks.Add(Heading("pro.serious.title"));
            section.Blocks.Add(Paragraph("pro.serious.body1"));
            section.Blocks.Add(EmphasisParagraph("pro.serious.body2.before", "pro.serious.body2.italic", "pro.serious.body2.after"));
            section.Blocks.Add(ContributionParagraph());
            section.Blocks.Add(Paragraph("pro.serious.closing"));
            section.Blocks.Add(Heading("pro.support.title"));
            section.Blocks.Add(SupportParagraph());
            section.Blocks.Add(ClosingParagraph());
            document.Blocks.Add(section);
            return document;
        }

        private static FlowDocument CreateDocument()
        {
            var document = new FlowDocument
            {
                PagePadding = new Thickness(12),
                FlowDirection = Loc.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                FontFamily = SystemFonts.MessageFontFamily,
                FontSize = SystemFonts.MessageFontSize,
                TextAlignment = Loc.IsRtl ? TextAlignment.Right : TextAlignment.Left
            };
            document.SetResourceReference(TextElement.ForegroundProperty, "BB.Text");
            return document;
        }

        private static Floater LicenseArtwork(double availableWidth)
        {
            var imageWidth = ArtworkWidth(availableWidth);
            var imageHeight = imageWidth * 637 / 1280;
            var image = CreateImage("gplv3", imageWidth, imageHeight);
            AutomationProperties.SetName(image, "GPLv3");

            return FloatRight(image, imageWidth);
        }

        private static double ArtworkWidth(double availableWidth)
            // DIP scale with Windows DPI; the full-resolution embedded source stays sharp.
            => availableWidth > 0 ? Math.Min(330, Math.Max(1, availableWidth - 48) * 0.4) : 330;

        private static Floater PrinciplesArtwork(double availableWidth)
        {
            var width = ArtworkWidth(availableWidth) * 0.8;
            var grid = new Grid { Width = width, Height = width };
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var names = new[] { "for-free", "open-source", "license", "copyright" };
            for (var i = 0; i < names.Length; i++)
            {
                var image = CreateImage(names[i], width * 0.38, width * 0.38);
                AutomationProperties.SetName(image, Loc.Get("contrib." + names[i]));
                var cell = new Border
                {
                    BorderThickness = new Thickness(i % 2 == 1 ? 1 : 0, i >= 2 ? 1 : 0, 0, 0),
                    Child = image
                };
                cell.SetResourceReference(Border.BorderBrushProperty, "BB.Border");
                Grid.SetRow(cell, i / 2);
                Grid.SetColumn(cell, i % 2);
                grid.Children.Add(cell);
            }
            return FloatRight(grid, width);
        }

        private static Floater ProArtwork()
        {
            var image = CreateImage("joke", 150, 198);
            AutomationProperties.SetName(image, Loc.Get("contrib.joke"));
            return FloatRight(image, 150);
        }

        private static Floater FloatRight(UIElement content, double width)
        {
            // In RTL wandert das Bild nach links, damit der Textfluss an der Leserichtung ausgerichtet bleibt.
            var container = new Floater
            {
                Width = width,
                Padding = new Thickness(0),
                Margin = new Thickness(Loc.IsRtl ? 0 : 12, 0, Loc.IsRtl ? 12 : 0, 10),
                HorizontalAlignment = Loc.IsRtl ? HorizontalAlignment.Left : HorizontalAlignment.Right
            };
            container.Blocks.Add(new BlockUIContainer(content) { Margin = new Thickness(0) });
            return container;
        }

        private static Image CreateImage(string name, double width, double height)
        {
            var image = new Image
            {
                Source = AppAssets.InfoImage(name),
                Width = width,
                Height = height,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            return image;
        }

        private static Paragraph Heading(string key)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 8, 0, 4),
                KeepWithNext = true
            };
            paragraph.Inlines.Add(new Run(Loc.Get(key))
            {
                FontSize = SystemFonts.MessageFontSize + 2,
                FontWeight = FontWeights.SemiBold
            });
            return paragraph;
        }

        private static Paragraph Paragraph(string key)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            AddTextWithLineBreak(paragraph, Loc.Get(key));
            return paragraph;
        }

        private static void AddTextWithLineBreak(Paragraph paragraph, string text)
        {
            var parts = text.Split(new[] { "{0}" }, StringSplitOptions.None);
            if (parts.Length > 2)
                throw new InvalidOperationException("The paragraph contains more than one line-break placeholder.");
            paragraph.Inlines.Add(new Run(parts[0]));
            if (parts.Length == 2)
            {
                paragraph.Inlines.Add(new LineBreak());
                paragraph.Inlines.Add(new Run(parts[1]));
            }
        }

        private static Paragraph GplStatementParagraph()
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            paragraph.Inlines.Add(new Run(Loc.Get("license.gpl.statement.before")));
            paragraph.Inlines.Add(new Run(" "));
            paragraph.Inlines.Add(Link("GPLv3", "https://www.gnu.org/licenses/gpl-3.0.en.html"));
            paragraph.Inlines.Add(new Run(Loc.Get("license.gpl.statement.after")));
            return paragraph;
        }

        private static Paragraph CopyrightParagraph()
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            paragraph.Inlines.Add(new Run("-"));
            paragraph.Inlines.Add(new Run(" "));
            paragraph.Inlines.Add(Link(ProfileUrl, ProfileUrl));
            return paragraph;
        }

        private static List SummaryList()
        {
            var list = new List
            {
                MarkerStyle = TextMarkerStyle.Disc,
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(18, 0, 0, 0)
            };
            foreach (var key in new[]
            {
                "license.tldr.private",
                "license.tldr.source",
                "license.tldr.commercial"
            })
            {
                list.ListItems.Add(new ListItem(
                    new Paragraph(new Run(Loc.Get(key))) { Margin = new Thickness(0, 0, 0, 6) }));
            }
            var sourceItem = list.ListItems.FirstListItem.NextListItem;
            var details = Paragraph("license.tldr.derivative");
            var nested = new List
            {
                MarkerStyle = TextMarkerStyle.Disc,
                Margin = new Thickness(0),
                Padding = new Thickness(18, 0, 0, 0)
            };
            nested.ListItems.Add(new ListItem(details));
            sourceItem.Blocks.Add(nested);
            var reference = new Paragraph { Margin = new Thickness(0, 0, 0, 6) };
            var link = Link(string.Empty,
                "https://www.heise.de/hintergrund/Chancen-und-Risiken-Copyleft-in-der-Softwareentwicklung-4926808.html?seite=2");
            link.Inlines.Clear();
            // A localized template keeps the publisher's name italic without imposing English word order.
            var parts = Loc.Get("license.tldr.reference").Split(new[] { "{0}" }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new InvalidOperationException("The license reference must contain exactly one publisher placeholder.");
            link.Inlines.Add(new Run(parts[0]));
            link.Inlines.Add(new Run("heise online") { FontStyle = FontStyles.Italic });
            link.Inlines.Add(new Run(parts[1]));
            reference.Inlines.Add(link);
            nested.ListItems.Add(new ListItem(reference));
            return list;
        }

        private static Paragraph EmphasisParagraph(string beforeKey, string italicKey, string afterKey)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            paragraph.Inlines.Add(new Run(Loc.Get(beforeKey)));
            paragraph.Inlines.Add(new Run(" "));
            paragraph.Inlines.Add(new Run(Loc.Get(italicKey)) { FontStyle = FontStyles.Italic });
            var after = Loc.Get(afterKey);
            // Language files trim outer whitespace, so spacing cannot depend on a leading blank.
            if (after.Length > 0 && char.IsLetterOrDigit(after[0]) &&
                Loc.Code != "ja" && !Loc.Code.StartsWith("zh", StringComparison.Ordinal))
                paragraph.Inlines.Add(new Run(" "));
            AddTextWithLineBreak(paragraph, after);
            return paragraph;
        }

        private static Paragraph ClosingParagraph()
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            var parts = Loc.Get("pro.support.closing").Split(new[] { "{0}" }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new InvalidOperationException("The closing paragraph must contain one picture placeholder.");
            paragraph.Inlines.Add(new Run(parts[0]));
            AddInlineImage(paragraph, "feel-good");
            paragraph.Inlines.Add(new Run(parts[1]));
            return paragraph;
        }

        private static Paragraph ContributionParagraph()
        {
            // Translate the whole paragraph so grammar and emphasis need not follow English word order.
            var template = Loc.Get("pro.serious.body3");
            foreach (var marker in new[] { "{0}", "{1}", "{2}" })
                if (template.IndexOf(marker, StringComparison.Ordinal) < 0 ||
                    template.IndexOf(marker, StringComparison.Ordinal) != template.LastIndexOf(marker, StringComparison.Ordinal))
                    throw new InvalidOperationException("The contribution paragraph requires each formatting marker exactly once.");
            if (template.IndexOf("{1}", StringComparison.Ordinal) > template.IndexOf("{2}", StringComparison.Ordinal))
                throw new InvalidOperationException("The contribution paragraph has reversed emphasis markers.");

            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            var italic = false;
            foreach (var part in Regex.Split(template, @"(\{[012]\})"))
            {
                if (part == "{0}") paragraph.Inlines.Add(new LineBreak());
                else if (part == "{1}") italic = true;
                else if (part == "{2}") italic = false;
                else paragraph.Inlines.Add(new Run(part) { FontStyle = italic ? FontStyles.Italic : FontStyles.Normal });
            }
            return paragraph;
        }

        private static Paragraph WhyParagraph()
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            AddTextWithLineBreak(paragraph, Loc.Get("pro.why.body"));
            paragraph.Inlines.Add(new Run(" "));
            paragraph.Inlines.Add(new Run(Loc.Get("pro.why.body.italic")) { FontStyle = FontStyles.Italic });
            return paragraph;
        }

        private static Paragraph SupportParagraph()
        {
            var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 9) };
            AddInlineImage(paragraph, "pill");
            paragraph.Inlines.Add(new Run(" "));
            paragraph.Inlines.Add(Link(Loc.Get("pro.support.youtube"), JokeUrl));
            paragraph.Inlines.Add(new Run(" "));
            AddSupportAction(paragraph);
            return paragraph;
        }

        private static void AddSupportAction(Paragraph paragraph)
        {
            // One localized template lets each language place the icons and PayPal link naturally.
            var template = Loc.Get("pro.support.action");
            var markers = new[] { "{0}", "{1}" };
            var used = new bool[markers.Length];
            var position = 0;

            while (position < template.Length)
            {
                var nextPosition = -1;
                var markerIndex = -1;
                for (var i = 0; i < markers.Length; i++)
                {
                    var candidate = template.IndexOf(markers[i], position, StringComparison.Ordinal);
                    if (candidate >= 0 && (nextPosition < 0 || candidate < nextPosition))
                    {
                        nextPosition = candidate;
                        markerIndex = i;
                    }
                }

                if (nextPosition < 0)
                {
                    paragraph.Inlines.Add(new Run(template.Substring(position)));
                    break;
                }

                if (used[markerIndex])
                    throw new InvalidOperationException("The support translation contains a duplicate placeholder.");
                if (nextPosition > position)
                    paragraph.Inlines.Add(new Run(template.Substring(position, nextPosition - position)));

                used[markerIndex] = true;
                if (markerIndex == 0)
                    AddInlineImage(paragraph, "everybody");
                else
                {
                    AddInlineImage(paragraph, "paypal");
                    paragraph.Inlines.Add(new Run(" "));
                    paragraph.Inlines.Add(Link("PayPal", PayPalUrl));
                }
                position = nextPosition + markers[markerIndex].Length;
            }

            if (!used[0] || !used[1])
                throw new InvalidOperationException("The support translation is missing a required placeholder.");
        }

        private static void AddInlineImage(Paragraph paragraph, string name)
        {
            var image = CreateImage(name, 20, 20);
            var labelKey = name == "feel-good" ? "contrib.feelGood"
                : "contrib." + name;
            AutomationProperties.SetName(image, Loc.Get(labelKey));
            paragraph.Inlines.Add(new InlineUIContainer(image) { BaselineAlignment = BaselineAlignment.Center });
        }

        private static Hyperlink Link(string text, string address)
        {
            var link = new Hyperlink(new Run(text)) { NavigateUri = new Uri(address, UriKind.Absolute) };
            link.SetResourceReference(TextElement.ForegroundProperty, "BB.Accent");
            link.RequestNavigate += (sender, args) =>
            {
                ExternalLinkService.Open(args.Uri);
                args.Handled = true;
            };
            return link;
        }
    }
}
