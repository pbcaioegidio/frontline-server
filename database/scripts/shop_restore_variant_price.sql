-- Desfaz shop_fix_variant_price.sql a partir do backup.
-- Devolve variante, contagem, preço e visibilidade ao estado anterior à correção.

BEGIN;

UPDATE system_shop s
SET variant_code_list = b.variant_code_list,
    "Item_count_list" = b.item_count_list,
    price_gold_list   = b.price_gold_list,
    price_cash_list   = b.price_cash_list,
    item_visible      = b.item_visible
FROM system_shop_price_backup_launch b
WHERE s.item_id = b.item_id;

COMMIT;

SELECT count(*) AS linhas_restauradas FROM system_shop_price_backup_launch;
