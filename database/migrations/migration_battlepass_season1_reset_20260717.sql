-- Battle Pass "Britannia Season 1" launch reset (2026-07-17)
-- The previous BattlepassInfo.json was a placeholder: 17/18 reward good-ids did not
-- exist in system_shop, so BattlepassCardReward -> ShopManager.GetGood returned null and
-- nothing was ever granted, even though UpdateSeasonPass kept incrementing the level counters.
-- The exp curve (RequiredExp) is unchanged, so each account's completed level is identical.
-- To retroactively pay out the now-valid rewards, zero the level counters (keep earned_points
-- and premium ownership); UpdateSeasonPass re-grants every earned level with real items on next
-- login/match. New accounts (levels already 0) are unaffected.

-- 1) De-duplicate rows (owner_id has no PK; a few owners have identical duplicate rows).
DELETE FROM player_battlepass a
USING player_battlepass b
WHERE a.ctid > b.ctid
  AND a.owner_id = b.owner_id;

-- 2) Reset level counters so earned levels re-grant against the real reward table.
UPDATE player_battlepass
SET battlepass_normal_levels = 0,
    battlepass_premium_levels = 0
WHERE battlepass_normal_levels <> 0
   OR battlepass_premium_levels <> 0;
