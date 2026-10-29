using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BatteryBuddy.Core.Models;

namespace BatteryBuddy.App.Services
{
    /// <summary>The same embedded category artwork is used by WPF, tray rendering and notifications.</summary>
    internal static class CategoryImages
    {
        private static readonly Dictionary<DeviceCategory, ImageSource> Images = new Dictionary<DeviceCategory, ImageSource>();

        private static string ResourceName(DeviceCategory category)
        {
            switch (category)
            {
                case DeviceCategory.Mouse: return "Assets.mouse.png";
                case DeviceCategory.Headset: return "Assets.headphones.png";
                case DeviceCategory.Earbuds: return "Assets.earbuds.png";
                case DeviceCategory.Phone: return "Assets.smartphone.png";
                case DeviceCategory.InternalBattery: return "Assets.battery-charge.png";
                case DeviceCategory.Other: return "Assets.device-unknown.png";
                case DeviceCategory.Speaker: return "Assets.speaker.png";
                case DeviceCategory.Keyboard: return "Assets.keyboard.png";
                case DeviceCategory.Pen: return "Assets.pen-tip.png";
                case DeviceCategory.Gamepad: return "Assets.gamepad.png";
                default: return null;
            }
        }

        private static Stream Open(string name)
            => Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
                ?? throw new InvalidOperationException("Missing embedded category image: " + name);

        public static ImageSource Get(DeviceCategory category)
        {
            var name = ResourceName(category);
            if (name == null)
                return null; // Categories without supplied artwork retain their existing glyph.
            lock (Images)
            {
                if (!Images.TryGetValue(category, out var image))
                {
                    using (var stream = Open(name))
                    {
                        var frame = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        frame.Freeze();
                        Images.Add(category, image = frame);
                    }
                }
                return image;
            }
        }

        /// <summary>Caller owns the bitmap. Cloning detaches it from the resource stream.</summary>
        public static Bitmap LoadBitmap(DeviceCategory category)
        {
            var name = ResourceName(category);
            if (name == null)
                return null;
            using (var stream = Open(name))
            using (var bitmap = new Bitmap(stream))
                return new Bitmap(bitmap);
        }

        private static readonly Dictionary<DeviceCategory, Bitmap> Trimmed = new Dictionary<DeviceCategory, Bitmap>();

        /// <summary>
        /// Artwork without its transparent border, reduced to at most <paramref name="maxSide"/> pixels.
        /// Small tray icons then show the motif itself instead of empty margins. Caller owns the returned copy.
        /// </summary>
        public static Bitmap LoadTrimmedBitmap(DeviceCategory category, int maxSide = 128)
        {
            lock (Trimmed)
            {
                if (!Trimmed.TryGetValue(category, out var cached))
                {
                    using (var source = LoadBitmap(category))
                    {
                        if (source == null)
                            return null;
                        Trimmed.Add(category, cached = Trim(source, maxSide));
                    }
                }
                return new Bitmap(cached);
            }
        }

        internal static Bitmap Trim(Bitmap source, int maxSide)
        {
            var bounds = OpaqueBounds(source);
            var scale = Math.Min(1f, (float)maxSide / Math.Max(bounds.Width, bounds.Height));
            var width = Math.Max(1, (int)Math.Round(bounds.Width * scale));
            var height = Math.Max(1, (int)Math.Round(bounds.Height * scale));
            var result = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(result))
            {
                graphics.Clear(System.Drawing.Color.Transparent);
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(source, new Rectangle(0, 0, width, height), bounds, GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>Smallest rectangle containing all visible pixels (almost transparent edge noise is ignored).</summary>
        internal static Rectangle OpaqueBounds(Bitmap source)
        {
            const int MinAlpha = 16;
            var rect = new Rectangle(0, 0, source.Width, source.Height);
            var data = source.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                var pixels = new int[data.Stride / 4 * data.Height];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                int left = source.Width, top = source.Height, right = -1, bottom = -1;
                for (var y = 0; y < source.Height; y++)
                {
                    var row = y * (data.Stride / 4);
                    for (var x = 0; x < source.Width; x++)
                    {
                        if (((pixels[row + x] >> 24) & 0xFF) < MinAlpha)
                            continue;
                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < top) top = y;
                        if (y > bottom) bottom = y;
                    }
                }
                // A completely transparent image keeps its full size instead of becoming empty.
                return right < 0 ? rect : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
            }
            finally
            {
                source.UnlockBits(data);
            }
        }

        public static void Draw(Graphics graphics, Bitmap image, RectangleF bounds)
        {
            var scale = Math.Min(bounds.Width / image.Width, bounds.Height / image.Height);
            var width = image.Width * scale;
            var height = image.Height * scale;
            graphics.DrawImage(image, bounds.X + (bounds.Width - width) / 2,
                bounds.Y + (bounds.Height - height) / 2, width, height);
        }
    }
}
