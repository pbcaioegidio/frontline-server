-- FrontLine: so publica caixas "seguras" (Point Bomb + FaceBook).
-- Gachas PBIC/Demon/GunZeeD crasham o client ao abrir odds (itens sem PEF/ShopItem no client).
BEGIN;

-- Esconde gachas problematicas
UPDATE system_shop
SET item_visible = false
WHERE item_id IN (1800708, 1800914, 1801809);

-- Garante nomes nas que ficam (evita Non String / NO NAME CASHITEM)
UPDATE system_shop SET item_name = 'Point Bomb Basic' WHERE item_id = 1800120 AND COALESCE(item_name,'') = '';
UPDATE system_shop SET item_name = 'Point Bomb Premium' WHERE item_id = 1800121 AND COALESCE(item_name,'') = '';
UPDATE system_shop SET item_name = 'FaceBook Box' WHERE item_id = 1800131 AND COALESCE(item_name,'') = '';

-- Confirma visiveis
UPDATE system_shop SET item_visible = true
WHERE item_id IN (1800120, 1800121, 1800131);

COMMIT;

SELECT item_id, item_name, item_visible
FROM system_shop
WHERE item_id IN (1800120,1800121,1800131,1800708,1800914,1801809)
ORDER BY item_id;
