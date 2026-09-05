using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_REQ : GameClientPacket
    {
        private string _clientHash;

        public override void Read()
        {
            _clientHash = ReadS(33);
        }

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                {
                    return;
                }

                Client.SendPacket(new PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
