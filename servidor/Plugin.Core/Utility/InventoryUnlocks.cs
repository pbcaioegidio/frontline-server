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
    /// Arma Especial 2 — o cadeado faz FindGoods(160010901) fixo.
    /// Sem esse GoodId no packed catalog → Aviso vazio, qty 0, Confirm morto.
    /// </summary>
    public static class InventoryUnlocks
    {
        private const uint FarFutureCount = 4212312359U;

        /// <summary>GoodsId que o client Resolve no botão Gold do cadeado.</summary>
        public const int Throwing2PadlockGoodsId = 160010901;

        public const int Throwing2ItemId = 1600109;
        public const int Throwing2EffectItemId = 1707109;

        public static bool IsThrow2UnlockGood(int goodId)
        {
            int baseId = goodId / 100;
            return baseId == 1600109 || baseId == 1600110 || baseId == 1700109;
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
                        // variant 01 → GoodsId 160010901 (o único que o padlock FindGoods).
                        // consume=1 (dias), cash>0 (senão Confirm não manda 1082).
                        cmd.CommandText = @"
UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    ""Item_count_list"" = '30',
    price_cash_list = '100',
    price_gold_list = '0',
    variant_code_list = '01'
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
            int[] prefer = { Throwing2PadlockGoodsId, 160011001, 170010904, 170010901 };
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
