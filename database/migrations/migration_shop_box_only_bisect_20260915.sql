-- FrontLine: bisect Please Wait — so caixa, zero emotes.
-- Hipotese: as 88 emotes (sempre ligadas nos testes anteriores) sao o gatilho, nao a caixa.
BEGIN;
UPDATE system_shop SET item_visible = false WHERE item_id BETWEEN 4100000 AND 4199999;
UPDATE system_shop SET item_visible = true,
    item_name = COALESCE(NULLIF(item_name, ''), 'Point Bomb Basic')
WHERE item_id = 1800120;
UPDATE system_shop SET item_visible = false
WHERE item_id IN (1800121, 1800131, 1800708, 1800914, 1801809);
COMMIT;
