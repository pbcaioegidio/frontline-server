using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Network.ServerPacket;
using System;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_LIST_REQ : AuthClientPacket
    {
        private byte[] _requestToken = new byte[0];

        public override void Read()
        {
            int remaining = (int)(BReader.BaseStream.Length - BReader.BaseStream.Position);
            if (remaining > 0)
                _requestToken = BReader.ReadBytes(Math.Min(remaining, 16));
        }

        public override void Run()
        {
            try
            {
                if (Client.Player == null)
                    return;

                Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_QUEST_LIST_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_QUEST_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
