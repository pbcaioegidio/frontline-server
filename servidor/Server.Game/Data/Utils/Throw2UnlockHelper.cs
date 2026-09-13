using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Collections.Generic;

namespace Server.Game.Data.Utils
{
    internal static class Throw2UnlockHelper
    {
        /// <summary>
        /// Após SendFullCatalog: simula compra do cupom 1700109 para setar RemainingDays.
        /// Idempotente por sessão (não floodar).
        /// </summary>
        public static void TrySendAfterShopCatalog(GameClient client, Account player)
        {
            if (client == null || player == null || player.Throw2UnlockAckSent)
                return;

            List<GoodsItem> cart = InventoryUnlocks.Throw2UnlockCart();
            if (cart == null || cart.Count == 0)
                return;

            player.Throw2UnlockAckSent = true;
            client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, cart));
            client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(1U, cart, player));
            CLogger.Print($"Throw2 unlock ACK PlayerId={player.PlayerId} GoodId={cart[0].Id}", LoggerType.Info);
        }
    }
}
