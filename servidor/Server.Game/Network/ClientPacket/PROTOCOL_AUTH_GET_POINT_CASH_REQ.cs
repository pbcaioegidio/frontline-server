using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_GET_POINT_CASH_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = Client.Player;
                if (player == null)
                {
                    return;
                }
                Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));
                if (player.RefreshLobbyInfo)
                {
                    player.RefreshLobbyInfo = false;
                    Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(player));
                    Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_RECORD_ACK(player.Statistic));
                    Client.SendPacket(new PROTOCOL_BASE_GET_CHARA_INFO_ACK(player));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}