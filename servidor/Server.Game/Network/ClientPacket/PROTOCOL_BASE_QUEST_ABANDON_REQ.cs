using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_ABANDON_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                if (this.Client.GetAccount() == null)
                    return;
                this.Client.SendPacket(new PROTOCOL_BASE_QUEST_ABANDON_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_QUEST_ABANDON_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
