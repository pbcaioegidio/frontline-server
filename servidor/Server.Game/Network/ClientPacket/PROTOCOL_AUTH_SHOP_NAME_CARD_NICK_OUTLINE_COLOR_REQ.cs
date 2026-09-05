using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_NAME_CARD_NICK_OUTLINE_COLOR_REQ : GameClientPacket
    {
        private byte Color;
        public override void Read()
        {
            Color = ReadC();
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
                ComDiv.UpdateDB("player_bonus", "nick_border_color", (int)Color, "owner_id", Player.PlayerId);
                Player.Bonus.NickBorderColor = (int)Color;
                Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(1, Player));
                Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(Player));
                Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("Namecard Outline Changed")));
                if (Player.Room != null)
                {
                    using (PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK packet = new PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK(Player.SlotId, Player.Bonus.NickBorderColor))
                    {
                        Player.Room.SendPacketToPlayers(packet);
                    }
                    Player.Room.UpdateSlotsInfo();
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
    }
}