using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Executable.Utility
{
    internal static class AppInfo
    {
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsMacOs => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static string WindowsInstallationDirectory => Path.GetPathRoot(Environment.SystemDirectory);
        public static bool Is64 => Environment.Is64BitOperatingSystem;
        public static string OsArch => Is64 ? "64" : "32";
        public static bool IsInDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    }
}
