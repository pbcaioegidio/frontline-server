using Launcher.PointBlank.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;

namespace Launcher.PointBlank
{
    /// <summary>
    /// Proteção real rodando no splash do FL Guard (não é só visual).
    /// </summary>
    internal static class GuardProtection
    {
        private static readonly string[] CriticalLocals =
        {
            "FrontLine.exe",
            "PointBlank.i3Exec",
            "Crypto.dll",
            "i3TDK.dll",
            "CHEAT_BLOCKER\\CB.exe"
        };

        private static readonly string[] SuspiciousProcessNames =
        {
            "cheatengine-x86_64", "cheatengine-i386", "cheatengine",
            "ce-x86", "ce-x64",
            "x64dbg", "x32dbg", "ollydbg",
            "ida64", "ida", "idag", "idag64",
            "scylla", "extremeinjector", "dllinjector"
        };

        public static async Task<(bool Ok, string Message)> RunAsync(string clientRoot, Action<string, string> status)
        {
            status?.Invoke("Verificando assinatura da lista...", "Aguardando conexão segura...");
            await Task.Delay(400).ConfigureAwait(false);

            string dat = Path.Combine(clientRoot, "UserFileList.dat");
            string sig = Path.Combine(clientRoot, "UserFileList.sig");
            if (!ManifestTrust.VerifyFile(dat, sig, out string trustError))
                return (false, trustError);

            status?.Invoke("Verificando integridade do jogo e anti-cheat... (FL GUARD)", "Validando arquivos críticos...");
            await Task.Delay(200).ConfigureAwait(false);

            Dictionary<string, string> map = LoadList(dat);
            if (map.Count == 0)
                return (false, "UserFileList.dat inválido.");

            foreach (string local in CriticalLocals)
            {
                string key = IntegrityRules.NormalizeLocal(local);
                if (!map.TryGetValue(key, out string expected))
                {
                    // lista antiga sem CB.exe path — tenta achar por nome
                    expected = map.FirstOrDefault(kv =>
                        kv.Key.EndsWith("\\" + Path.GetFileName(key), StringComparison.OrdinalIgnoreCase)
                        || kv.Key.Equals(Path.GetFileName(key), StringComparison.OrdinalIgnoreCase)).Value;
                    if (string.IsNullOrEmpty(expected))
                        continue;
                }

                string full = Path.Combine(clientRoot, key);
                if (!File.Exists(full))
                    return (false, "Arquivo crítico ausente: " + key);

                string hash = HashMd5(full);
                if (!string.Equals(hash, expected, StringComparison.OrdinalIgnoreCase))
                    return (false, "Arquivo alterado: " + key);
            }

            status?.Invoke("Verificando integridade do jogo e anti-cheat... (FL GUARD)", "Procurando programas suspeitos...");
            await Task.Delay(300).ConfigureAwait(false);

            string bad = FindSuspiciousProcess();
            if (bad != null)
                return (false, "Feche o programa suspeito e tente de novo: " + bad);

            status?.Invoke("Verificando integridade do jogo e anti-cheat... (FL GUARD)", "Canal seguro estabelecido.");
            await Task.Delay(500).ConfigureAwait(false);
            return (true, null);
        }

        private static string FindSuspiciousProcess()
        {
            Process[] all;
            try { all = Process.GetProcesses(); }
            catch { return null; }

            foreach (Process p in all)
            {
                string name = null;
                try { name = p.ProcessName; } catch { }
                if (string.IsNullOrEmpty(name))
                    continue;
                string n = name.ToLowerInvariant();
                foreach (string s in SuspiciousProcessNames)
                {
                    if (n.Contains(s))
                        return name;
                }
            }
            return null;
        }

        private static Dictionary<string, string> LoadList(string path)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var doc = new XmlDocument();
                doc.Load(path);
                XmlNode list = doc.SelectSingleNode("/list");
                if (list == null) return map;
                foreach (XmlNode node in list.ChildNodes)
                {
                    if (node.Name != "file" || node.Attributes == null) continue;
                    string local = node.Attributes["local"]?.Value ?? node.Attributes["n"]?.Value;
                    string hash = node.Attributes["hash"]?.Value ?? node.Attributes["m"]?.Value;
                    if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(hash)) continue;
                    map[IntegrityRules.NormalizeLocal(local)] = hash.Trim();
                }
            }
            catch { }
            return map;
        }

        private static string HashMd5(string path)
        {
            using var md5 = MD5.Create();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
            return Convert.ToHexString(md5.ComputeHash(fs));
        }
    }
}
