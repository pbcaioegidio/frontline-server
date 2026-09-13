-- Arma Especial 2: o client faz FindGoods do GoodsId 160010901 (stub system_shop).
-- Visibility=false tira do packed catalog → popup vazio / qty 0 / 0 Gold sem EXTEND_REQ.
-- AuthType/consume=2 + count=100 → UI mostra "0" horas. Usar dias (consume=1).

-- Stub visível, grátis, com dias reais (GoodsId 160010901/02/03/04)
UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    "Item_count_list" = '1,3,7,100',
    price_cash_list = '0,0,0,0',
    price_gold_list = '0,0,0,0',
    variant_code_list = '01,02,03,04'
WHERE item_id = 1600109;

UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot (Alt)',
    item_visible = true,
    item_consume = 1,
    "Item_count_list" = '1,3,7,100',
    price_cash_list = '0,0,0,0',
    price_gold_list = '0,0,0,0',
    variant_code_list = '01,02,03,04'
WHERE item_id = 1600110;

-- Cupom 17 espelho (visível no catálogo de effects)
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

SELECT item_id, item_visible, item_consume, "Item_count_list", price_gold_list, variant_code_list
FROM system_shop WHERE item_id IN (1600109, 1600110);

SELECT coupon_id, coupon_visible, coupon_count_day_list, price_gold_list
FROM system_shop_effects WHERE coupon_id = 1700109;
