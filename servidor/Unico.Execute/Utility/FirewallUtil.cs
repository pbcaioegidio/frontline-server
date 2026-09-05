using Plugin.Core;
using Plugin.Core.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace Executable.Utility
{
    public static class FirewallUtil
    {
        public static void AddFirewallRule(string fullPath)
        {
            RunNetsh($"advfirewall firewall add rule name=\"Server: {Path.GetFileName(fullPath)}\" dir=out action=allow program=\"{fullPath}\" enable=yes");
        }

        public static void RemoveFirewallRule(string fullPath)
        {
            RunNetsh($"advfirewall firewall delete rule name=\"Server: {Path.GetFileName(fullPath)}\" program=\"{fullPath}\"");
        }

        private static void RunNetsh(string arguments)
        {
            try
            {
                using (Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }))
                {
                    process.WaitForExit(10000);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
