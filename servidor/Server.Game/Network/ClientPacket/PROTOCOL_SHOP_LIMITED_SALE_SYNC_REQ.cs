using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_SHOP_LIMITED_SALE_SYNC_REQ : GameClientPacket
    {
        private int GoodId;

        public override void Read()
        {

        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;


                if (ShopManager.ItemLimited != null && ShopManager.ItemLimited.Count > 0)
                {
                    this.Client.SendPacket(new PROTOCOL_SHOP_LIMITED_SALE_SYNC_ACK());
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_SHOP_LIMITED_SALE_SYNC_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }

    }
}
