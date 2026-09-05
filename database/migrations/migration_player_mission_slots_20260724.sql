-- Consolidate mission card-set ownership+state into one per-slot table.
-- Replaces the accounts.mission_id1..3 (ownership) + player_missions.card1..4 /
-- mission1..4_raw (card+progress) split. Slot range 0..3 (client MAX_CARDSET_PER_USER=4).
-- Idempotent: re-running is a no-op. Old columns are left populated; dropping them
-- is a separate later migration, only after live validation.

CREATE TABLE IF NOT EXISTS "public"."player_mission_slots" (
  "owner_id"     int8        NOT NULL,
  "slot"         int2        NOT NULL CHECK ("slot" BETWEEN 0 AND 3),
  "card_set_id"  int4        NOT NULL,
  "current_card" int4        NOT NULL DEFAULT 0,
  "progress"     bytea       NOT NULL DEFAULT decode(repeat('00', 40), 'hex'),
  "acquired_at"  timestamptz NOT NULL DEFAULT now(),
  PRIMARY KEY ("owner_id", "slot")
);
ALTER TABLE "public"."player_mission_slots" OWNER TO "postgres";

INSERT INTO "public"."player_mission_slots" (owner_id, slot, card_set_id, current_card, progress)
SELECT a.player_id, s.slot, s.card_set_id,
       COALESCE(s.current_card, 0),
       COALESCE(NULLIF(s.progress, ''::bytea), decode(repeat('00', 40), 'hex'))
FROM accounts a
LEFT JOIN player_missions pm ON pm.owner_id = a.player_id
CROSS JOIN LATERAL (VALUES
    (0::int2, a.mission_id1, pm.card1, pm.mission1_raw),
    (1::int2, a.mission_id2, pm.card2, pm.mission2_raw),
    (2::int2, a.mission_id3, pm.card3, pm.mission3_raw)
) AS s(slot, card_set_id, current_card, progress)
WHERE s.card_set_id IS NOT NULL AND s.card_set_id > 0
ON CONFLICT (owner_id, slot) DO NOTHING;
