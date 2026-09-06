-- FrontLine: curadoria da loja para LANÇAMENTO
-- REGRA: NÃO apaga nenhuma linha. Só altera item_visible (true/false).
-- Backup da visibilidade atual → restaura com shop_restore_visibility.sql

BEGIN;

-- 1) Backup (uma vez). Se já existir, não sobrescreve.
CREATE TABLE IF NOT EXISTS system_shop_visibility_backup_launch AS
SELECT item_id, item_visible, shop_tag, now() AS backed_up_at
FROM system_shop
WHERE false; -- só cria estrutura vazia se não existir

INSERT INTO system_shop_visibility_backup_launch (item_id, item_visible, shop_tag, backed_up_at)
SELECT s.item_id, s.item_visible, s.shop_tag, now()
FROM system_shop s
WHERE NOT EXISTS (
  SELECT 1 FROM system_shop_visibility_backup_launch b WHERE b.item_id = s.item_id
);

-- 2) Esconde tudo que está visível e NÃO está na whitelist
UPDATE system_shop
SET item_visible = false
WHERE item_visible = true
  AND item_id NOT IN (
    -- ===== Arsenal clássico (Gold / base) =====
    -- Rifles
    103001, -- SG 550 Ext.
    103002, -- AK-47 Ext.
    103003, -- M4A1 Ext.
    103004, -- K-2
    103005, -- F2000 Ext.
    103006, -- HK33
    103013, -- G36C Ext.
    103036, -- AUG A3
    103067, -- G36C
    103730, -- G36C Ext. (gold alt)
    103732, -- FAMAS G2 (gold)
    -- SMG
    104001, -- MP5K Ext.
    104002, -- Spectre Ext.
    104003, -- K-1 Ext.
    104004, -- MP7 Ext.
    104006, -- K-1
    104008, -- UMP45 Ext.
    104011, -- P90 Ext.
    104013, -- Kriss S.V
    -- Sniper
    105001, -- Dragunov
    105002, -- PSG1
    105003, -- SSG-69
    105005, -- L115A1
    105029, -- VSK94
    -- Shotgun
    106001, -- 870MCS
    106003, -- SPAS-15
    106005, -- M1887
    106019, -- Jackhammer
    106020, -- Kel-Tec KSG
    -- Pistola
    202001, -- Desert Eagle
    202002, -- MK.23 Ext.
    202006, -- P99
    202007, -- C. Python
    202022, -- Colt 45
    -- Faca
    301002, -- M-9
    301004, -- Amok Kukri
    301007, -- Mini Axe

    -- ===== New (curado, sem Durability/Silver spam) =====
    105684, -- Kar98k Wraith
    105692, -- Kar98k PBNC2026
    105694, -- Barrett Tarantula
    105699, -- Kar98k Tarantula
    105705, -- BORA NewYear2026
    105709, -- Kar98k 17th Anniversary
    106327, -- Zombie Slayer Wraith
    106332, -- Zombie Slayer PBNC
    106336, -- Zombie Slayer Tarantula
    106339, -- Saiga NewYear2026
    110040, -- PKM
    116002, -- RPG7
    136471, -- Honey Badger Astel
    136512, -- T77 Wraith
    136528, -- APC9 PBNC
    136535, -- T77 Tarantula
    139050, -- Pindad SS3 Wraith
    139060, -- Pindad SS3 PBNC
    139065, -- Pindad SS3 Tarantula
    202315, -- Taurus PBNC
    301498, -- FangBlade PBNC
    528047, -- Medical Kit PBNC

    -- ===== Hot (mantém os 11 atuais) =====
    103841, -- AUG-HBAR Comeback
    103894, -- Pindad SS3 Spring2024
    103919, -- AUG-HBAR Astel
    3000159, 3000160, 3000161, -- ACC Astel
    3300037, 3300038, 3300039, -- Astel chars
    3500091, 3500092 -- NameCard Astel
  );

-- 3) Garante que a whitelist está visível (caso algum estivesse oculto)
UPDATE system_shop
SET item_visible = true
WHERE item_id IN (
    103001,103002,103003,103004,103005,103006,103013,103036,103067,103730,103732,
    104001,104002,104003,104004,104006,104008,104011,104013,
    105001,105002,105003,105005,105029,
    106001,106003,106005,106019,106020,
    202001,202002,202006,202007,202022,
    301002,301004,301007,
    105684,105692,105694,105699,105705,105709,
    106327,106332,106336,106339,
    110040,116002,136471,136512,136528,136535,
    139050,139060,139065,202315,301498,528047,
    103841,103894,103919,
    3000159,3000160,3000161,
    3300037,3300038,3300039,
    3500091,3500092
);

COMMIT;

-- Resumo
SELECT
  count(*) FILTER (WHERE item_visible) AS visiveis,
  count(*) FILTER (WHERE NOT item_visible) AS ocultos,
  count(*) FILTER (WHERE item_visible AND shop_tag = 0) AS normal,
  count(*) FILTER (WHERE item_visible AND shop_tag = 1) AS new_tag,
  count(*) FILTER (WHERE item_visible AND shop_tag = 2) AS hot_tag
FROM system_shop;

SELECT shop_tag, item_id, item_name
FROM system_shop
WHERE item_visible
ORDER BY shop_tag, item_id;
