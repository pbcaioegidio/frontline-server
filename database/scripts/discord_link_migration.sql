-- FrontLine: vínculo Discord ↔ conta (cadastro / VIP / ban sync)
-- NÃO apaga dados. Só ADD COLUMN / tabelas novas.

BEGIN;

-- discord_id: snowflake Discord (texto). NULL = não vinculado.
ALTER TABLE accounts
  ADD COLUMN IF NOT EXISTS discord_id varchar(32);

CREATE UNIQUE INDEX IF NOT EXISTS accounts_discord_id_uidx
  ON accounts (discord_id)
  WHERE discord_id IS NOT NULL AND discord_id <> '';

-- Auditoria de ações do bot
CREATE TABLE IF NOT EXISTS account_discord_log (
  id bigserial PRIMARY KEY,
  discord_id varchar(32) NOT NULL,
  player_id bigint,
  username varchar(16),
  action varchar(32) NOT NULL,
  detail text,
  created_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS account_discord_log_discord_idx
  ON account_discord_log (discord_id, created_at DESC);

-- Fila: hardban no jogo → bot bane no Discord
CREATE TABLE IF NOT EXISTS discord_pending_bans (
  id bigserial PRIMARY KEY,
  discord_id varchar(32) NOT NULL,
  player_id bigint,
  username varchar(16),
  reason text,
  created_at timestamptz NOT NULL DEFAULT now(),
  processed_at timestamptz
);

CREATE INDEX IF NOT EXISTS discord_pending_bans_pending_idx
  ON discord_pending_bans (id)
  WHERE processed_at IS NULL;

-- Quando access_level vira banido (-1), enfileira ban Discord se vinculado
CREATE OR REPLACE FUNCTION accounts_queue_discord_ban()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
  IF NEW.access_level = -1
     AND (OLD.access_level IS DISTINCT FROM -1)
     AND NEW.discord_id IS NOT NULL
     AND NEW.discord_id <> '' THEN
    INSERT INTO discord_pending_bans (discord_id, player_id, username, reason)
    VALUES (
      NEW.discord_id,
      NEW.player_id,
      NEW.username,
      'Hardban no jogo (access_level=-1)'
    );
  END IF;
  RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_accounts_queue_discord_ban ON accounts;
CREATE TRIGGER trg_accounts_queue_discord_ban
  AFTER UPDATE OF access_level ON accounts
  FOR EACH ROW
  EXECUTE FUNCTION accounts_queue_discord_ban();

COMMIT;

SELECT column_name, data_type, character_maximum_length
FROM information_schema.columns
WHERE table_name = 'accounts' AND column_name = 'discord_id';
