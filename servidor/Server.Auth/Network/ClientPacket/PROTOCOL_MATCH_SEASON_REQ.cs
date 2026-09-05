using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Network.ServerPacket;
using System;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_MATCH_SEASON_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Client.SendPacket((AuthServerPacket)new PROTOCOL_MATCH_SEASON_ACK());
                Client.SendPacket((AuthServerPacket)new PROTOCOL_SERVER_MESSAGE_USER_EVENT_START_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MATCH_SEASON_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
