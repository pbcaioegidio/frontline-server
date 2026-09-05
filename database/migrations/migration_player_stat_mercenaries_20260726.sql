-- Registro de mercenario de guerra de clan exibido no MyInfo (opcode 977, bloco final de 32 bytes)

CREATE TABLE IF NOT EXISTS player_stat_mercenaries (
    owner_id        bigint PRIMARY KEY,
    matches         integer NOT NULL DEFAULT 0,
    match_wins      integer NOT NULL DEFAULT 0,
    match_loses     integer NOT NULL DEFAULT 0,
    drops_count     integer NOT NULL DEFAULT 0,
    kills_count     integer NOT NULL DEFAULT 0,
    deaths_count    integer NOT NULL DEFAULT 0,
    headshots_count integer NOT NULL DEFAULT 0,
    assists_count   integer NOT NULL DEFAULT 0
);
