using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FL.Guard.Core.Protection
{
    /// <summary>
    /// Escaneia processos suspeitos e calcula hash dos módulos do FrontLine.exe.
    /// Roda no launcher (heartbeat); nada bruto de memória — só nomes e hashes de arquivo.
    /// </summary>
    public static class RuntimeScan
    {
        private static readonly string[] SuspiciousNames =
        {
            "cheatengine", "cheat engine", "ce-x64", "ce-x86",
            "x64dbg", "x32dbg", "ollydbg", "immunitydebugger", "windbg",
            "ida64", "ida32", "idaq", "idaw", "ghidra",
            "processhacker", "procmon", "procexp", "process explorer",
            "artmoney", "gameguardian", "speedhack", "wemod", "cheathappens",
            "extreme injector", "extremeinjector", "xenos", "dllinjector", "injector",
            "httpdebugger", "fiddler", "charles", "wireshark", "rawcap",
            "scylla", "pe-sieve", "hollows_hunter", "megadumper",
            "reclass", "reclass.net", "cheat turbine"
        };

        public sealed class ScanResult
        {
            public string ModulesHash = "";
            public List<string> Suspicious = new List<string>();
            public List<string> Modules = new List<string>();
            public bool GameRunning;
            public string Status = "ok";
            public string StatusReason = "";
        }

        public static ScanResult Scan(string gameProcessName = "FrontLine")
        {
            var result = new ScanResult();
            try
            {
                // 1) processos suspeitos
                foreach (Process p in Process.GetProcesses())
                {
                    string name = "";
                    try { name = (p.ProcessName ?? "").ToLowerInvariant(); } catch { continue; }
                    if (string.IsNullOrEmpty(name)) continue;

                    foreach (string marker in SuspiciousNames)
                    {
                        if (name.Contains(marker.Replace(" ", "")))
                        {
                            result.Suspicious.Add(p.ProcessName);
                            break;
                        }
                    }
                }
                result.Suspicious = result.Suspicious.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList();

                // 2) módulos do jogo
                string procName = Path.GetFileNameWithoutExtension(gameProcessName ?? "FrontLine");
                Process game = null;
                try
                {
                    game = Process.GetProcessesByName(procName).FirstOrDefault(p =>
                    {
                        try { return p.MainWindowHandle != IntPtr.Zero || p.Modules.Count > 0; }
                        catch { return false; }
                    });
                }
                catch { }

                if (game == null)
                {
                    result.GameRunning = false;
                    result.Status = "suspect";
                    result.StatusReason = "FrontLine.exe não encontrado";
                }
                else
                {
                    result.GameRunning = true;
                    try
                    {
                        string exePath = null;
                        try { exePath = game.MainModule?.FileName; } catch { }

                        var parts = new List<string>();
                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                            parts.Add("exe:" + FileSha256(exePath));

                        try
                        {
                            foreach (ProcessModule mod in game.Modules)
                            {
                                string mn = (mod.ModuleName ?? "").ToLowerInvariant();
                                if (string.IsNullOrEmpty(mn)) continue;
                                // ignora DLLs do Windows (system32 / SysWOW64)
                                string path = "";
                                try { path = mod.FileName ?? ""; } catch { }
                                if (IsSystemPath(path)) continue;
                                result.Modules.Add(mn);
                                parts.Add(mn + ":" + (string.IsNullOrEmpty(path) || !File.Exists(path) ? "0" : FileSha256(path).Substring(0, 16)));
                            }
                        }
                        catch
                        {
                            // acesso a Modules pode falhar sem elevação — só hash do exe
                        }

                        result.Modules.Sort(StringComparer.OrdinalIgnoreCase);
                        result.ModulesHash = Sha256Text(string.Join("|", parts));
                    }
                    catch (Exception ex)
                    {
                        result.StatusReason = Trunc(ex.Message, 120);
                    }
                }

                if (result.Suspicious.Count > 0)
                {
                    result.Status = "tamper";
                    result.StatusReason = "processo suspeito: " + string.Join(",", result.Suspicious.Take(5));
                }
            }
            catch (Exception ex)
            {
                result.Status = "suspect";
                result.StatusReason = Trunc(ex.Message, 120);
            }
            return result;
        }

        private static bool IsSystemPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;
            string p = path.ToLowerInvariant().Replace('/', '\\');
            return p.Contains("\\windows\\system32\\") ||
                   p.Contains("\\windows\\syswow64\\") ||
                   p.Contains("\\windows\\winsxs\\") ||
                   p.Contains("\\microsoft.net\\");
        }

        private static string FileSha256(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                using (var sha = SHA256.Create())
                {
                    byte[] h = sha.ComputeHash(fs);
                    var sb = new StringBuilder(h.Length * 2);
                    foreach (byte b in h) sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
            catch { return "0"; }
        }

        private static string Sha256Text(string text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                var sb = new StringBuilder(h.Length * 2);
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
