-- Migration: replace invalid 119/RU default item IDs with valid 121 BR IDs.
-- Safe to run multiple times (idempotent). Only touches the known 119 sentinel
-- values, leaving players' real purchased items untouched.
-- Cause: 119-era default IDs do not exist in the 121 client item-DB (Shop.dat),
-- so shop/character-tab render (CreateEquip / getWeaponInfo) crashes.

BEGIN;

-- 1) Fix equipped weapons / characters that still hold 119 sentinel IDs.
UPDATE player_equipments SET weapon_secondary = 202022 WHERE weapon_secondary = 202003;
UPDATE player_equipments SET weapon_melee     = 301012 WHERE weapon_melee     = 301001;
UPDATE player_equipments SET weapon_explosive = 407056 WHERE weapon_explosive = 407001;
UPDATE player_equipments SET weapon_special   = 508002 WHERE weapon_special   = 508001;
UPDATE player_equipments SET chara_red_side   = 601666 WHERE chara_red_side   IN (601001, 632656);
UPDATE player_equipments SET chara_blue_side  = 602002 WHERE chara_blue_side  = 664657;

-- 2) Empty out the placeholder cosmetic parts (119 sentinels) -> 0 (no part).
UPDATE player_equipments SET part_head    = 0 WHERE part_head    = 1000700000;
UPDATE player_equipments SET part_face    = 0 WHERE part_face    = 1000800000;
UPDATE player_equipments SET part_jacket  = 0 WHERE part_jacket  = 1000900000;
UPDATE player_equipments SET part_pocket  = 0 WHERE part_pocket  = 1001000000;
UPDATE player_equipments SET part_glove   = 0 WHERE part_glove   = 1001100000;
UPDATE player_equipments SET part_belt    = 0 WHERE part_belt    = 1001200000;
UPDATE player_equipments SET part_holster = 0 WHERE part_holster = 1001300000;
UPDATE player_equipments SET part_skin    = 0 WHERE part_skin    = 1001400000;

-- 3) Remove invalid 119 base items from inventories so they cannot be equipped
--    from the inventory UI (which would crash the 121 client on render).
DELETE FROM player_items WHERE id IN (
  110009,      -- MK-46
  202003,      -- K-5
  301001,      -- M-7
  323001,      -- Barefist
  407001,      -- K-400
  508001,      -- Smoke
  601001,      -- Red Bulls
  602002,      -- Acid Pool
  1000700000,  -- Part Head
  1000800000,  -- Part Face
  1000900000,  -- Part Jacket
  1001000000,  -- Part Pocket
  1001100000,  -- Part Glove
  1001200000,  -- Part Belt
  1001300000,  -- Part Holster
  1001400000   -- Part Skin
);

COMMIT;
