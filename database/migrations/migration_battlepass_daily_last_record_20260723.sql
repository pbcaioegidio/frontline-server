-- Battle-pass daily-point cap support.
-- `points` (already present) holds the season points earned on the current day;
-- `last_record` is the yyyyMMdd of the day those points were last touched, so the
-- server-owned MaxDailyPoints cap resets correctly across restarts (not just
-- within a running session).
ALTER TABLE player_battlepass ADD COLUMN IF NOT EXISTS last_record int4 NOT NULL DEFAULT 0;
