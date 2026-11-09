using System;
using System.Security.Cryptography;
using System.Text;

namespace BatteryBuddy.Providers.Native
{
    internal static class HidBatteryIdentity
    {
        internal static string Create(string prefix, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A HID battery device requires a stable interface path.", nameof(path));
            using (var sha = SHA256.Create())
                return prefix + BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(path.ToUpperInvariant()))).Replace("-", string.Empty);
        }

        internal static bool IsUsbInterface(string path, int number)
        {
            const string prefix = @"\\?\hid#";
            if (path == null || !path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
            var end = path.IndexOf('#', prefix.Length);
            if (end <= prefix.Length)
                return false;
            var expected = "mi_" + number.ToString("x2", System.Globalization.CultureInfo.InvariantCulture);
            foreach (var part in path.Substring(prefix.Length, end - prefix.Length).Split('&'))
                if (string.Equals(part, expected, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
