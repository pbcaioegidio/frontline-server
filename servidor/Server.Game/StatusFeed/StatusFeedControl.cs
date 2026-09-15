using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.Settings;
using Plugin.Core.XML;
using System;
using System.IO;
using System.Text;

namespace Server.Game.StatusFeed
{
    /// <summary>
    /// Comandos owner (bot Discord #ai) via StatusFeed WS — mesmos efeitos do painel WinForms.
    /// </summary>
    public static class StatusFeedControl
    {
        public static (bool ok, string message) Run(string cmd, string target)
        {
            try
            {
                cmd = (cmd ?? "").Trim().ToLowerInvariant();
                target = (target ?? "").Trim().ToLowerInvariant();

                if (cmd == "clearlogs" || (cmd == "reload" && target == "logs"))
                    return ClearLogs();

                if (cmd == "togglelog" || cmd == "logmode" || (cmd == "reload" && target == "logmode"))
                    return ToggleLogMode();

                if (cmd == "settings" || cmd == "showsettings" || (cmd == "reload" && target == "settings"))
                    return ShowSettingsIni();

                if (cmd != "reload")
                    return (false, "cmd desconhecido (use reload|clearlogs|togglelog|settings)");

                switch (target)
                {
                    case "all":
                    case "tudo":
                        return ReloadAll();
                    case "config":
                        return ReloadConfig();
                    case "shop":
                    case "loja":
                        return ReloadShop();
                    case "events":
                    case "eventos":
                        return ReloadEvents();
                    case "rules":
                    case "regras":
                        return ReloadRules();
                    case "attachments":
                    case "anexos":
                        return ReloadAttachments();
                    default:
                        return (false, "target inválido: all|config|shop|events|rules|attachments");
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[StatusFeedControl] " + ex.Message, LoggerType.Warning, ex);
                return (false, ex.Message);
            }
        }

        private static (bool, string) ReloadAll()
        {
            var sb = new StringBuilder();
            void step(string name, Func<(bool, string)> fn)
            {
                var (ok, msg) = fn();
                sb.Append(ok ? "✓ " : "✗ ").Append(name).Append(": ").Append(msg).Append(" | ");
            }
            step("config", ReloadConfig);
            step("shop", ReloadShop);
            step("events", ReloadEvents);
            step("rules", ReloadRules);
            step("attachments", ReloadAttachments);
            CLogger.Print("[StatusFeedControl] reload all done", LoggerType.Command);
            string text = sb.ToString().TrimEnd(' ', '|');
            return (true, "Reload completo — " + text);
        }

        private static (bool, string) ToggleLogMode()
        {
            ConfigLoader.ShowMoreInfo = !ConfigLoader.ShowMoreInfo;
            try
            {
                var cfg = new ConfigEngine("Config/Settings.ini");
                if (cfg.KeyExists("MoreInfo", "Server"))
                    cfg.WriteX("MoreInfo", ConfigLoader.ShowMoreInfo, "Server");
            }
            catch (Exception ex)
            {
                CLogger.Print("[StatusFeedControl] togglelog persist: " + ex.Message, LoggerType.Warning);
            }
            string mode = ConfigLoader.ShowMoreInfo ? "Alto" : "Padrão";
            CLogger.Print($"[StatusFeedControl] log mode = {mode}", LoggerType.Command);
            return (true, $"Modo de log: {mode}");
        }

        /// <summary>
        /// Conteúdo do Settings.ini para o Discord (senhas/salts mascarados).
        /// Prefixo SETTINGS_FILE\n para o bot anexar como arquivo.
        /// </summary>
        private static (bool, string) ShowSettingsIni()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Settings.ini");
            if (!File.Exists(path))
                return (false, "Arquivo Config/Settings.ini não encontrado");

            string[] lines = File.ReadAllLines(path);
            var sb = new StringBuilder(lines.Length * 40);
            sb.Append("SETTINGS_FILE\n");
            sb.Append("# FrontLine Settings.ini (senhas/tokens mascarados)\n");
            sb.Append("# Gerado em ").Append(DateTime.UtcNow.ToString("u")).Append(" UTC\n\n");

            foreach (string raw in lines)
            {
                string line = raw ?? "";
                int eq = line.IndexOf('=');
                if (eq > 0)
                {
                    string key = line.Substring(0, eq).Trim();
                    if (IsSensitiveIniKey(key))
                    {
                        sb.Append(key).Append(" = ***\n");
                        continue;
                    }
                }
                sb.Append(line).Append('\n');
            }

            CLogger.Print("[StatusFeedControl] settings.ini enviado ao Discord (redacted)", LoggerType.Command);
            return (true, sb.ToString());
        }

