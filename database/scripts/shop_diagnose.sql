SELECT shop_tag, count(*) AS qtd, count(*) FILTER (WHERE item_visible) AS visible
FROM system_shop GROUP BY shop_tag ORDER BY shop_tag;

SELECT
  count(*) FILTER (WHERE item_visible) AS visiveis,
  count(*) FILTER (WHERE NOT item_visible) AS ocultos,
  count(*) FILTER (WHERE item_visible AND shop_tag = 0) AS tag0_normal,
  count(*) FILTER (WHERE item_visible AND shop_tag = 1) AS tag1_new,
  count(*) FILTER (WHERE item_visible AND shop_tag = 2) AS tag2_hot
FROM system_shop;

-- faixas de item_id (tipo aproximado PB)
SELECT
  CASE
    WHEN item_id BETWEEN 100000 AND 103999 THEN 'AR/rifle'
    WHEN item_id BETWEEN 104000 AND 104999 THEN 'SMG'
    WHEN item_id BETWEEN 105000 AND 105999 THEN 'Sniper'
    WHEN item_id BETWEEN 106000 AND 106999 THEN 'Shotgun'
    WHEN item_id BETWEEN 107000 AND 199999 THEN 'outras armas'
    WHEN item_id BETWEEN 200000 AND 299999 THEN 'pistola'
    WHEN item_id BETWEEN 300000 AND 399999 THEN 'faca'
    WHEN item_id BETWEEN 1000000 AND 1999999 THEN 'boost/effect'
    ELSE 'outro'
  END AS faixa,
  count(*) FILTER (WHERE item_visible) AS vis,
  count(*) FILTER (WHERE item_visible AND shop_tag = 1) AS new_vis,
  count(*) FILTER (WHERE item_visible AND shop_tag = 2) AS hot_vis
FROM system_shop
GROUP BY 1
ORDER BY 1;

SELECT item_id, item_name, shop_tag,
  left(price_cash_list, 48) AS cash,
  left(price_gold_list, 48) AS gold
FROM system_shop
WHERE item_visible AND shop_tag = 1
ORDER BY item_id
LIMIT 40;

SELECT item_id, item_name, shop_tag,
  left(price_cash_list, 48) AS cash,
  left(price_gold_list, 48) AS gold
FROM system_shop
WHERE item_visible AND shop_tag = 2
ORDER BY item_id
LIMIT 40;

-- cash free / gold free samples (price empty or 0)
SELECT count(*) AS new_cash_only
FROM system_shop
WHERE item_visible AND shop_tag = 1
  AND price_cash_list NOT IN ('', '0') AND (price_gold_list = '' OR price_gold_list = '0');

SELECT count(*) AS hot_cash_only
FROM system_shop
WHERE item_visible AND shop_tag = 2
  AND price_cash_list NOT IN ('', '0') AND (price_gold_list = '' OR price_gold_list = '0');
