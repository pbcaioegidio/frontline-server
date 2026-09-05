-- Attendance ("visit") reward claim tracking.
-- `last_visit_check_day` counts the days the player marked as attended, but nothing
-- recorded which of those days already paid out, so replaying
-- PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ re-granted the same box indefinitely.
-- `last_visit_reward_day` is the next day index still owed a reward; a claim is only
-- accepted for exactly that index and it advances by one.
-- The backfill treats every already-attended day as paid, so nobody gets a free
-- re-claim window from this migration. It runs only when the column is created:
-- re-applying must not clobber a player who is mid-campaign with a pending claim.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_schema = 'public' AND table_name = 'player_events'
                     AND column_name = 'last_visit_reward_day') THEN
        ALTER TABLE player_events ADD COLUMN last_visit_reward_day int4 NOT NULL DEFAULT 0;
        UPDATE player_events SET last_visit_reward_day = last_visit_check_day;
    END IF;
END $$;
