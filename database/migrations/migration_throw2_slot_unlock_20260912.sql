-- Arma Especial 2 / Throwing 2 Point (BuyExtend).
-- GoodsId real: 170010901 (cupom system_shop_effects 1700109), NÃO o stub system_shop 1600109.
-- Stub 1600109 com item_consume=2 + count=100 → UI mostra seletor "0" e "0 Gold".

-- 1) Esconde stub quebrado (GoodsId 160010901)
UPDATE system_shop
SET item_visible = false
WHERE item_id IN (1600109, 1600110);

-- 2) Produto real do cadeado (espelho de Increase Grenade Slot 1700035)
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
  coupon_visible = FALSE,
  discount_percent = 0;

-- 3) Limpa grants errados (1600109 não abre o cadeado no client)
DELETE FROM player_items WHERE id = 1600109;

-- 4) Item ativo que o client conhece:
--    LoadShopEffects: ItemId = 17 + DD + 109 → dia 7 = 1707109; dia 30 = 1730109
--    count = yyMMddHHmm (Temporary / equip=2)
INSERT INTO player_items (owner_id, id, name, count, equip)
SELECT a.player_id, 1707109, 'Increase Throwing 2 Slot [Active]', 4212312359, 2
FROM accounts a
WHERE NOT EXISTS (
  SELECT 1 FROM player_items pi
  WHERE pi.owner_id = a.player_id AND pi.id IN (1707109, 1701109, 1703109, 1730109, 17100109)
);

SELECT coupon_id, coupon_name, coupon_visible, price_gold_list, coupon_count_day_list
FROM system_shop_effects WHERE coupon_id = 1700109;

SELECT item_id, item_name, item_visible FROM system_shop WHERE item_id IN (1600109, 1600110);
