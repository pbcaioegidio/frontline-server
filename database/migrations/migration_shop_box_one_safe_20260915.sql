-- FrontLine: uma caixa so, com odds ja presentes no catalogo packed do client.
BEGIN;

-- Esconde todas as caixas atuais
UPDATE system_shop
SET item_visible = false
WHERE item_id IN (1800120, 1800121, 1800131, 1800708, 1800914, 1801809);

-- Reescreve odds do Point Bomb Basic com goods VISIVEIS (mesmo set da presença)
DELETE FROM system_random_box_rewards WHERE box_id = 1800120;

INSERT INTO system_random_box_rewards (box_id, seq, idx, good_id, percent, is_special) VALUES
  (1800120, 0, 0, 10568404, 100, false),
  (1800120, 1, 1, 10569404,  90, false),
  (1800120, 2, 2, 10602004,  80, false),
  (1800120, 3, 3, 10632704,  70, false),
  (1800120, 4, 4, 10569904,  60, false),
  (1800120, 5, 5, 10633604,  50, false),
  (1800120, 6, 6, 10569204,  30, true),
  (1800120, 7, 7, 10633204,  30, true);

UPDATE system_random_boxes
SET items_count = 8
WHERE box_id = 1800120;

UPDATE system_shop
SET item_visible = true,
    item_name = 'Point Bomb Basic'
WHERE item_id = 1800120;

COMMIT;

SELECT item_id, item_name, item_visible FROM system_shop
WHERE item_id IN (1800120,1800121,1800131,1800708,1800914,1801809)
ORDER BY 1;

SELECT box_id, seq, good_id, percent FROM system_random_box_rewards
WHERE box_id = 1800120 ORDER BY seq;
