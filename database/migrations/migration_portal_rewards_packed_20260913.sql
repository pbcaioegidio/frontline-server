-- Recompensas de portal/login: só goods com item_visible=true (catálogo packed).
-- Antes usava skins invisíveis (10310204 etc.) → skip no PortalManager / risco de crash.

UPDATE system_event_login_rewards
SET good_id = 10568404
WHERE event_id = 1 AND good_id IN (10310201, 10310204);

-- Garante colunas do portal (idempotente; já existia migration 20260728).
ALTER TABLE system_event_boost  ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_boost  ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 0;
ALTER TABLE system_event_login  ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_login  ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 1;
ALTER TABLE system_event_rankup ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_rankup ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 1;
