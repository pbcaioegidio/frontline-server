using System;
using System.Collections.Generic;
using System.IO;

namespace Launcher.PointBlank.Services
{
    /// <summary>
    /// O que entra na lista de integridade e o que é arquivo extra suspeito.
    /// Config do jogador (config.zpt) e logs ficam de fora de propósito.
    /// </summary>
    public static class IntegrityRules
    {
        public static readonly HashSet<string> ExtraExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".i3pack", ".i3i", ".i3vteximage", ".i3animpack", ".i3s", ".i3chr",
            ".dll", ".exe", ".lua", ".pak", ".i3exec"
        };

        public static string NormalizeLocal(string relative)
        {
            return (relative ?? "").Replace('/', '\\').TrimStart('\\');
        }

        public static bool ShouldSkip(string relative)
        {
            string local = NormalizeLocal(relative);
            if (string.IsNullOrWhiteSpace(local))
                return true;

            string name = Path.GetFileName(local);
            string ext = Path.GetExtension(local);

            if (name.Equals("UserFileList.dat", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("UserFileList.sig", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("ufl-md5.txt", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("config.zpt", StringComparison.OrdinalIgnoreCase)) return true;
            // Preferencias do jogador (resolucao, etc.) — FLConfig/jogo reescrevem
            if (name.Equals("env_settings.ini", StringComparison.OrdinalIgnoreCase)
                && StartsWithFolder(local, "EnvSet")) return true;
            // Versao/config local — mudam no Update / por maquina
            if (name.Equals("launcher.svl", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("LocalConfig.json", StringComparison.OrdinalIgnoreCase)) return true;
            // Catalogos sincronizados com o server (Data/Raws) — o client reescreve no login
            if (name.Equals("Shop.dat", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("EventPortal.dat", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("Thumbs.db", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("FLSetup.exe", StringComparison.OrdinalIgnoreCase)) return true;
            // Desinstalador Inno (criado so apos o setup — nao entra na lista)
            if (name.StartsWith("unins", StringComparison.OrdinalIgnoreCase)
                && (ext.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".dat", StringComparison.OrdinalIgnoreCase)
                    || ext.Equals(".msg", StringComparison.OrdinalIgnoreCase)))
                return true;

            // Socket: config/DB/Evidence mudam por máquina — não entram na lista assinada
            if (StartsWithFolder(local, "FLService\\config")) return true;
            if (StartsWithFolder(local, "FLService\\Evidence")) return true;
            if (StartsWithFolder(local, "FLService\\logs")) return true;
            if (StartsWithFolder(local, "FLService\\tools")) return true; // ffmpeg local (MP4)
            if (name.Equals("config.ini", StringComparison.OrdinalIgnoreCase)
                && StartsWithFolder(local, "FLService")) return true;

            if (ext.Equals(".log", StringComparison.OrdinalIgnoreCase)) return true;
            if (ext.Equals(".pdb", StringComparison.OrdinalIgnoreCase)) return true;
            if (ext.Equals(".ini", StringComparison.OrdinalIgnoreCase)
                && StartsWithFolder(local, "FLService")) return true;
            // caches do client (mudam ao jogar)
            if (ext.Equals(".i3gl", StringComparison.OrdinalIgnoreCase)) return true;
            if (ext.Equals(".pbc", StringComparison.OrdinalIgnoreCase)) return true;

            // cache/runtime do Chromium (CEF) muda sozinho — não é adulteração
            if (StartsWithFolder(local, "CEF\\Cache")) return true;
            if (StartsWithFolder(local, "CEF\\UserData")) return true;
            if (StartsWithFolder(local, "CEF\\GPUCache")) return true;
            if (StartsWithFolder(local, "Shader\\Cache")) return true;
            if (StartsWithFolder(local, "Shader"))
            {
                // Cache.i3GL e similares sob Shader\
                if (name.IndexOf("cache", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }

            if (StartsWithFolder(local, "_icon_bak")) return true;
            // _fl_publish_tmp e _fl_publish_tmp_cfg (e similares)
            if (local.StartsWith("_fl_publish_tmp", StringComparison.OrdinalIgnoreCase)) return true;
            if (StartsWithFolder(local, "tools")) return true;
            if (name.StartsWith("FLLauncher.exe.bak", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".new", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.Equals("_apply_launcher_update.cmd", StringComparison.OrdinalIgnoreCase)) return true;
            if (StartsWithFolder(local, "_DownloadPatchFiles")) return true;
            if (name.Equals("V3PreCommon.pbc", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static bool IsProtectedExtra(string relative)
        {
            if (ShouldSkip(relative))
                return false;
            return ExtraExtensions.Contains(Path.GetExtension(NormalizeLocal(relative)));
        }

        private static bool StartsWithFolder(string local, string folder)
        {
            return local.Equals(folder, StringComparison.OrdinalIgnoreCase)
                || local.StartsWith(folder + "\\", StringComparison.OrdinalIgnoreCase);
        }
    }
}
