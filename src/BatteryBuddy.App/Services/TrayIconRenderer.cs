using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BatteryBuddy.Core.Logic;
using BatteryBuddy.Core.Models;
using Microsoft.Win32;

namespace BatteryBuddy.App.Services
{
    /// <summary>Ausgewählte Segoe-MDL2-Symbole je Gerätekategorie; die Codepunkte sind auch in der Windows-10-Systemschrift vorhanden.</summary>
    public static class CategoryGlyphs
    {
        public const string FontName = "Segoe MDL2 Assets";

        public static string Get(DeviceCategory category)
        {
            switch (category)
            {
                case DeviceCategory.Headset: return "\uE95B";
                case DeviceCategory.Earbuds: return "\uE95B";
                case DeviceCategory.Mouse: return "\uE962";
                case DeviceCategory.Keyboard: return "\uE765";
                case DeviceCategory.Gamepad: return "\uE7FC";
                case DeviceCategory.Phone: return "\uE8EA";
                case DeviceCategory.Speaker: return "\uE7F5";
                case DeviceCategory.Pen: return "\uE76D";
                case DeviceCategory.InternalBattery: return "\uEBAA";
                default: return "\uE897";
            }
        }
    }

    /// <summary>Ein zusaetzliches Tray-Symbol: Kategorie oben, Prozentwert darunter.</summary>
    public static class TrayIconRenderer
    {
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        /// <summary>Hellt die Taskleiste hell? Dann braucht das Symbol dunkle Farben, sonst ist es nicht lesbar.</summary>
        public static bool TaskbarIsLight()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    return key?.GetValue("SystemUsesLightTheme") is int v && v == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Color LevelColor(ChargeLevel level, bool light)
        {
            switch (level)
            {
                case ChargeLevel.Green: return light ? Color.FromArgb(16, 124, 16) : Color.FromArgb(108, 224, 108);
                case ChargeLevel.Yellow: return light ? Color.FromArgb(157, 93, 0) : Color.FromArgb(255, 209, 64);
                case ChargeLevel.Red: return light ? Color.FromArgb(196, 43, 28) : Color.FromArgb(255, 99, 88);
                case ChargeLevel.Critical: return light ? Color.FromArgb(128, 0, 32) : Color.FromArgb(236, 70, 110); // burgunderrot
                default: return light ? Color.FromArgb(96, 96, 96) : Color.FromArgb(180, 180, 180);
            }
        }

        /// <summary>Erzeugt ein neues Icon. Der Aufrufer muss es nach Gebrauch mit Dispose freigeben.</summary>
        public static Icon Render(TrayEntry entry, bool light)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            var size = Math.Min(64, Math.Max(32, SystemInformation.SmallIconSize.Width * 2));
            using (var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var image = CategoryImages.LoadTrimmedBitmap(entry.Category) ?? GlyphBitmap(entry.Category, size, light))
            using (var number = new Bitmap(size * 4, size * 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(number))
                using (var font = new Font("Segoe UI", size * 1.4f, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(LevelColor(entry.Level, light)))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.DrawString(entry.Percent.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        font, brush, 0, 0, StringFormat.GenericTypographic);
                }
                // Schrift-Leerraum ebenfalls abschneiden: Motiv und Ziffern nutzen ihre halbe Hoehe wirklich aus.
                using (var text = CategoryImages.Trim(number, size * 4))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    CategoryImages.Draw(g, image, new RectangleF(0, 0, size, size * 0.46f));
                    CategoryImages.Draw(g, text, new RectangleF(0, size * 0.5f, size, size * 0.5f));
                }
                return CreateIcon(bmp);
            }
        }

        private static Bitmap GlyphBitmap(DeviceCategory category, int size, bool light)
        {
            using (var source = new Bitmap(size * 2, size * 2))
            {
                using (var g = Graphics.FromImage(source))
                using (var font = new Font(CategoryGlyphs.FontName, size, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(light ? Color.Black : Color.White))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.DrawString(CategoryGlyphs.Get(category), font, brush, 0, 0, StringFormat.GenericTypographic);
                }
                return CategoryImages.Trim(source, size * 2);
            }
        }

        /// <summary>Supplied category artwork, or the existing glyph for categories without an image.</summary>
        public static Icon RenderNotification(DeviceCategory category)
        {
            var size = SystemInformation.IconSize.Width;
            using (var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                using (var background = new SolidBrush(Color.FromArgb(0, 120, 180)))
                using (var foreground = new SolidBrush(Color.White))
                using (var font = new Font(CategoryGlyphs.FontName, size * 0.65f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (var format = new StringFormat(StringFormat.GenericTypographic)
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    graphics.Clear(Color.Transparent);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    using (var image = CategoryImages.LoadBitmap(category))
                    {
                        if (image != null)
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            CategoryImages.Draw(graphics, image, new RectangleF(0, 0, size, size));
                        }
                        else
                        {
                            graphics.FillEllipse(background, 1, 1, size - 2, size - 2);
                            graphics.DrawString(CategoryGlyphs.Get(category), font, foreground,
                                new RectangleF(0, 0, size, size), format);
                        }
                    }
                }
                return CreateIcon(bitmap);
            }
        }

        internal static Icon CreateIcon(Bitmap bitmap)
        {
            var handle = bitmap.GetHicon();
            try
            {
                using (var temp = Icon.FromHandle(handle))
                    return (Icon)temp.Clone();
            }
            finally
            {
                DestroyIcon(handle);
            }
        }

    }
}
