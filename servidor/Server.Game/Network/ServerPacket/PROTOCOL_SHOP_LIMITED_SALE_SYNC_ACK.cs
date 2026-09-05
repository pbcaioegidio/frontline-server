using Plugin.Core.Managers;
using Plugin.Core.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SHOP_LIMITED_SALE_SYNC_ACK : GameServerPacket
    {
        private List<ItemsLimited> Items;
        public PROTOCOL_SHOP_LIMITED_SALE_SYNC_ACK()
        {
            lock (ShopManager.ItemLimited)
            {
                Items = new List<ItemsLimited>(ShopManager.ItemLimited);
            }
        }

        public override void Write()
        {
            WriteH(1105);
            WriteC((byte)Items.Count);
            foreach (ItemsLimited Item in Items)
            {
                WriteD(Item.GoodId);
                WriteD((int)Item.Remain);
            }
        }

    }
}
