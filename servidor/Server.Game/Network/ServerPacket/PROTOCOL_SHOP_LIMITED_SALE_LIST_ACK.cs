using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SHOP_LIMITED_SALE_LIST_ACK : GameServerPacket
    {
        private List<ItemsLimited> Items;

        public PROTOCOL_SHOP_LIMITED_SALE_LIST_ACK()
        {
            lock (ShopManager.ItemLimited)
            {
                Items = new List<ItemsLimited>(ShopManager.ItemLimited);
            }
        }

        public override void Write()
        {
            byte[] raw = new byte[Items.Count * 28];
            for (int i = 0; i < Items.Count; i++)
            {
                ItemsLimited item = Items[i];
                int offset = i * 28;
                WriteIntLE(raw, offset + 0, 0);
                WriteIntLE(raw, offset + 4, item.GoodId);
                WriteIntLE(raw, offset + 8, unchecked((int)item.StartDate));
                WriteIntLE(raw, offset + 12, unchecked((int)item.EndDate));
                WriteIntLE(raw, offset + 16, item.SaleType);
                WriteIntLE(raw, offset + 20, unchecked((int)item.Remain));
                WriteIntLE(raw, offset + 24, 0);
            }

            byte[] packed = ZlibUtil.Compress(raw);
            WriteH(1103);
            WriteD(Items.Count);
            WriteD(0);
            WriteD(packed.Length);
            WriteB(packed);
        }

        private static void WriteIntLE(byte[] buffer, int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Buffer.BlockCopy(bytes, 0, buffer, offset, 4);
        }
    }
}
