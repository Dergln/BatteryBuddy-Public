using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BatteryBuddy.App.Services;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.App.Views
{
    /// <summary>Welche Lizenz ein Eintrag hat (bestimmt Anzeige und Tooltip in der Spalte "Lizenz").</summary>
    internal enum LicenseKind { CcBy30, Flaticon, PublicDomain }

    /// <summary>Ein Eintrag der Beiträge-Liste. Der Text der Spalte "Lizenzdetails" kommt von der Plattform und wird nie übersetzt.</summary>
    internal sealed class Contribution
    {
        public string NameKey;                    // Übersetzbare Bezeichnung
        public Func<ImageSource> Icon;            // Vorhandenes App-Bild, keine weiteren Dateien laden
        public string NoteKey;                    // Optionaler übersetzbarer Zusatz (z. B. Hinweis auf Bearbeitung)
        public string TextBefore = string.Empty;  // Lizenzdetails: Text vor dem Link
        public string LinkText;                   // Lizenzdetails: Linktext (oder die URL selbst)
        public string LinkUrl;
        public string TextAfter = string.Empty;   // Lizenzdetails: Text nach dem Link
        public LicenseKind License;
    }

    /// <summary>Baut die Tabelle "Icon | Bezeichnung | Lizenzdetails | Lizenz" des Tabs "Beiträge".</summary>
    internal static class ContributionsView
    {
        // Neue Einträge hier ergänzen (siehe auch THIRD-PARTY-NOTICES.md im Repository).
        public static readonly IReadOnlyList<Contribution> All = new[]
        {
            new Contribution
            {
                NameKey = "contrib.gplv3",
                Icon = () => AppAssets.InfoImage("gpl-icon"),
                LinkText = "GPLv3 logo — Free Software Foundation / Wikimedia Commons",
                LinkUrl = "https://commons.wikimedia.org/wiki/File:GPLv3_Logo.svg",
                License = LicenseKind.PublicDomain
            },
            new Contribution
            {
                NameKey = "contrib.sound",
                Icon = AppAssets.FreesoundImage,
                TextBefore = "Electric Toothbrush dead batteries 1.wav by sangtao -- ",
                LinkText = "https://freesound.org/s/190925/", LinkUrl = "https://freesound.org/s/190925/",
                TextAfter = " -- License: Attribution 3.0",
                License = LicenseKind.CcBy30
            },
            new Contribution
            {
                NameKey = "contrib.buddy",
                Icon = AppAssets.AppIconImage,
                LinkText = "Electrician icons created by justicon - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/electrician",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.thunder",
                Icon = AppAssets.AppIconImage,
                LinkText = "Thunder icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/thunder",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.loading",
                Icon = AppAssets.LoadingImage,
                LinkText = "Loading icons created by Krystsina Mikhailouskaya - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/loading",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.confused",
                Icon = AppAssets.ConfusedBatteryImage,
                LinkText = "Battery icons created by Rohim - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/battery",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.mouse",
                Icon = () => CategoryImages.Get(DeviceCategory.Mouse),
                LinkText = "Device icons created by alekseyvanin - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/device",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.headphones",
                Icon = () => CategoryImages.Get(DeviceCategory.Headset),
                LinkText = "Headset icons created by meaicon - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/headset",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.earbuds",
                Icon = () => CategoryImages.Get(DeviceCategory.Earbuds),
                LinkText = "Headphone icons created by Kemalmoe - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/headphone",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.phone",
                Icon = () => CategoryImages.Get(DeviceCategory.Phone),
                LinkText = "Smartphone icons created by Roundicons - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/smartphone",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.internalBattery",
                Icon = () => CategoryImages.Get(DeviceCategory.InternalBattery),
                LinkText = "Low battery icons created by Us and Up - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/low-battery",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.unknown1",
                Icon = () => CategoryImages.Get(DeviceCategory.Other),
                LinkText = "Techonology icons created by Vitaly Gorbachev - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/techonology",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.unknown2",
                Icon = () => CategoryImages.Get(DeviceCategory.Other),
                LinkText = "Question icons created by Mzipi - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/question",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.speaker",
                Icon = () => CategoryImages.Get(DeviceCategory.Speaker),
                LinkText = "Speaker icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/speaker",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.keyboard",
                Icon = () => CategoryImages.Get(DeviceCategory.Keyboard),
                LinkText = "Typing icons created by meaicon - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/typing",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.pen",
                Icon = () => CategoryImages.Get(DeviceCategory.Pen),
                LinkText = "Pen icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/pen",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.gamepad",
                Icon = () => CategoryImages.Get(DeviceCategory.Gamepad),
                LinkText = "Joystick icons created by Good Ware - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/joystick",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.ai",
                Icon = () => AppAssets.OriginImage("ai"),
                LinkText = "Robot icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/robot",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.teamwork",
                Icon = () => AppAssets.OriginImage("teamwork"),
                LinkText = "Idea icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/idea",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.idea",
                Icon = () => AppAssets.OriginImage("idea"),
                LinkText = "Robot icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/robot",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.copyright",
                Icon = () => AppAssets.InfoImage("copyright"),
                LinkText = "Copyright icons created by Smashicons - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/copyright",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.for-free",
                Icon = () => AppAssets.InfoImage("for-free"),
                LinkText = "Free icons created by Three musketeers - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/free",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.license",
                Icon = () => AppAssets.InfoImage("license"),
                LinkText = "Guarantee icons created by Dewi Sari - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/guarantee",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.open-source",
                Icon = () => AppAssets.InfoImage("open-source"),
                LinkText = "Open source icons created by Smashicons - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/open-source",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.joke",
                Icon = () => AppAssets.InfoImage("joke-orig"),
                LinkText = "April fools day icons created by SBTS2018 - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/april-fools-day",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.lol",
                Icon = () => AppAssets.InfoImage("lol"),
                LinkText = "Joke icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/joke",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.pill",
                Icon = () => AppAssets.InfoImage("pill"),
                LinkText = "Capsule icons created by sonnycandra - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/capsule",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.feelGood",
                Icon = () => AppAssets.InfoImage("feel-good"),
                LinkText = "Optimistic icons created by Parzival’ 1997 - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/optimistic",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.paypal",
                Icon = () => AppAssets.InfoImage("paypal"),
                LinkText = "Paypal icons created by Pixel perfect - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/paypal",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.everybody",
                Icon = () => AppAssets.InfoImage("everybody"),
                LinkText = "Citizens icons created by Awicon - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/citizens",
                License = LicenseKind.Flaticon
            },
            new Contribution
            {
                NameKey = "contrib.bug",
                Icon = () => AppAssets.InfoImage("bug"),
                LinkText = "Virus icons created by Magnific - Flaticon", LinkUrl = "https://www.flaticon.com/free-icons/virus",
                License = LicenseKind.Flaticon
            }
        };

        private const string CcByUrl = "https://creativecommons.org/licenses/by/3.0/";

        public static void Build(Grid grid)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MinWidth = 150 });

            AddRow(grid, 0, Header(Loc.Get("contrib.colIcon")), Header(Loc.Get("contrib.colName")), Header(Loc.Get("contrib.colDetails")), Header(Loc.Get("contrib.colLicense")));
            // Alphabetisch nach dem angezeigten (übersetzten) Namen, mit den Sortierregeln der UI-Sprache.
            var sorted = Sorted(Loc.Code);
            for (var i = 0; i < sorted.Count; i++)
                AddRow(grid, i + 1, IconCell(sorted[i]), NameCell(sorted[i]), DetailsCell(sorted[i]), LicenseCell(sorted[i]));
        }

        internal static IReadOnlyList<Contribution> Sorted(string languageCode)
        {
            CultureInfo culture;
            try { culture = CultureInfo.GetCultureInfo(languageCode ?? string.Empty); }
            catch (CultureNotFoundException) { culture = CultureInfo.InvariantCulture; }
            var comparer = StringComparer.Create(culture, true);
            return All.OrderBy(c => Loc.Get(c.NameKey), comparer).ToList();
        }

        private static void AddRow(Grid grid, int row, params UIElement[] cells)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (var c = 0; c < cells.Length; c++)
            {
                var border = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(6, 6, 10, 6),
                    Child = cells[c]
                };
                border.SetResourceReference(Border.BorderBrushProperty, "BB.Border");
                Grid.SetRow(border, row);
                Grid.SetColumn(border, c);
                grid.Children.Add(border);
            }
        }

        private static UIElement Header(string text) => new TextBlock { Text = text, FontWeight = FontWeights.SemiBold };

        private static UIElement IconCell(Contribution c)
        {
            // 22 DIP entsprechen dem Motiv im Footer; Originale bleiben proportional und unveraendert.
            var image = new Image
            {
                Source = c.Icon() ?? throw new InvalidOperationException("Missing contribution image: " + c.NameKey),
                Width = 22,
                Height = 22,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            System.Windows.Automation.AutomationProperties.SetName(image, Loc.Get(c.NameKey));
            return image;
        }

        private static UIElement NameCell(Contribution c)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = Loc.Get(c.NameKey), TextWrapping = TextWrapping.Wrap });
            if (c.NoteKey != null)
            {
                var note = new TextBlock { Text = Loc.Get(c.NoteKey), TextWrapping = TextWrapping.Wrap, FontSize = 11, Margin = new Thickness(0, 2, 0, 0) };
                note.SetResourceReference(TextBlock.ForegroundProperty, "BB.Muted");
                panel.Children.Add(note);
            }
            return panel;
        }

        private static UIElement DetailsCell(Contribution c)
        {
            var text = new TextBlock { TextWrapping = TextWrapping.Wrap };
            if (c.TextBefore.Length > 0)
                text.Inlines.Add(new Run(c.TextBefore));

            var link = new Hyperlink(new Run(c.LinkText)) { NavigateUri = new Uri(c.LinkUrl) };
            link.SetResourceReference(TextElement.ForegroundProperty, "BB.Accent");
            link.RequestNavigate += (s, e) => { OpenUrl(e.Uri); e.Handled = true; };
            text.Inlines.Add(link);

            if (c.TextAfter.Length > 0)
                text.Inlines.Add(new Run(c.TextAfter));
            return text;
        }

        private static UIElement LicenseCell(Contribution c)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
            if (c.License == LicenseKind.CcBy30)
            {
                panel.Children.Add(CircleIcon(CcGlyph()));
                panel.Children.Add(CircleIcon(PersonGlyph()));
                panel.Children.Add(Label("CC BY 3.0"));
                panel.ToolTip = Loc.Get("contrib.license.ccby");
                panel.Cursor = Cursors.Hand;
                panel.MouseLeftButtonUp += (s, e) => OpenUrl(new Uri(CcByUrl));
            }
            else if (c.License == LicenseKind.PublicDomain)
            {
                panel.Children.Add(Label(Loc.Get("contrib.license.publicDomain")));
                panel.ToolTip = Loc.Get("contrib.license.publicDomain");
            }
            else
            {
                panel.Children.Add(CircleIcon(Glyph("F")));
                panel.Children.Add(Label("Flaticon"));
                panel.ToolTip = Loc.Get("contrib.license.flaticon");
            }
            SetAutomationName(panel, c);
            return panel;
        }

        // Screenreader: Die Symbole allein sagen nichts, deshalb bekommt die Zelle einen Namen.
        private static void SetAutomationName(DependencyObject o, Contribution c)
            => System.Windows.Automation.AutomationProperties.SetName(o,
                c.License == LicenseKind.CcBy30 ? "CC BY 3.0 - " + Loc.Get("contrib.license.ccby")
                : c.License == LicenseKind.PublicDomain ? Loc.Get("contrib.license.publicDomain")
                : "Flaticon - " + Loc.Get("contrib.license.flaticon"));

        private static TextBlock Label(string text)
            => new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };

        /// <summary>Kreis mit Inhalt, ähnlich den Lizenz-Symbolen von Creative Commons (selbst gezeichnet, keine Bilddatei nötig).</summary>
        private static UIElement CircleIcon(UIElement content)
        {
            var circle = new Border
            {
                Width = 22, Height = 22, CornerRadius = new CornerRadius(11), BorderThickness = new Thickness(1.5),
                Margin = new Thickness(0, 0, 2, 0), Child = content
            };
            circle.SetResourceReference(Border.BorderBrushProperty, "BB.Text");
            return circle;
        }

        private static UIElement Glyph(string text)
        {
            var t = new TextBlock { Text = text, FontSize = 10, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            t.SetResourceReference(TextBlock.ForegroundProperty, "BB.Text");
            return t;
        }

        private static UIElement CcGlyph() => Glyph("cc");

        /// <summary>Strichfigur für "Namensnennung" (BY): Kopf und Körper.</summary>
        private static UIElement PersonGlyph()
        {
            var canvas = new Canvas { Width = 19, Height = 19 };
            var head = new Ellipse { Width = 4.5, Height = 4.5 };
            var body = new Rectangle { Width = 7, Height = 8, RadiusX = 2, RadiusY = 2 };
            head.SetResourceReference(Shape.FillProperty, "BB.Text");
            body.SetResourceReference(Shape.FillProperty, "BB.Text");
            Canvas.SetLeft(head, 7.25); Canvas.SetTop(head, 2.5);
            Canvas.SetLeft(body, 6); Canvas.SetTop(body, 8);
            canvas.Children.Add(head);
            canvas.Children.Add(body);
            return canvas;
        }

        private static void OpenUrl(Uri uri)
            => ExternalLinkService.Open(uri);
    }
}
