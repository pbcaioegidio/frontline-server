-- Libera Arma Especial 2 (ExtraGrenade) e Smoke 2 (ExtraThrowGrenade) para todos.
-- Desativa a compra quebrada na loja (gold 0 / popup Aviso vazio).

BEGIN;

-- Bit 0x80 = ExtraGrenade, bit 0x4000000 = ExtraThrowGrenade
UPDATE accounts
SET coupon_effect = coupon_effect | 128 | 67108864
WHERE (coupon_effect & 128) = 0
   OR (coupon_effect & 67108864) = 0;

-- Esconde goods de unlock pago (fluxo BuyExtendGoods / cadeado)
UPDATE system_shop_effects
SET coupon_visible = FALSE
WHERE coupon_id IN (1700035, 1700191);

-- Item ativo no inventário (client libera o cadeado ao ver o cupom [Active])
INSERT INTO player_items (owner_id, id, name, count, equip)
SELECT a.player_id, 1600035, 'Increase Grenade Slot [Active]', 4212312359, 2
FROM accounts a
WHERE NOT EXISTS (
    SELECT 1 FROM player_items pi WHERE pi.owner_id = a.player_id AND pi.id = 1600035
);

INSERT INTO player_items (owner_id, id, name, count, equip)
SELECT a.player_id, 1600191, 'Increase Smoke Slot [Active]', 4212312359, 2
FROM accounts a
WHERE NOT EXISTS (
    SELECT 1 FROM player_items pi WHERE pi.owner_id = a.player_id AND pi.id = 1600191
);

COMMIT;
