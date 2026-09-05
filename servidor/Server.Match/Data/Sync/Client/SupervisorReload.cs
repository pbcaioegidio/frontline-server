using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Network;
using Server.Match.Data.XML;

namespace Server.Match.Data.Sync.Client
{
    public class SupervisorReload
    {
        public static void Load(SyncClientPacket C)
        {
            int subCmd = C.ReadC();
            uint broadcastId = (uint)C.ReadD();

            if (subCmd != 5)
                return;

            // These statics are shared with Auth and the game channels when they run in the same
            // process, so only the endpoint that claims the broadcast reloads them.
            if (!ReloadCoordinator.ClaimShared(subCmd, broadcastId))
                return;

            MapStructureXML.Reload();
            CharaStructureXML.Reload();
            ItemStatisticXML.Reload();
            CLogger.Print($"[SupervisorReload] subCmd={subCmd} applied.", LoggerType.Command);
        }
    }
}
