using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Executable.Utility
{
    public static class HWID
    {
        public static string Generate()
        {
            string[] Resource = new string[]
            {
                GetInfo(Hardware.Cpuid),
                GetInfo(Hardware.Motherboard)
            };
            string Input = string.Join("\n", Resource);
            string Result = Hash(Input);
            return Result;
        }
        private static string Hash(string input)
        {
            using (SHA1Managed SHA1 = new SHA1Managed())
            {
                byte[] Hases = SHA1.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder Builder = new StringBuilder(Hases.Length * 2);
                foreach (byte C in Hases) // can be "x2" if you want lowercase
                {
                    Builder.Append(C.ToString("X2"));
                }
                return Builder.ToString();
            }
        }
        private static string Wmi(string wmiClass, string wmiProperty)
        {
            string result = "";
            var mc = new ManagementClass(wmiClass);
            var moc = mc.GetInstances();
            foreach (var o in moc)
            {
                var mo = o as ManagementObject;
                if (result != "")
                {
                    continue;
                }
                try
                {
                    result = mo[wmiProperty].ToString();
                    break;
                }
                catch
                {
                }
            }

            return result;
        }
        private static string Dmidecode(string query, string find)
        {
            Cmd cmd = new Cmd();
            var k = cmd.Run("/usr/bin/sudo", $" {query}", new CmdOptions
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStdOut = true,
                UseOsShell = false
            }, true);
            find = find.EndsWith(":") ? find : $"{find}:";
            var lines = k.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim(' ', '\t'));
            string line = lines.First(a => a.StartsWith(find));
            string res = line.Substring(line.IndexOf(find, StringComparison.Ordinal) + find.Length).Trim(' ', '\t');
            return res;
        }
        private static string GetIoregOutput(string node)
        {
            Process proc = new Process();
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "/bin/sh"
            };
            string command = @"/usr/sbin/ioreg -rd1 -c IOPlatformExpertDevice | awk -F'\""' '/" + node + "/{ print $(NF-1) }'";
            psi.Arguments = $"-c \"{command}\"";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            string result = null;
            proc.StartInfo = psi;
            proc.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    result = e.Data;
                }
            };
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            return result;
        }
        private static string GetInfo(Hardware HW)
        {
            switch (HW)
            {
                case Hardware.Motherboard when AppInfo.IsLinux:
                {
                    string result = Dmidecode("dmidecode -t 2", "Manufacturer");
                    return result;
                }
                case Hardware.Motherboard when AppInfo.IsWindows:
                {
                    string result = Wmi("Win32_BaseBoard", "Manufacturer");
                    return result;
                }
                case Hardware.Motherboard when AppInfo.IsMacOs:
                {
                    string macSerial = GetIoregOutput("IOPlatformSerialNumber");
                    return macSerial;
                }
                case Hardware.Cpuid when AppInfo.IsLinux:
                {
                    string res = Dmidecode("dmidecode -t 4", "ID");
                    IEnumerable<string> parts = res.Split(' ').Reverse();
                    string result = string.Join("", parts);
                    return result;
                }
                case Hardware.Cpuid when AppInfo.IsWindows:
                {
                    string asmCpuId = ASM.GetProcessorId();
                    return asmCpuId?.Length > 2 ? asmCpuId : Wmi("Win32_Processor", "ProcessorId");
                }
                case Hardware.Cpuid when AppInfo.IsMacOs:
                {
                    string uuid = GetIoregOutput("IOPlatformUUID");
                    return uuid;
                }
                default: throw new InvalidEnumArgumentException();
            }
        }
        private enum Hardware
        {
            Motherboard,
            Cpuid
        }
    }
}
