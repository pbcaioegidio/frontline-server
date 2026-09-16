-- FrontLine: religa emotes + publica Caixa AUG A3 (reusa 1800708, asset de gacha no client).
-- Odds omitidas no RandomBox.dat da loja (server-v202609.30.16+); abertura via inventario/server.
BEGIN;

-- Emotes de volta
UPDATE system_shop
SET item_visible = true
WHERE item_id BETWEEN 4100000 AND 4199999;

-- Caixa AUG A3: reusa PBIC gacha slot (mesh/icone existem no client)
UPDATE system_shop
SET item_visible = true,
    item_name = 'Caixa AUG A3',
    item_consume = 1,
    variant_code_list = '01',
    "Item_count_list" = '1',
    price_cash_list = '500',
    price_gold_list = '0',
    shop_tag = 1
WHERE item_id = 1800708;

-- Odds: variantes da AUG A3 base (ja visivel/packed) + skins com variant no shop
DELETE FROM system_random_box_rewards WHERE box_id = 1800708;

INSERT INTO system_random_box_rewards (box_id, seq, idx, good_id, percent, is_special) VALUES
  (1800708, 0, 0, 10303656, 100, false),  -- AUG A3 (qty menor)
  (1800708, 1, 1, 10303657,  80, false),
  (1800708, 2, 2, 10303661,  50, false),
  (1800708, 3, 3, 10303663,  30, true),   -- AUG A3 (melhor qty)
  (1800708, 4, 4, 10303712,  40, false),  -- AUG A3 G 30d
  (1800708, 5, 5, 10303708,  60, false),  -- AUG A3 G 7d
  (1800708, 6, 6, 10321912,  25, true),   -- AUG A3 PBIC 2015 30d
  (1800708, 7, 7, 10321908,  45, false);

INSERT INTO system_random_boxes (box_id, items_count, description) VALUES
  (1800708, 8, 'Caixa AUG A3')
ON CONFLICT (box_id) DO UPDATE
SET items_count = EXCLUDED.items_count,
    description = EXCLUDED.description;

-- Garante variants das skins nas odds (sem publicar na vitrine)
UPDATE system_shop SET
  variant_code_list = '04,06,08,12,05',
  "Item_count_list" = '86400,259200,604800,2592000,1',
  price_cash_list = '400,800,1200,4000,0',
  price_gold_list = '0,0,0,0,1'
WHERE item_id = 103037
  AND (variant_code_list IS NULL OR variant_code_list NOT LIKE '%08%');

UPDATE system_shop SET
  variant_code_list = '04,06,08,12',
  "Item_count_list" = '86400,259200,604800,2592000',
  price_cash_list = '0,800,1200,4000',
  price_gold_list = '1,0,0,0'
WHERE item_id = 103219
  AND (variant_code_list IS NULL OR variant_code_list NOT LIKE '%04%');

-- Mantem Point Bomb; esconde outras gachas problematicas
UPDATE system_shop SET item_visible = true WHERE item_id = 1800120;
UPDATE system_shop SET item_visible = false
WHERE item_id IN (1800121, 1800131, 1800914, 1801809);

COMMIT;
