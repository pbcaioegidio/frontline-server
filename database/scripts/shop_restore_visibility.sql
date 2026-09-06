-- Restaura visibilidade da loja a partir do backup de lançamento.
-- NÃO apaga itens. Só devolve item_visible ao estado anterior.

BEGIN;

UPDATE system_shop s
SET item_visible = b.item_visible
FROM system_shop_visibility_backup_launch b
WHERE s.item_id = b.item_id;

COMMIT;

SELECT
  count(*) FILTER (WHERE item_visible) AS visiveis,
  count(*) FILTER (WHERE NOT item_visible) AS ocultos
FROM system_shop;
