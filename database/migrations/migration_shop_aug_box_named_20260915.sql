-- FrontLine: troca Caixa AUG do ID 1800708 (Non String / icone PBIC sem string no client)
-- para 1800121 (Point Bomb Premium — tem ShopItem\_Name no client).
-- Converte itens ja comprados 1800708 -> 1800121 no inventario.
BEGIN;

-- Odds AUG na 1800121
DELETE FROM system_random_box_rewards WHERE box_id = 1800121;
INSERT INTO system_random_box_rewards (box_id, seq, idx, good_id, percent, is_special) VALUES
  (1800121, 0, 0, 10303656, 100, false),
  (1800121, 1, 1, 10303657,  80, false),
  (1800121, 2, 2, 10303661,  50, false),
  (1800121, 3, 3, 10303663,  30, true),
  (1800121, 4, 4, 10303712,  40, false),
  (1800121, 5, 5, 10303708,  60, false),
  (1800121, 6, 6, 10321912,  25, true),
  (1800121, 7, 7, 10321908,  45, false);

INSERT INTO system_random_boxes (box_id, items_count, description) VALUES
  (1800121, 8, 'Caixa AUG A3')
ON CONFLICT (box_id) DO UPDATE
SET items_count = EXCLUDED.items_count,
    description = EXCLUDED.description;

UPDATE system_shop
SET item_visible = true,
    item_name = 'Caixa AUG A3',
    item_consume = 1,
    variant_code_list = '01',
    "Item_count_list" = '1',
    price_cash_list = '500',
    price_gold_list = '0',
    shop_tag = 1
WHERE item_id = 1800121;

-- Esconde PBIC (Non String) e limpa odds dela
UPDATE system_shop SET item_visible = false WHERE item_id = 1800708;
DELETE FROM system_random_box_rewards WHERE box_id = 1800708;
UPDATE system_random_boxes SET items_count = 0, description = 'deprecated Non String' WHERE box_id = 1800708;

-- Inventario: quem ja comprou 1800708 passa a ter 1800121
UPDATE player_items
SET id = 1800121,
    name = 'Caixa AUG A3'
WHERE id = 1800708;

COMMIT;
