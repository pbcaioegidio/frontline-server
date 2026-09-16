-- FrontLine: esconde emotes e caixas da vitrine (pedido do user).
BEGIN;

UPDATE system_shop
SET item_visible = false
WHERE item_id BETWEEN 4100000 AND 4199999;

UPDATE system_shop s
SET item_visible = false
WHERE EXISTS (SELECT 1 FROM system_random_boxes b WHERE b.box_id = s.item_id);

COMMIT;
