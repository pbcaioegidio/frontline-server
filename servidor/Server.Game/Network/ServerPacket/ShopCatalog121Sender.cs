using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    internal static class ShopCatalog121Sender
    {
        private const int ChunkSize = 8000;

        public static void SendFullCatalog(GameClient client, Account player, bool includePackedGoods)
        {
            if (client == null || player == null)
                return;

            SendPackedItems(client);

            if (includePackedGoods)
                SendPackedGoods(client);

            SendPackedRepairs(client);

            foreach (ShopData data in BattleBoxXML.ShopDataBattleBoxes)
                client.SendPacket(new PROTOCOL_BATTLEBOX_GET_LIST_ACK(data, BattleBoxXML.TotalBoxes));

            client.SendPacket(new PROTOCOL_SHOP_FLASH_SALE_LIST_ACK(
                ShopManager.FlashSaleGoodIds, ShopManager.FlashSaleStartDate, ShopManager.FlashSaleEndMinute));

            SendPackedMatching(client, player.CafePC);

            client.SendPacket(new PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(ShopManager.CatalogVersion));
        }

        public static void SendPackedGoods(GameClient client)
        {
            SendPacked(client, ShopManager.PackedGoodsBuffer,
                (total, offset, chunk) => new PROTOCOL_AUTH_SHOP_PACKED_GOODSLIST_ACK(total, offset, chunk));
        }

        private static void SendPackedItems(GameClient client)
        {
            SendPacked(client, ShopManager.PackedItemsBuffer,
                (total, offset, chunk) => new PROTOCOL_AUTH_SHOP_ITEMLIST_ACK(total, offset, chunk));
        }

        private static void SendPackedRepairs(GameClient client)
        {
            SendPacked(client, ShopManager.PackedRepairsBuffer,
                (total, offset, chunk) => new PROTOCOL_AUTH_SHOP_REPAIRLIST_ACK(total, offset, chunk));
        }

        private static void SendPackedMatching(GameClient client, CafeEnum cafePC)
        {
            byte[] packed = cafePC == CafeEnum.None
                ? ShopManager.PackedMatching1Buffer
                : ShopManager.PackedMatching2Buffer;

            SendPacked(client, packed,
                (total, offset, chunk) => new PROTOCOL_AUTH_SHOP_PACKED_MATCHINGLIST_ACK(total, offset, chunk));
        }

        private static void SendPacked(GameClient client, byte[] packed, Func<int, int, byte[], GameServerPacket> packetFactory)
        {
            if (client == null || packed == null || packed.Length == 0)
                return;

            for (int offset = 0; offset < packed.Length; offset += ChunkSize)
            {
                int len = Math.Min(ChunkSize, packed.Length - offset);
                byte[] chunk = new byte[len];
                Array.Copy(packed, offset, chunk, 0, len);
                client.SendPacket(packetFactory(packed.Length, offset, chunk));
            }
        }
    }
}
