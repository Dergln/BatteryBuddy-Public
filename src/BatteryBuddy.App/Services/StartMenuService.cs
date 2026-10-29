using System;
using System.IO;

namespace BatteryBuddy.App.Services
{
    /// <summary>Legt eine Startmenü-Verknüpfung (.lnk) für den aktuellen Benutzer an oder entfernt sie.</summary>
    public static class StartMenuService
    {
        private static string LinkPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Programs), "BatteryBuddy.lnk");

        public static bool Exists() => File.Exists(LinkPath);

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                if (!enabled)
                {
                    if (File.Exists(LinkPath))
                        File.Delete(LinkPath);
                    return true;
                }

                // WScript.Shell ist in jedem Windows enthalten; wir nutzen es per Reflection (spätes Binden).
                var type = Type.GetTypeFromProgID("WScript.Shell");
                if (type == null)
                    return false;

                var shell = Activator.CreateInstance(type);
                var link = type.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { LinkPath });
                var lt = link.GetType();
                var exe = AutostartService.ExePath();
                lt.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, link, new object[] { exe });
                lt.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, link, new object[] { Path.GetDirectoryName(exe) });
                lt.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, link, new object[] { "BatteryBuddy" });
                lt.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, link, null);
                return File.Exists(LinkPath);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Reflection.TargetInvocationException || ex is System.Runtime.InteropServices.COMException)
            {
                return false;
            }
        }
    }
}
