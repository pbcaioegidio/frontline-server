using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.Network;
using Plugin.Core.XML;

namespace Server.Game.Data.Sync.Client
{
    public class SupervisorReload
    {
        public static void Load(SyncClientPacket C, GameManager Owner)
        {
            int subCmd = C.ReadC();
            uint broadcastId = (uint)C.ReadD();

            // Auth, the game channels and Match may share this process and these statics, so the
            // shared reload runs for the first endpoint that claims the broadcast; anything tied
            // to this channel's own manager still runs on every endpoint.
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
                    var gameConfig = ServerConfigJSON.GetConfig(ConfigLoader.ConfigId);
                    if (gameConfig != null)
                        foreach (GameManager manager in GameXender.All)
                            manager.Config = gameConfig;
                    break;

                case 2:
                    if (shared)
                    {
                        ShopManager.Reset();
                        ShopManager.Load(1);
                        ShopManager.Load(2);
                    }
                    GameXender.UpdateShop(Owner);
                    break;

                case 3:
                    if (shared)
                    {
                        EventLoginXML.Reload();
                        EventBoostXML.Reload();
                        EventPlaytimeJSON.Reload();
                        EventQuestXML.Reload();
                        EventRankUpXML.Reload();
                        EventVisitXML.Reload();
                        EventXmasXML.Reload();
                    }
                    GameXender.UpdateEvents(Owner);
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
                    break;
            }
            if (shared)
                CLogger.Print($"[SupervisorReload] subCmd={subCmd} applied.", LoggerType.Command);
        }
    }
}
