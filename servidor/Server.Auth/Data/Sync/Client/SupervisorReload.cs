using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.Network;
using Plugin.Core.XML;

namespace Server.Auth.Data.Sync.Client
{
    public class SupervisorReload
    {
        public static void Load(SyncClientPacket C)
        {
            int subCmd = C.ReadC();
            uint broadcastId = (uint)C.ReadD();

            // Auth, the game channels and Match may share this process and these statics, so the
            // shared reload runs for the first endpoint that claims the broadcast; anything tied
            // to this service's own instance still runs on every endpoint.
            bool shared = ReloadCoordinator.ClaimShared(subCmd, broadcastId);

            switch (subCmd)
            {
                case 1:
                    if (shared)
                    {
                        ServerConfigJSON.Reload();
                        CommandHelperJSON.Reload();
                        ResolutionJSON.Reload();
                    }
                    var authConfig = ServerConfigJSON.GetConfig(ConfigLoader.ConfigId);
                    if (authConfig != null)
                        AuthXender.Client.Config = authConfig;
                    break;

                case 2:
                    if (!shared)
                        break;
                    ShopManager.Reset();
                    ShopManager.Load(1);
                    ShopManager.Load(2);
                    break;

                case 3:
                    if (!shared)
                        break;
                    EventLoginXML.Reload();
                    EventBoostXML.Reload();
                    EventPlaytimeJSON.Reload();
                    EventQuestXML.Reload();
                    EventRankUpXML.Reload();
                    EventVisitXML.Reload();
                    EventXmasXML.Reload();
                    break;

                case 4:
                    if (!shared)
                        break;
                    GameRuleXML.Reload();
                    break;

                case 5:
                    if (!shared)
                        break;
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
                    global::Server.Auth.Data.XML.ChannelsXML.Reload();
                    break;
            }
            if (shared)
                CLogger.Print($"[SupervisorReload] subCmd={subCmd} applied.", LoggerType.Command);
        }
    }
}
