using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Windows;

namespace BatteryBuddy.App.Services
{
    internal static class ExternalLinkService
    {
        // Nur fest hinterlegte HTTPS-Ziele mit exakt erlaubtem Host dürfen den Browser öffnen.
        private static readonly string[] AllowedHosts =
        {
            "freesound.org", "www.flaticon.com", "creativecommons.org",
            "github.com", "www.gnu.org", "commons.wikimedia.org", "www.heise.de", "www.youtube.com", "www.paypal.com"
        };

        internal static bool IsAllowed(Uri uri)
            => uri != null && uri.IsAbsoluteUri && uri.Scheme == Uri.UriSchemeHttps && uri.Port == 443
                && string.IsNullOrEmpty(uri.UserInfo) && Array.IndexOf(AllowedHosts, uri.Host) >= 0;

        internal static void Open(Uri uri)
        {
            if (!IsAllowed(uri))
            {
                ShowError("links.notAllowed");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Win32Exception)
            {
                ShowError("links.openFailed");
            }
            catch (InvalidOperationException)
            {
                ShowError("links.openFailed");
            }
            catch (SecurityException)
            {
                ShowError("links.openFailed");
            }
            catch (ArgumentException)
            {
                ShowError("links.openFailed");
            }
        }

        private static void ShowError(string key)
            => MessageBox.Show(Loc.Get(key), Loc.Get("app.name"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
