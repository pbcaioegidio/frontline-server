-- FrontLine: esconde Point Bomb (odds Point Up crasham preview) e mantem Caixa AUG A3.
-- Tambem tenta limpar nome no shop (client PEF de 1800708 ainda pode mostrar Non String).
BEGIN;

UPDATE system_shop
SET item_visible = false
WHERE item_id = 1800120;

UPDATE system_shop
SET item_visible = true,
    item_name = 'Caixa AUG A3',
    shop_tag = 1
WHERE item_id = 1800708;

-- Emotes continuam
UPDATE system_shop
SET item_visible = true
WHERE item_id BETWEEN 4100000 AND 4199999;

COMMIT;
