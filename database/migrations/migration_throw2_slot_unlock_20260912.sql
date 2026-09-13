-- Arma Especial 2: client NÃO manda EXTEND se cash=0 e gold=0 (Confirm vira no-op).
-- Catálogo precisa cash > 0 (UI); servidor isenta a cobrança no EXTEND.

UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    "Item_count_list" = '1,3,7,100',
    price_cash_list = '100,270,500,1500',
    price_gold_list = '0,0,0,0',
    variant_code_list = '01,02,03,04'
WHERE item_id IN (1600109, 1600110);

INSERT INTO system_shop_effects
  (coupon_id, coupon_name, coupon_count_day_list, price_cash_list, price_gold_list,
   shop_tag, coupon_visible, discount_percent)
VALUES
  (1700109, 'Increase Throwing 2 Slot', '1,3,7,100', '100,270,500,1500', '0,0,0,0', 0, TRUE, 0)
ON CONFLICT (coupon_id) DO UPDATE SET
  coupon_name = EXCLUDED.coupon_name,
  coupon_count_day_list = '1,3,7,100',
  price_cash_list = '100,270,500,1500',
  price_gold_list = '0,0,0,0',
  coupon_visible = TRUE;

SELECT item_id, item_visible, "Item_count_list", price_cash_list, price_gold_list
FROM system_shop WHERE item_id IN (1600109, 1600110);

SELECT coupon_id, coupon_visible, coupon_count_day_list, price_cash_list
FROM system_shop_effects WHERE coupon_id = 1700109;
