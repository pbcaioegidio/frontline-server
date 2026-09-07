-- FrontLine: corrige dados quebrados dos itens visíveis da loja.
-- Motivo: card "NO PEF(103006)" e crash 0xC0000005 em ShopBottomUIs (WeaponInfo nullptr),
-- além de 8 itens à venda por 1 Gold.
-- REGRA: não apaga linha nenhuma. Backup antes → shop_restore_variant_price.sql desfaz.

BEGIN;

-- 1) Backup de variante/contagem/preço dos itens visíveis (uma vez só).
CREATE TABLE IF NOT EXISTS system_shop_price_backup_launch (
  item_id           integer PRIMARY KEY,
  variant_code_list varchar,
  item_count_list   varchar,
  price_gold_list   varchar,
  price_cash_list   varchar,
  item_visible      boolean,
  backed_up_at      timestamptz
);

INSERT INTO system_shop_price_backup_launch
SELECT s.item_id, s.variant_code_list, s."Item_count_list", s.price_gold_list,
       s.price_cash_list, s.item_visible, now()
FROM system_shop s
WHERE s.item_visible
  AND NOT EXISTS (SELECT 1 FROM system_shop_price_backup_launch b WHERE b.item_id = s.item_id);

-- 2) HK33: variant_code_list = '8' é o único valor de 1 dígito em toda a tabela.
--    O client não acha o PEF desse código e escreve "NO PEF" no card.
--    Código correto é '08' (7 dias) e a contagem tem que ser 604800 segundos.
UPDATE system_shop
SET variant_code_list = '08',
    "Item_count_list" = '604800'
WHERE item_id = 103006
  AND variant_code_list = '8';

-- 3) Preços de 1 Gold. Cada tier passa a seguir o preço por unidade do tier
--    de 100 unidades do próprio item.
UPDATE system_shop SET price_gold_list = '4875,9750,45500'       WHERE item_id = 103003; -- M4A1 Ext.: 50und a 97,5/und
UPDATE system_shop SET price_gold_list = '4313,5175,17250,80500' WHERE item_id = 103036; -- AUG A3: 25 e 30und a 172,5/und
UPDATE system_shop SET price_gold_list = '2250,9000,42000'       WHERE item_id = 104011; -- P90 Ext.: 25und a 90/und
UPDATE system_shop SET price_gold_list = '120,6000,28000'        WHERE item_id = 301007; -- Mini Axe: 2und a 60/und

-- Itens de 1 dia (código 04): 10% do preço de 30 dias, mesma razão usada no Kel-Tec KSG em cash.
UPDATE system_shop SET price_gold_list = '100' WHERE item_id IN (103067, 104003, 202022); -- G36C, K-1 Ext., Colt 45

-- Kel-Tec KSG: o tier de 3 dias estava 0 cash + 1 gold, ou seja, de graça.
UPDATE system_shop
SET price_gold_list = '0,0,0,0',
    price_cash_list = '450,800,1400,4500'
WHERE item_id = 106020;

-- 4) P99: card renderiza em branco (sem ícone). Fica oculto até validar o recurso no client.
UPDATE system_shop SET item_visible = false WHERE item_id = 202006;

COMMIT;

-- Conferência
SELECT item_id, item_name, variant_code_list AS var, "Item_count_list" AS cnt,
       price_gold_list AS gold, price_cash_list AS cash
FROM system_shop
WHERE item_id IN (103003, 103006, 103036, 103067, 104003, 104011, 106020, 202006, 202022, 301007)
ORDER BY item_id;

-- Sobrou algum item visível a 1 Gold ou de graça?
SELECT item_id, item_name, price_gold_list, price_cash_list
FROM system_shop
WHERE item_visible
  AND (','||price_gold_list||',') LIKE '%,1,%'
ORDER BY item_id;
