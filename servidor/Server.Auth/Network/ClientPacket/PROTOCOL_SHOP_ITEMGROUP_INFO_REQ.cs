using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;
using System;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_SHOP_ITEMGROUP_INFO_REQ : AuthClientPacket
    {
        private string _itemGroupHash;

        public override void Read()
        {
            _itemGroupHash = ReadS(32);
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

                if (Client.ItemGroupSent)
                {
                    return;
                }

                Client.ItemGroupSent = true;
                Client.SendPacket(new PROTOCOL_SHOP_ITEMGROUP_INFO_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_SHOP_ITEMGROUP_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
