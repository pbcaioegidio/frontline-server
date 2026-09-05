using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;

namespace Server.Game.Data.Sync.Client
{
    public class ServerCache
    {
        public static void Load(SyncClientPacket C)
        {
            int id = C.ReadD();
            int num = C.ReadD();
            SChannelModel server = SChannelXML.GetServer(id);
            if (server == null)
                return;
            server.LastPlayers = num;
        }
    }
}