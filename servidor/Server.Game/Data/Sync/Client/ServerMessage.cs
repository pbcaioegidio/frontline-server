using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Sync.Client
{
    public static class ServerMessage
    {
        
        public static void Load(SyncClientPacket C, GameManager Owner)
        {
            byte Length = C.ReadC();
            string A_1 = C.ReadS((int)Length);
            if (string.IsNullOrEmpty(A_1) || Length > (byte)60)
                return;
            int num = 0;
            using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(A_1))
                num = Owner.SendPacketToAllClients(Packet);
            CLogger.Print($"Message sent to '{num}' Players", LoggerType.Command);
        }
    }
}