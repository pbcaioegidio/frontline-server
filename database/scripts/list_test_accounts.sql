SELECT player_id, username, nickname, discord_id, gold, cash, rank, access_level, created_at
FROM accounts
ORDER BY player_id;
SELECT discord_id, player_id, username, action, created_at
FROM account_discord_log
ORDER BY id DESC
LIMIT 20;
