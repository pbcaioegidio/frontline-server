-- FrontLine: publica na loja as caixas com odds + todos os emoticons (sem renomear).
BEGIN;

-- Caixas que já têm system_random_boxes (abrir com odds)
UPDATE system_shop s
SET item_visible = true
WHERE EXISTS (
  SELECT 1 FROM system_random_boxes b WHERE b.box_id = s.item_id
);

-- Emoticons (família 41xxxxx)
UPDATE system_shop
SET item_visible = true
WHERE item_id BETWEEN 4100000 AND 4199999;

COMMIT;

SELECT 'boxes_visible' AS k, count(*)::int AS n
FROM system_shop s
WHERE EXISTS (SELECT 1 FROM system_random_boxes b WHERE b.box_id = s.item_id)
  AND s.item_visible
UNION ALL
SELECT 'emotes_visible', count(*)::int
FROM system_shop
WHERE item_id BETWEEN 4100000 AND 4199999 AND item_visible;
