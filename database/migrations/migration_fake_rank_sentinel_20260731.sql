-- player_bonus.fake_rank sentinel 55 -> 255.
-- 55 was the "no override, use the real rank" marker, but 55 is a legitimate rank in
-- the 122 client (STBL_IDX_RANK_55 = Lendário Marechal), so nobody could be given that
-- rank as a fake one. 255 is outside every band the client accepts (0..56, 58..60,
-- 97..99, 101..112) and is already what system_access_levels uses for "no override".
-- It runs only while the column default is still 55: after the first pass 55 means a
-- real Lendário Marechal override, and a second blind UPDATE would wipe those.
DO $$
BEGIN
    IF (SELECT column_default FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = 'player_bonus'
          AND column_name = 'fake_rank') = '55' THEN
        UPDATE player_bonus SET fake_rank = 255 WHERE fake_rank = 55;
        ALTER TABLE player_bonus ALTER COLUMN fake_rank SET DEFAULT 255;
    END IF;
END $$;
