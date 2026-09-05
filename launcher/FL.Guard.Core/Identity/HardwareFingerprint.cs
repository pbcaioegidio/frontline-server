using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace FL.Guard.Core.Identity
{
    /// <summary>
    /// Coleta identificadores de hardware da máquina e gera fingerprint.
    /// Cada componente vira SHA-256 antes de sair do PC. Coletas rodam em paralelo com timeout
    /// (WMI pode travar); componente que falha fica vazio e entra em <see cref="HardwareComponents.Missing"/>.
    /// </summary>
    public static class HardwareFingerprint
    {
        private static readonly TimeSpan QueryTimeout = TimeSpan.FromSeconds(8);
        private static readonly object CacheLock = new object();
        private static HardwareComponents _cached;

        /// <summary>Sal fixo do produto: hashes não batem com os de outros jogos/sistemas.</summary>
        private const string Salt = "FrontLine.FLGuard.v1";

        // Valores lixo que fabricantes deixam nos campos SMBIOS.
        private static readonly string[] Junk =
        {
            "", "0", "00", "none", "null", "n/a", "na", "default", "default string", "to be filled by o.e.m.",
            "to be filled by oem", "system serial number", "base board serial number", "chassis serial number",
            "not specified", "not available", "unknown", "oem", "invalid", "123456789", "1234567890",
            "serial", "empty", "xxxxxxxx", "00000000", "0000000000", "ffffffff",
            "00000000-0000-0000-0000-000000000000", "ffffffff-ffff-ffff-ffff-ffffffffffff",
            "03000200-0400-0500-0006-000700080009"
        };

        /// <summary>Coleta (com cache por processo).</summary>
        public static HardwareComponents Collect(bool forceRefresh = false)
        {
            lock (CacheLock)
            {
                if (_cached != null && !forceRefresh)
                    return _cached;
            }

            HardwareComponents result = CollectInternal();

            lock (CacheLock)
                _cached = result;
            return result;
        }

        public static Task<HardwareComponents> CollectAsync(bool forceRefresh = false)
            => Task.Run(() => Collect(forceRefresh));

        private static HardwareComponents CollectInternal()
        {
            var missing = new List<string>();

            Task<string> tMotherboard = WithTimeout("motherboard", ReadMotherboard, missing);
            Task<string> tBios = WithTimeout("bios_uuid", ReadBiosUuid, missing);
            Task<string> tCpu = WithTimeout("cpu", ReadCpu, missing);
            Task<string> tDisk = WithTimeout("disk", ReadSystemDiskSerial, missing);
            Task<string> tRam = WithTimeout("ram", ReadRam, missing);
            Task<string> tGpu = WithTimeout("gpu", ReadGpu, missing);
            Task<byte[]> tTpm = WithTimeout("tpm", TpmEndorsementKey.TryRead, missing);
            Task<string> tGuid = WithTimeout("machine_guid", ReadMachineGuid, missing);
            Task<List<string>> tMacs = WithTimeout("mac", ReadPhysicalMacs, missing);

            Task.WaitAll(tMotherboard, tBios, tCpu, tDisk, tRam, tGpu, tTpm, tGuid, tMacs);

            string mb = Clean(tMotherboard.Result);
            string bios = Clean(tBios.Result);
            string cpu = Clean(tCpu.Result);
            string disk = Clean(tDisk.Result);
            string ram = Clean(tRam.Result);
            string gpu = Clean(tGpu.Result);
            string guid = Clean(tGuid.Result);
            byte[] tpm = tTpm.Result;
            List<string> macs = tMacs.Result ?? new List<string>();

            string tpmHash = tpm != null && tpm.Length > 0 ? Hash(tpm) : "";

            var comps = new HardwareComponents
            {
                Motherboard = HashStr(mb),
                BiosUuid = HashStr(bios),
                Cpu = HashStr(cpu),
                Disk = HashStr(disk),
                Ram = HashStr(ram),
                Tpm = tpmHash,
                Gpu = HashStr(gpu),
                MachineGuid = HashStr(guid),
                Macs = macs.Select(HashStr).Where(h => h.Length > 0).OrderBy(h => h, StringComparer.Ordinal).ToList(),
                TpmPresent = tpmHash.Length > 0,
                Missing = missing.Distinct().OrderBy(m => m, StringComparer.Ordinal).ToList(),
                CollectedAtUtc = DateTime.UtcNow
            };

            // Fingerprint composto usa os valores brutos normalizados (antes do hash individual).
            string composite = string.Join("|", mb, bios, cpu, disk, ram, tpmHash);
            comps.Fingerprint = HashStr(composite);

            int strength = 0;
            if (mb.Length > 0) strength++;
            if (bios.Length > 0) strength++;
            if (disk.Length > 0) strength++;
            if (ram.Length > 0) strength++;
            if (tpmHash.Length > 0) strength++;
            comps.Strength = strength;

            return comps;
        }

        // ------------------------------------------------------------------
        // Leitores
        // ------------------------------------------------------------------

        private static string ReadMotherboard()
        {
            string serial = WmiFirst("SELECT SerialNumber, Manufacturer, Product FROM Win32_BaseBoard", "SerialNumber");
            if (!IsJunk(serial))
                return serial;

            // Fallback: serial do gabinete/sistema
            string sys = WmiFirst("SELECT IdentifyingNumber FROM Win32_ComputerSystemProduct", "IdentifyingNumber");
            return IsJunk(sys) ? "" : sys;
        }

        private static string ReadBiosUuid()
        {
            string uuid = WmiFirst("SELECT UUID FROM Win32_ComputerSystemProduct", "UUID");
            return IsJunk(uuid) ? "" : uuid;
        }

        private static string ReadCpu()
        {
            // ProcessorId é igual entre CPUs do mesmo modelo — usado só como componente fraco.
            string id = WmiFirst("SELECT ProcessorId, Name FROM Win32_Processor", "ProcessorId");
            return IsJunk(id) ? "" : id;
        }

        private static string ReadSystemDiskSerial()
        {
            string root = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            string letter = root.TrimEnd('\\'); // "C:"

            try
            {
                using (var partitions = new ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID='" + letter + "'} WHERE AssocClass=Win32_LogicalDiskToPartition"))
                {
                    foreach (ManagementObject part in partitions.Get())
                    {
                        string partId = part["DeviceID"]?.ToString();
                        if (string.IsNullOrEmpty(partId)) continue;

                        using (var drives = new ManagementObjectSearcher(
                            "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partId.Replace("\\", "\\\\").Replace("'", "\\'") + "'} WHERE AssocClass=Win32_DiskDriveToDiskPartition"))
                        {
                            foreach (ManagementObject drive in drives.Get())
                            {
                                string serial = drive["SerialNumber"]?.ToString();
                                if (!IsJunk(serial))
                                    return serial;
                            }
                        }
                    }
                }
            }
            catch { }

            // Fallback: primeiro disco físico com serial
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber, Index FROM Win32_DiskDrive"))
                {
                    var all = searcher.Get().Cast<ManagementObject>()
                        .Select(d => new { Index = ToInt(d["Index"]), Serial = d["SerialNumber"]?.ToString() })
                        .Where(d => !IsJunk(d.Serial))
                        .OrderBy(d => d.Index)
                        .ToList();
                    if (all.Count > 0)
                        return all[0].Serial;
                }
            }
            catch { }
            return "";
        }

        private static string ReadRam()
        {
            var serials = WmiAll("SELECT SerialNumber, PartNumber, Capacity FROM Win32_PhysicalMemory", "SerialNumber")
                .Where(s => !IsJunk(s))
                .Select(Normalize)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();
            return serials.Count == 0 ? "" : string.Join(";", serials);
        }

        private static string ReadGpu()
        {
            var ids = WmiAll("SELECT PNPDeviceID, Name FROM Win32_VideoController", "PNPDeviceID")
                .Where(s => !IsJunk(s))
                .Select(Normalize)
                .OrderBy(s => s, StringComparer.Ordinal)
                .ToList();
            return ids.Count == 0 ? "" : string.Join(";", ids);
        }

        private static string ReadMachineGuid()
        {
            try
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography", false))
                {
                    string v = key?.GetValue("MachineGuid") as string;
                    return IsJunk(v) ? "" : v;
                }
            }
            catch
            {
                return "";
            }
        }

        private static readonly string[] VirtualNicMarkers =
        {
            "virtual", "vmware", "hyper-v", "vbox", "virtualbox", "tap-", "tap ", "tunnel", "vpn", "wan miniport",
            "bluetooth", "loopback", "npcap", "wireguard", "zerotier", "hamachi", "radmin", "docker", "wsl", "teredo", "isatap"
        };

        private static List<string> ReadPhysicalMacs()
        {
            var macs = new List<string>();
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.GigabitEthernet)
                        continue;

                    string desc = (nic.Description ?? "").ToLowerInvariant();
                    if (VirtualNicMarkers.Any(m => desc.Contains(m)))
                        continue;

                    byte[] addr = nic.GetPhysicalAddress()?.GetAddressBytes();
                    if (addr == null || addr.Length != 6 || addr.All(b => b == 0))
                        continue;

                    // MAC administrado localmente (bit 0x02 no primeiro byte) costuma ser virtual/randomizado.
                    if ((addr[0] & 0x02) != 0)
                        continue;

                    macs.Add(string.Join(":", addr.Select(b => b.ToString("X2"))));
                }
            }
            catch { }
            return macs.Distinct().OrderBy(m => m, StringComparer.Ordinal).ToList();
        }

        // ------------------------------------------------------------------
        // WMI helpers
        // ------------------------------------------------------------------

        private static string WmiFirst(string query, string property)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string v = obj[property]?.ToString();
                        if (!string.IsNullOrWhiteSpace(v))
                            return v;
                    }
                }
            }
            catch { }
            return "";
        }

        private static List<string> WmiAll(string query, string property)
        {
            var list = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string v = obj[property]?.ToString();
                        if (!string.IsNullOrWhiteSpace(v))
                            list.Add(v);
                    }
                }
            }
            catch { }
            return list;
        }

        private static Task<T> WithTimeout<T>(string name, Func<T> reader, List<string> missing)
        {
            var task = Task.Run(reader);
            return task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    return t.Result;
                lock (missing) missing.Add(name);
                return default(T);
            }).WithTimeoutFallback(QueryTimeout, () =>
            {
                lock (missing) missing.Add(name);
                return default(T);
            });
        }

        // ------------------------------------------------------------------
        // Normalização e hash
        // ------------------------------------------------------------------

        private static bool IsJunk(string v)
        {
            if (v == null) return true;
            string n = v.Trim().ToLowerInvariant();
            if (n.Length < 3) return true;
            if (Junk.Contains(n)) return true;
            // só um caractere repetido (000000, ffffff, xxxxxx)
            if (n.Distinct().Count() == 1) return true;
            return false;
        }

        private static string Clean(string v) => string.IsNullOrWhiteSpace(v) ? "" : Normalize(v);

        private static string Normalize(string v)
        {
            // remove espaços e deixa maiúsculo — mesmo valor, mesma forma em qualquer boot
            return new string((v ?? "").Trim().ToUpperInvariant().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        private static string HashStr(string v)
        {
            if (string.IsNullOrEmpty(v)) return "";
            return Hash(Encoding.UTF8.GetBytes(v));
        }

        private static string Hash(byte[] data)
        {
            if (data == null || data.Length == 0) return "";
            using (var sha = SHA256.Create())
            {
                byte[] salt = Encoding.UTF8.GetBytes(Salt);
                var buf = new byte[salt.Length + data.Length];
                Buffer.BlockCopy(salt, 0, buf, 0, salt.Length);
                Buffer.BlockCopy(data, 0, buf, salt.Length, data.Length);
                byte[] h = sha.ComputeHash(buf);
                var sb = new StringBuilder(h.Length * 2);
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static int ToInt(object o)
        {
            try { return Convert.ToInt32(o); } catch { return int.MaxValue; }
        }
    }

    internal static class TaskTimeoutExtensions
    {
        public static Task<T> WithTimeoutFallback<T>(this Task<T> task, TimeSpan timeout, Func<T> fallback)
        {
            var tcs = new TaskCompletionSource<T>();
            var cts = new CancellationTokenSource();

            task.ContinueWith(t =>
            {
                cts.Cancel();
                if (t.Status == TaskStatus.RanToCompletion)
                    tcs.TrySetResult(t.Result);
                else
                    tcs.TrySetResult(fallback());
            }, TaskContinuationOptions.ExecuteSynchronously);

            Task.Delay(timeout, cts.Token).ContinueWith(_ =>
            {
                if (!task.IsCompleted)
                    tcs.TrySetResult(fallback());
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return tcs.Task;
        }
    }
}
