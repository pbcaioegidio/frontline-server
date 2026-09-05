-- Char-fix dedup + synthetic cleanup (2026-06-29)
-- Root cause: default chars (Basic.xml templates: Viper Red 601005, Natasha 601666,
-- Acid Pool 602002, Chou 602011) are injected into every player's inventory each login
-- with a synthetic stock object_id (itemId | 0x40000000). The grid is INVENTORY-driven
-- (a char only in player_characters, like Wolf, does NOT show), so:
--   * a default char that ALSO has a real player_items row = two inventory rows = duplicate tile
--   * persisting templates to player_characters used the shared synthetic id -> global PK collision
-- The code fix (CreatePlayerCharacter never inserts a stock id as PK) stops the collision.
-- This migration cleans the existing data.
BEGIN;

DROP TABLE IF EXISTS _bak_pi_charfix_20260629;
CREATE TABLE _bak_pi_charfix_20260629 AS SELECT * FROM player_items;
DROP TABLE IF EXISTS _bak_pc_charfix_20260629;
CREATE TABLE _bak_pc_charfix_20260629 AS SELECT * FROM player_characters;

-- G: drop the synthetic-id player_characters rows left by the old buggy persist (account 25's 3).
DELETE FROM player_characters
WHERE object_id >= 1073741824 AND object_id <= 1090519039;

-- F: drop redundant real player_items for the 4 default chars. Templates always provide them,
--    so a real row is a duplicate inventory tile. Equip is keyed by char id (player_equipments),
--    not by this item, so equipped defaults stay equipped via the template.
DELETE FROM player_items
WHERE id IN (601005, 601666, 602002, 602011);

-- report
SELECT 'pc_synthetic_left' AS k, count(*) AS v FROM player_characters
  WHERE object_id >= 1073741824 AND object_id <= 1090519039
UNION ALL
SELECT 'default_items_left', count(*) FROM player_items
  WHERE id IN (601005, 601666, 602002, 602011);

COMMIT;
