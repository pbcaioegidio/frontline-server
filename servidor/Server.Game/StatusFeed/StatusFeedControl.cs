using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.XML;
using System;
using System.IO;

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

                if (cmd != "reload")
                    return (false, "cmd desconhecido (use reload|clearlogs)");

                switch (target)
                {
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
                        return (false, "target inválido: config|shop|events|rules|attachments");
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[StatusFeedControl] " + ex.Message, LoggerType.Warning, ex);
                return (false, ex.Message);
            }
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
            foreach (var manager in GameXender.All)
                GameXender.UpdateEvents(manager);
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
            return (true, "Anexos recarregados (titles/missions/ranks/boxes/…)");
        }

        private static (bool, string) ClearLogs()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            string logs = Path.Combine(root, "Logs");
            if (!Directory.Exists(logs))
                return (true, "Pasta Logs/ inexistente — nada a limpar");

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
            CLogger.Print($"[StatusFeedControl] clearlogs files={files} dirs={dirs}", LoggerType.Command);
            return (true, $"Logs limpos ({files} arquivo(s), {dirs} pasta(s))");
        }
    }
}