        private static bool IsSensitiveIniKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            string k = key.Trim();
            return k.Equals("Pass", StringComparison.OrdinalIgnoreCase)
                || k.Equals("Password", StringComparison.OrdinalIgnoreCase)
                || k.Equals("CryptedPasswordSalt", StringComparison.OrdinalIgnoreCase)
                || k.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0
                || k.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0
                || k.IndexOf("Secret", StringComparison.OrdinalIgnoreCase) >= 0
                || k.IndexOf("Salt", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static (bool, string) ReloadConfig()
        {
            ServerConfigJSON.Reload();
            CommandHelperJSON.Reload();
            ResolutionJSON.Reload();
            var gameConfig = ServerConfigJSON.GetConfig(ConfigLoader.ConfigId);
            if (gameConfig != null)
            {
                foreach (var manager in GameXender.All)
                    manager.Config = gameConfig;
            }
            CLogger.Print("[StatusFeedControl] config reload OK", LoggerType.Command);
            return (true, "Config recarregada (ServerConfig/CommandHelper/Resolution)");
        }

        private static (bool, string) ReloadShop()
        {
            ShopManager.Reset();
            ShopManager.Load(1);
            ShopManager.Load(2);
            GameXender.UpdateShop();
            CLogger.Print("[StatusFeedControl] shop reload OK", LoggerType.Command);
            return (true, "Loja recarregada e enviada aos clients online");
        }

        private static (bool, string) ReloadEvents()
        {
            EventLoginXML.Reload();
            EventBoostXML.Reload();
            EventPlaytimeJSON.Reload();
            EventQuestXML.Reload();
            EventRankUpXML.Reload();
            EventVisitXML.Reload();
            EventXmasXML.Reload();
            GameXender.UpdateEvents();
            CLogger.Print("[StatusFeedControl] events reload OK", LoggerType.Command);
            return (true, "Eventos recarregados e portal atualizado");
        }

        private static (bool, string) ReloadRules()
        {
            GameRuleXML.Reload();
            CLogger.Print("[StatusFeedControl] rules reload OK", LoggerType.Command);
            return (true, "Regras (GameRuleXML) recarregadas");
        }

        private static (bool, string) ReloadAttachments()
        {
            TemplatePackXML.Reload();
            TitleSystemXML.Reload();
            TitleAwardXML.Reload();
            MissionAwardXML.Reload();
            MissionConfigXML.Reload();
            MissionStreamXML.Reload();
            SChannelXML.Reload();
            ChannelTypeConditionManager.Reload();
            SynchronizeXML.Reload();
            SystemMapXML.Reload();
            ClanRankXML.Reload();
            PlayerRankXML.Reload();
            CouponEffectXML.Reload();
            PermissionXML.Reload();
            RandomBoxXML.Reload();
            BattleBoxXML.Reload();
            DirectLibraryXML.Reload();
            InternetCafeXML.Reload();
            RedeemCodeXML.Reload();
            CompetitiveXML.Reload();
            BattlePassManager.Reload();
            NickFilter.Reload();
            global::Server.Game.Data.XML.ChannelsXML.Reload();
            CLogger.Print("[StatusFeedControl] attachments reload OK", LoggerType.Command);
            return (true, "Anexos recarregados (titles/missions/ranks/boxes/canais/…)");
        }

        private static (bool, string) ClearLogs()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string logs = Path.Combine(root, "Logs");
            Directory.CreateDirectory(logs);

            int files = 0;
            int dirs = 0;
            foreach (string f in Directory.GetFiles(logs, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(f, FileAttributes.Normal);
                    File.Delete(f);
                    files++;
                }
                catch { /* keep going */ }
            }
            foreach (string d in Directory.GetDirectories(logs))
            {
                try
                {
                    Directory.Delete(d, true);
                    dirs++;
                }
                catch { /* keep going */ }
            }

            // JsonlSink / logger precisam das pastas de volta
            Directory.CreateDirectory(logs);
            Directory.CreateDirectory(Path.Combine(logs, "events"));

            CLogger.Print($"[StatusFeedControl] clearlogs files={files} dirs={dirs}", LoggerType.Command);
            string arq = files == 1 ? "1 arquivo" : $"{files} arquivos";
            string pas = dirs == 1 ? "1 pasta" : $"{dirs} pastas";
            return (true, $"Logs limpos: {arq}, {pas} — pastas recriadas");
        }
    }
}
