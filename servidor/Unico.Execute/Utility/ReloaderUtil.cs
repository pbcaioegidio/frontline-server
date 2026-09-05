using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.XML;
using System;

namespace Executable.Utility
{
    public class ReloaderUtil
    {
        public static bool ReloadConfig()
        {
            try
            {
                //ServerConfigJSON.Configs
                ServerConfigJSON.Load();
                NickFilter.Filters.Clear();
                NickFilter.Load();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        public static bool ReloadShop()
        {
            try
            {
                ShopManager.Reset();
                ShopManager.Load(1);
                ShopManager.Load(2);
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        public static bool ReloadEvent()
        {
            try
            {
                //EventLoginJSON.Reload();
                EventLoginXML.Reload();
                //EventBoostJSON.Reload();
                EventBoostXML.Reload();
                //EventPlaytimeJSON.Reload();
                EventPlaytimeJSON.Reload();
                //EventQuestJSON.Reload();
                EventQuestXML.Reload();
                //EventRankUpJSON.Reload();
                EventRankUpXML.Reload();
                //EventVisitJSON.Reload();
                EventVisitXML.Reload();
                //EventXmasJSON.Reload();
                EventXmasXML.Reload();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        public static bool ReloadRules()
        {
            try
            {
                GameRuleXML.Reload();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
        public static bool ReloadAttachments()
        {
            try
            {
                TemplatePackXML.Load();
                SystemMapXML.Load();
                RandomBoxXML.Load();
                BattleBoxXML.Load();
                InternetCafeXML.Cafes.Clear();
                InternetCafeXML.Load();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
    }
}
