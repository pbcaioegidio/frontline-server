using Plugin.Core.Network;
using Server.Auth.Data.Models;
using Server.Auth.Data.XML;


namespace Server.Auth.Data.Sync.Client
{
    public class ChannelCache
    {
        public static void Load(SyncClientPacket C)
        {
            int ServerId = C.ReadD();
            int num1 = C.ReadD();
            int num2 = C.ReadD();
            int Id = num1;
            ChannelModel channel = ChannelsXML.GetChannel(ServerId, Id);
            if (channel == null)
                return;
            channel.TotalPlayers = num2;
        }
    }
}