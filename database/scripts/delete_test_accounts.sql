-- Apaga só as 2 contas de teste do cadastro Discord (player_id 37 e 38).
BEGIN;

DELETE FROM account_discord_log WHERE player_id IN (37, 38);
DELETE FROM discord_pending_bans WHERE player_id IN (37, 38);

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'player_items','player_characters','player_equipments','player_missions',
    'player_friends','player_events','player_bonus','player_titles',
    'player_stat','player_configs','account_devices','ban_identifiers',
    'login_history','security_events'
  ]
  LOOP
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema='public' AND table_name=t) THEN
      BEGIN
        EXECUTE format('DELETE FROM %I WHERE player_id = ANY(ARRAY[37,38]::bigint[])', t);
      EXCEPTION WHEN undefined_column THEN
        BEGIN
          EXECUTE format('DELETE FROM %I WHERE owner_id = ANY(ARRAY[37,38]::bigint[])', t);
        EXCEPTION WHEN undefined_column THEN NULL;
        END;
      END;
    END IF;
  END LOOP;
END
$$;

DELETE FROM accounts WHERE player_id IN (37, 38);

COMMIT;

SELECT count(*) AS contas_restantes FROM accounts;
SELECT player_id, username FROM accounts ORDER BY player_id;
