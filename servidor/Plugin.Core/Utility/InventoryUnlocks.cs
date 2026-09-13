using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;

namespace Plugin.Core.Utility
{
    /// <summary>
    /// Arma Especial 2 (Throwing 2 Point). O client Resolve via FindGoods(160010901).
    /// MaxDays no SYSTEM_INFO só define o teto; o cadeado abre com compra/ACK (RemainingDays).
    /// </summary>
    public static class InventoryUnlocks
    {
        private const uint FarFutureCount = 4212312359U;

        /// <summary>GoodsId do stub variant 100 dias (item 1600109 + code 04).</summary>
        public const int Throwing2GoodsIdMax = 160010904;

        public const int Throwing2ItemId = 1600109;
        public const int Throwing2EffectItemId = 1707109;

        public static void EnsureThrow2Slot(long playerId, PlayerInventory inventory)
        {
            if (inventory == null || playerId <= 0)
                return;

            if (inventory.GetItem(Throwing2ItemId) == null)
            {
                ComDiv.TryCreateItem(
                    new ItemsModel(Throwing2ItemId, "Increase Throwing 2 Slot [Active]", ItemEquipType.Temporary, FarFutureCount),
                    inventory,
                    playerId);
            }

            if (inventory.GetItem(Throwing2EffectItemId) == null)
            {
                ComDiv.TryCreateItem(
                    new ItemsModel(Throwing2EffectItemId, "Increase Throwing 2 Slot [Active]", ItemEquipType.Temporary, FarFutureCount),
                    inventory,
                    playerId);
            }
        }

        /// <summary>
        /// Goods visível e empacotável para FindGoods do cadeado (nunca AuthType=2 + count=100).
        /// </summary>
        public static void EnsureThrow2ShopCatalog()
        {
            try
            {
                using (var conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    ""Item_count_list"" = '1,3,7,100',
    price_cash_list = '0,0,0,0',
    price_gold_list = '0,0,0,0',
    variant_code_list = '01,02,03,04'
WHERE item_id IN (1600109, 1600110);

INSERT INTO system_shop_effects
  (coupon_id, coupon_name, coupon_count_day_list, price_cash_list, price_gold_list,
   shop_tag, coupon_visible, discount_percent)
VALUES
  (1700109, 'Increase Throwing 2 Slot', '1,3,7,100', '0,0,0,0', '0,0,0,0', 0, TRUE, 0)
ON CONFLICT (coupon_id) DO UPDATE SET
  coupon_name = EXCLUDED.coupon_name,
  coupon_count_day_list = '1,3,7,100',
  price_cash_list = '0,0,0,0',
  price_gold_list = '0,0,0,0',
  coupon_visible = TRUE;
";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"InventoryUnlocks.EnsureThrow2ShopCatalog: {ex.Message}", LoggerType.Warning);
            }
        }

        /// <summary>
        /// Localiza o good 160010904 (100 dias grátis) para ACK de compra sintética.
        /// </summary>
        public static GoodsItem FindThrow2MaxGoods()
        {
            lock (ShopManager.ShopBuyableList)
            {
                foreach (GoodsItem g in ShopManager.ShopBuyableList)
                {
                    if (g.Id == Throwing2GoodsIdMax)
                        return g;
                }
            }
            lock (ShopManager.ShopAllList)
            {
                foreach (GoodsItem g in ShopManager.ShopAllList)
                {
                    if (g.Id == Throwing2GoodsIdMax || g.Id == 160010901)
                        return g;
                }
            }
            return null;
        }

        public static List<GoodsItem> Throw2UnlockCart()
        {
            GoodsItem g = FindThrow2MaxGoods();
            if (g == null)
                return null;
            return new List<GoodsItem> { g };
        }
    }
}
