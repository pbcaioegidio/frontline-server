-- resumo da loja
\d system_shop
SELECT count(*) AS total_shop FROM system_shop;

SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'system_shop'
ORDER BY ordinal_position;
