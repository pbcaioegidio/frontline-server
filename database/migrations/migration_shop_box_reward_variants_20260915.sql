-- FrontLine: completa variantes das recompensas de caixa que faltam em system_shop.
-- Sem a variante o good_id nao nasce no ShopAllList e o preview da caixa trava (Please Wait).
-- Mantem item_visible=false (nao coloca o loot na vitrine; o packed inclui via ShopManager).
BEGIN;

-- PBIC 2015: so tinha 06/08/12; odds usam tambem 04 (1d)
UPDATE system_shop SET
  variant_code_list = '04,' || variant_code_list,
  "Item_count_list" = '86400,' || "Item_count_list",
  price_cash_list = '0,' || price_cash_list,
  price_gold_list = '0,' || price_gold_list
WHERE item_id IN (103219, 104218, 105120, 106058, 301104)
  AND variant_code_list IS NOT NULL
  AND variant_code_list NOT LIKE '%04%'
  AND ',' || variant_code_list || ',' NOT LIKE '%,04,%';

-- Demon Eye: so tinha 04; odds usam 06/08/12
UPDATE system_shop SET
  variant_code_list = '04,06,08,12',
  "Item_count_list" = '86400,259200,604800,2592000',
  price_cash_list = '0,0,0,0',
  price_gold_list = '1,1,1,1'
WHERE item_id IN (103274, 104286, 104288, 105167)
  AND COALESCE(variant_code_list, '') IN ('04', '4');

-- Head PBIC: so tinha 12; odds usam 04/06/08
UPDATE system_shop SET
  variant_code_list = '04,06,08,12',
  "Item_count_list" = '86400,259200,604800,2592000',
  price_cash_list = '0,0,0,0',
  price_gold_list = '1,1,1,1000'
WHERE item_id = 202070
  AND COALESCE(variant_code_list, '') IN ('12');

COMMIT;

-- Sanity: rewards das caixas devem ter variant na shop
SELECT COUNT(*) FILTER (
         WHERE NOT (',' || s.variant_code_list || ',' LIKE '%,' || lpad((r.good_id % 100)::text, 2, '0') || ',%')
       ) AS still_missing_variant
FROM (SELECT DISTINCT good_id FROM system_random_box_rewards) r
JOIN system_shop s ON s.item_id = (r.good_id / 100)::int;
