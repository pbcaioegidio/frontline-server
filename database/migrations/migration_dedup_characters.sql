-- Fix duplicate character tiles caused by player_characters.object_id not matching
-- the same char's player_items.object_id. The client keys the registered char (2453)
-- by player_characters.object_id and the inventory item (2319) by player_items.object_id;
-- when they differ it cannot dedup -> two tiles per char.
BEGIN;

-- rollback snapshot
DROP TABLE IF EXISTS _bak_player_characters_20260629;
CREATE TABLE _bak_player_characters_20260629 AS SELECT * FROM player_characters;

-- 1. remove literal duplicate character rows (keep the lowest object_id per owner+id)
DELETE FROM player_characters pc
USING player_characters pc2
WHERE pc.owner_id = pc2.owner_id
  AND pc.id = pc2.id
  AND pc.object_id > pc2.object_id;

-- 2. align char.object_id to its inventory item.object_id (two-pass, collision-proof)
--    pass A: park mismatched rows at item_oid + 2e9 (unique, above all existing ids)
UPDATE player_characters pc
SET object_id = pi.object_id + 2000000000
FROM player_items pi
WHERE pi.owner_id = pc.owner_id
  AND pi.id = pc.id
  AND pc.object_id <> pi.object_id;
--    pass B: drop back down to the exact item_oid (all targets now free)
UPDATE player_characters
SET object_id = object_id - 2000000000
WHERE object_id > 2000000000;

-- report
SELECT COUNT(*) AS still_mismatched
FROM player_characters pc
JOIN player_items pi ON pi.owner_id = pc.owner_id AND pi.id = pc.id
WHERE pc.object_id <> pi.object_id;

COMMIT;
