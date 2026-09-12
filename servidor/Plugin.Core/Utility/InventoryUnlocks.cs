using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;

namespace Plugin.Core.Utility
{
    /// <summary>
    /// Unlock permanente do slot Arma Especial 2 (Throwing 2 Point / BuyExtend 1700109).
    /// O stub system_shop 1600109 gera o popup "0 Gold"; o client usa cupom 17xxxxxx.
    /// </summary>
    public static class InventoryUnlocks
    {
        // yyMMddHHmm longe o bastante para Temporary (equip=2); cabe em uint.
        private const uint FarFutureCount = 4212312359U;

        // LoadShopEffects: ItemId = "17" + day:D2 + "109" → 7 dias = 1707109
        public const int Throwing2SlotItemId = 1707109;

        public static void EnsureThrow2Slot(long playerId, PlayerInventory inventory)
        {
            if (inventory == null || playerId <= 0)
                return;

            if (HasThrow2UnlockItem(inventory))
                return;

            ItemsModel item = new ItemsModel(
                Throwing2SlotItemId,
                "Increase Throwing 2 Slot [Active]",
                ItemEquipType.Temporary,
                FarFutureCount);

            ComDiv.TryCreateItem(item, inventory, playerId);
        }

        private static bool HasThrow2UnlockItem(PlayerInventory inventory)
        {
            // Variantes de período geradas por LoadShopEffects para coupon 1700109
            int[] ids = { 1701109, 1703109, 1707109, 1730109, 17100109 };
            foreach (int id in ids)
            {
                if (inventory.GetItem(id) != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Garante cupom 1700109 na DB e esconde stub 1600109 (chamar no Load da shop).
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
SET item_visible = false
WHERE item_id IN (1600109, 1600110);

INSERT INTO system_shop_effects
  (coupon_id, coupon_name, coupon_count_day_list, price_cash_list, price_gold_list,
   shop_tag, coupon_visible, discount_percent)
VALUES
  (1700109, 'Increase Throwing 2 Slot', '1,3,7,30', '0,0,0,0', '0,0,0,0', 0, FALSE, 0)
ON CONFLICT (coupon_id) DO UPDATE SET
  coupon_name = EXCLUDED.coupon_name,
  coupon_count_day_list = EXCLUDED.coupon_count_day_list,
  price_cash_list = '0,0,0,0',
  price_gold_list = '0,0,0,0',
  coupon_visible = FALSE;
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
    }
}
