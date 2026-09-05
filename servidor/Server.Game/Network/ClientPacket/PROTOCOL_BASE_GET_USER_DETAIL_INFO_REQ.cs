using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_USER_DETAIL_INFO_REQ : GameClientPacket
    {
        private int SessionId;

        public override void Read() => SessionId = ReadD();

        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    return;
                }
                PlayerSession Session = Player.GetChannel().GetPlayer(SessionId);
                if (Session != null)
                {
                    Account account = AccountManager.GetAccount(Session.PlayerId, true);
                    if (account != null)
                    {
                        if (Player.Nickname != account.Nickname)
                        {
                            Player.FindPlayer = account.Nickname;
                        }
                        Client.SendPacket(new PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK(0U, account, int.MaxValue));
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_USER_STATISTICS_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}