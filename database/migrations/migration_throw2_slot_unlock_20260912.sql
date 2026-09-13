-- Cadeado FindGoods(160010901). Sem esse GoodId → Aviso qty 0 / Confirm morto.
-- variant 01 + consume=1 + cash>0.

UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    item_consume = 1,
    "Item_count_list" = '30',
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
  coupon_count_day_list = '1,3,7,30',
  price_cash_list = '100,270,500,1500',
  price_gold_list = '0,0,0,0',
  coupon_visible = TRUE;

SELECT item_id, variant_code_list, "Item_count_list", price_cash_list, item_consume
FROM system_shop WHERE item_id = 1600109;
