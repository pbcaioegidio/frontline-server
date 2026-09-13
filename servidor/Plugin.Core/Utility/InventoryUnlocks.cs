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
    /// Arma Especial 2 (Throwing 2 Point).
    /// Client Shop.dat period-codes: 04=1d, 06=3d, 08=7d, 12=30d.
    /// variant_code 01 → UI qty 0 → Confirm no-op (sem EXTEND_REQ).
    /// </summary>
    public static class InventoryUnlocks
    {
        private const uint FarFutureCount = 4212312359U;

        // GoodsId = itemId + period code (ex.: 160010912 = 30 dias)
        public const int Throwing2GoodsId30d = 160010912;
        public const int Throwing2GoodsId1d = 160010904;

        public const int Throwing2ItemId = 1600109;
        public const int Throwing2EffectItemId = 1707109;

        public static bool IsThrow2UnlockGood(int goodId)
        {
            int baseId = goodId / 100;
            // period-code goods: 160010904 / 06 / 08 / 12 (div 100 → 1600109)
            // effect goods: 170010901..04
            return baseId == 1600109 || baseId == 1600110 || baseId == 1700109
                || goodId == 160010904 || goodId == 160010906
                || goodId == 160010908 || goodId == 160010912;
        }

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

        public static void EnsureThrow2ShopCatalog()
        {
            try
            {
                using (var conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        // Period codes oficiais (não 01,02,03,04 — isso gera qty 0 na UI).
                        cmd.CommandText = @"
UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    ""Item_count_list"" = '1,3,7,30',
    price_cash_list = '100,270,500,1500',
    price_gold_list = '0,0,0,0',
    variant_code_list = '04,06,08,12'
WHERE item_id IN (1600109, 1600110);

INSERT INTO system_shop_effects
  (coupon_id, coupon_name, coupon_count_day_list, price_cash_list, price_gold_list,
   shop_tag, coupon_visible, discount_percent)
VALUES
  (1700109, 'Increase Throwing 2 Slot', '1,3,7,30', '100,270,500,1500', '0,0,0,0', 0, TRUE, 0)
ON CONFLICT (coupon_id) DO UPDATE SET
  coupon_name = EXCLUDED.coupon_name,
  coupon_count_day_list = '1,3,7,30',
  price_cash_list = '100,270,500,1500',
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

        public static GoodsItem FindThrow2MaxGoods()
        {
            int[] prefer =
            {
                Throwing2GoodsId30d, // 160010912
                160010908, 160010906, Throwing2GoodsId1d,
                170010904, 170010901
            };
            lock (ShopManager.ShopBuyableList)
            {
                foreach (int id in prefer)
                {
                    foreach (GoodsItem g in ShopManager.ShopBuyableList)
                    {
                        if (g.Id == id)
                            return g;
                    }
                }
            }
            lock (ShopManager.ShopAllList)
            {
                foreach (int id in prefer)
                {
                    foreach (GoodsItem g in ShopManager.ShopAllList)
                    {
                        if (g.Id == id)
                            return g;
                    }
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
