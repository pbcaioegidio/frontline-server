using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;

namespace Plugin.Core.Utility
{
    /// <summary>
    /// Desbloqueios permanentes de slots de inventário/equipamento que no client
    /// oficial exigem cupom pago (BuyExtendGoods). Aqui liberamos de graça para
    /// todos — o popup de "Arma Especial 2" com Gold 0 vinha do good 1700035
    /// (Increase Grenade Slot) com price_gold_list zerado.
    /// </summary>
    public static class InventoryUnlocks
    {
        // yyMMddHHmm longe o bastante para o client tratar como ativo permanente.
        // Precisa caber em uint (max ~42xxxxxx = 2042).
        private const uint FarFutureCount = 4212312359U;

        public static CouponEffects EnsureFreeExtraSlots(long playerId, CouponEffects effects, PlayerInventory inventory)
        {
            if (inventory == null || playerId <= 0)
                return effects;

            effects = EnsureEffectAndItem(
                playerId,
                effects,
                inventory,
                CouponEffects.ExtraGrenade,
                EffectId.ExtraGrenade,
                "Increase Grenade Slot [Active]");

            effects = EnsureEffectAndItem(
                playerId,
                effects,
                inventory,
                CouponEffects.ExtraThrowGrenade,
                EffectId.IncreaseSmokeSlot,
                "Increase Smoke Slot [Active]");

            return effects;
        }

        private static CouponEffects EnsureEffectAndItem(
            long playerId,
            CouponEffects effects,
            PlayerInventory inventory,
            CouponEffects flag,
            int itemId,
            string activeName)
        {
            if (!effects.HasFlag((Enum)flag))
            {
                effects |= flag;
                DaoManagerSQL.UpdateCouponEffect(playerId, effects);
            }

            if (inventory.GetItem(itemId) == null)
            {
                ItemsModel item = new ItemsModel(itemId, activeName, ItemEquipType.Temporary, FarFutureCount);
                ComDiv.TryCreateItem(item, inventory, playerId);
            }

            return effects;
        }
    }
}
