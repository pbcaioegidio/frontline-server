using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_QUEST_LIST_REQ : GameClientPacket
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
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                this.Client.SendPacket(new PROTOCOL_BASE_QUEST_LIST_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_QUEST_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
