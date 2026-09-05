using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Launcher.PointBlank.Services
{
    /// <summary>
    /// Troca FLLauncher.exe (e outros EXEs em uso) via .new + script CMD após sair.
    /// Windows não deixa sobrescrever o próprio processo.
    /// </summary>
    public static class SelfUpdateHelper
    {
        public static bool IsRunningExecutable(string destPath)
        {
            try
            {
                string current = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(current)) return false;
                return string.Equals(
                    Path.GetFullPath(destPath),
                    Path.GetFullPath(current),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>EXE do launcher ou qualquer arquivo que está em uso pelo processo atual.</summary>
        public static bool NeedsDeferredReplace(string destPath)
        {
            string name = Path.GetFileName(destPath);
            if (name.Equals("FLLauncher.exe", StringComparison.OrdinalIgnoreCase))
                return true;
            return IsRunningExecutable(destPath);
        }

        public static string DeferredPath(string destPath) => destPath + ".new";

        /// <summary>
        /// Gera CMD que espera o launcher fechar, troca .new → exe e reabre.
        /// Retorna o path do .cmd criado.
        /// </summary>
        public static string WriteRestartScript(string clientRoot, string exeName = "FLLauncher.exe")
        {
            string newPath = Path.Combine(clientRoot, exeName + ".new");
            string exePath = Path.Combine(clientRoot, exeName);
            string bakPath = Path.Combine(clientRoot, exeName + ".bak");
            string cmdPath = Path.Combine(clientRoot, "_apply_launcher_update.cmd");

            // /c fecha a janela; timeout dá tempo do processo liberar o handle
            var sb = new StringBuilder();
            sb.AppendLine("@echo off");
            sb.AppendLine("cd /d \"%~dp0\"");
            sb.AppendLine("timeout /t 2 /nobreak >nul");
            sb.AppendLine(":wait");
            sb.AppendLine("tasklist /FI \"IMAGENAME eq " + exeName + "\" 2>nul | find /I \"" + exeName + "\" >nul");
            sb.AppendLine("if not errorlevel 1 (");
            sb.AppendLine("  timeout /t 1 /nobreak >nul");
            sb.AppendLine("  goto wait");
            sb.AppendLine(")");
            sb.AppendLine("if exist \"" + Path.GetFileName(newPath) + "\" (");
            sb.AppendLine("  if exist \"" + Path.GetFileName(bakPath) + "\" del /f /q \"" + Path.GetFileName(bakPath) + "\"");
            sb.AppendLine("  if exist \"" + Path.GetFileName(exePath) + "\" move /y \"" + Path.GetFileName(exePath) + "\" \"" + Path.GetFileName(bakPath) + "\" >nul");
            sb.AppendLine("  move /y \"" + Path.GetFileName(newPath) + "\" \"" + Path.GetFileName(exePath) + "\" >nul");
            sb.AppendLine(")");
            sb.AppendLine("start \"\" \"" + Path.GetFileName(exePath) + "\"");
            sb.AppendLine("del /f /q \"%~f0\" >nul 2>&1");

            File.WriteAllText(cmdPath, sb.ToString(), Encoding.ASCII);
            return cmdPath;
        }

        public static void StartRestartAndExit(string clientRoot)
        {
            string cmd = WriteRestartScript(clientRoot);
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"" + cmd + "\"",
                WorkingDirectory = clientRoot,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);
            ApplicationExit();
        }

        private static void ApplicationExit()
        {
            try { System.Windows.Forms.Application.Exit(); }
            catch { Environment.Exit(0); }
        }
    }
}
