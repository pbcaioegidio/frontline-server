-- FrontLine: limpa dados de jogador, mantém catálogos system_*
BEGIN;

UPDATE events SET created_by = NULL WHERE created_by IS NOT NULL;

TRUNCATE TABLE
  auth_tokens,
  anticheat_detections,
  player_rewards
RESTART IDENTITY CASCADE;

DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'live_sessions') THEN
    EXECUTE 'TRUNCATE TABLE live_sessions RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'capture_requests') THEN
    EXECUTE 'TRUNCATE TABLE capture_requests RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'shop_audit') THEN
    EXECUTE 'TRUNCATE TABLE shop_audit RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'account_devices') THEN
    EXECUTE 'TRUNCATE TABLE account_devices RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'account_links') THEN
    EXECUTE 'TRUNCATE TABLE account_links RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'login_audit') THEN
    EXECUTE 'TRUNCATE TABLE login_audit RESTART IDENTITY CASCADE';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'security_events') THEN
    EXECUTE 'TRUNCATE TABLE security_events RESTART IDENTITY CASCADE';
  END IF;
END $$;

TRUNCATE TABLE
  player_items,
  player_characters,
  player_equipments,
  player_bonus,
  player_configs,
  player_quickstarts,
  player_titles,
  player_vip,
  player_missions,
  player_mission_slots,
  player_events,
  player_battlepass,
  player_competitive,
  player_friends,
  player_messages,
  player_reports,
  player_stat_basics,
  player_stat_seasons,
  player_stat_dailies,
  player_stat_weapons,
  player_stat_clans,
  player_stat_acemodes,
  player_stat_battlecups,
  player_stat_battleroyales,
  player_stat_mercenaries
RESTART IDENTITY;

TRUNCATE TABLE
  base_nick_history,
  base_redeem_history,
  base_report_history,
  base_ban_history,
  base_auto_ban,
  base_ban_hwid
RESTART IDENTITY;

DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'banned_hwid') THEN
    EXECUTE 'TRUNCATE TABLE banned_hwid RESTART IDENTITY';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ban_identifiers') THEN
    EXECUTE 'TRUNCATE TABLE ban_identifiers RESTART IDENTITY';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'macro_suspects') THEN
    EXECUTE 'TRUNCATE TABLE macro_suspects RESTART IDENTITY';
  END IF;
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'launcher_access') THEN
    EXECUTE 'TRUNCATE TABLE launcher_access RESTART IDENTITY';
  END IF;
END $$;

TRUNCATE TABLE
  system_clan_invites,
  system_clan,
  clan_matches
RESTART IDENTITY CASCADE;

DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'web_session') THEN
    EXECUTE 'TRUNCATE TABLE web_session, web_history_topup, web_log_registros RESTART IDENTITY CASCADE';
  END IF;
END $$;

TRUNCATE TABLE accounts RESTART IDENTITY CASCADE;

COMMIT;

SELECT 'accounts' AS t, count(*) FROM accounts
UNION ALL SELECT 'player_items', count(*) FROM player_items
UNION ALL SELECT 'player_characters', count(*) FROM player_characters
UNION ALL SELECT 'system_clan', count(*) FROM system_clan;
