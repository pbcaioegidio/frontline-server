using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_SYSTEM_INFO_REQ : AuthClientPacket
    {
        public override void Read()
        {
            ReadC(); //0
        }

        public override void Run()
        {
            try
            {
                Account Player = Client.Player;
                if (Player == null)
                {
                    return;
                }
                ServerConfig CFG = AuthXender.Client.Config;
                if (CFG != null)
                {
                    Client.SendPacket(new PROTOCOL_BASE_NOTICE_ACK(CFG));
                    if (CFG.OfficialBannerEnabled)
                    {
                        Client.SendPacket(new PROTOCOL_BASE_URL_LIST_ACK(CFG));
                    }
                    Client.SendPacket(new PROTOCOL_BASE_BOOSTEVENT_INFO_ACK());
                    Client.SendPacket(new PROTOCOL_BASE_CHANNELTYPE_CONDITION_ACK());
                    Client.SendPacket(new PROTOCOL_BASE_GET_SYSTEM_INFO_ACK(CFG, Player.Mission));
                    Client.SendPacket(new PROTOCOL_BASE_BATTLE_MODE_WEAPON_INFO_ACK());
                }
                List<MessageModel> Gifts = DaoManagerSQL.GetGiftMessages(Player.PlayerId);
                if (Gifts.Count > 0)
                {
                    DaoManagerSQL.RecycleMessages(Player.PlayerId, Gifts);
                    if (Gifts.Count > 0)
                    {
                        Client.SendPacket(new PROTOCOL_BASE_USER_GIFTLIST_ACK(0, Gifts)); //need fix
                    }
                }
                Client.SendMapInfoOnce(); // ensure CreateRoom rule book is loaded even if client never sends GET_MAP_INFO_REQ
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
    }
}