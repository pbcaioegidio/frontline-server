-- FrontLine: restaura odds originais Point Up na Point Bomb Basic.
-- Armas no preview da gacha crasham o client (0xC0000005 @ clique).
-- Point Up (cat 20) e o tipo certo desta caixa; precisa estar packed+matching.
BEGIN;

DELETE FROM system_random_box_rewards WHERE box_id = 1800120;

INSERT INTO system_random_box_rewards (box_id, seq, idx, good_id, percent, is_special) VALUES
  (1800120, 0, 0, 200000551, 100, false),
  (1800120, 1, 1, 200001051, 100, false),
  (1800120, 2, 2, 200003051, 100, false),
  (1800120, 3, 3, 200005051,  50, false),
  (1800120, 4, 4, 200010051,  50, false),
  (1800120, 5, 5, 200030001,  50, false),
  (1800120, 6, 6, 200050051,  40, false),
  (1800120, 7, 7, 200100001,  30, false),
  (1800120, 8, 8, 200200001,  20, false);

UPDATE system_random_boxes SET items_count = 9 WHERE box_id = 1800120;

-- Garante variants que geram exatamente esses good_id
UPDATE system_shop SET variant_code_list = '51', "Item_count_list" = '1', item_consume = 1, price_gold_list = '1', price_cash_list = '0'
WHERE item_id IN (2000005, 2000010, 2000030, 2000100, 2000500);

UPDATE system_shop SET variant_code_list = '51,64', "Item_count_list" = '1,2', item_consume = 1,
  price_gold_list = '1,0', price_cash_list = '0,0'
WHERE item_id = 2000050;

UPDATE system_shop SET variant_code_list = '01', "Item_count_list" = '1', item_consume = 1, price_gold_list = '300', price_cash_list = '0'
WHERE item_id = 2000300;

UPDATE system_shop SET variant_code_list = '01', "Item_count_list" = '1', item_consume = 1, price_gold_list = '1000', price_cash_list = '0'
WHERE item_id = 2001000;

UPDATE system_shop SET variant_code_list = '01', "Item_count_list" = '1', item_consume = 1, price_gold_list = '2000', price_cash_list = '0'
WHERE item_id = 2002000;

-- So esta caixa na loja
UPDATE system_shop SET item_visible = false
WHERE item_id IN (1800121, 1800131, 1800708, 1800914, 1801809);

UPDATE system_shop SET item_visible = true, item_name = 'Point Bomb Basic'
WHERE item_id = 1800120;

COMMIT;

SELECT r.box_id, r.seq, r.good_id, s.item_name, s.variant_code_list
FROM system_random_box_rewards r
LEFT JOIN system_shop s ON s.item_id = (r.good_id / 100)::int
WHERE r.box_id = 1800120
ORDER BY r.seq;
