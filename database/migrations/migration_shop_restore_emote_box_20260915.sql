-- FrontLine: religa emotes + 1 caixa segura apos o baseline ter aberto sem crash.
-- Gachas de arma (PBIC/Demon/GunZeeD) ficam ocultas — elas crasham o preview do client.
BEGIN;

-- Emotes (Emotion tab)
UPDATE system_shop
SET item_visible = true
WHERE item_id BETWEEN 4100000 AND 4199999;

-- Uma caixa so: Point Bomb Basic (odds Point Up ja no DB)
UPDATE system_shop
SET item_visible = true,
    item_name = COALESCE(NULLIF(item_name, ''), 'Point Bomb Basic')
WHERE item_id = 1800120;

-- Mantem as outras caixas ocultas
UPDATE system_shop
SET item_visible = false
WHERE item_id IN (1800121, 1800131, 1800708, 1800914, 1801809);

COMMIT;
