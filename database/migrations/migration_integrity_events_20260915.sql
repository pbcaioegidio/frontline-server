-- Integridade FL Guard: launcher reporta FileCheck → staff/IA veem o arquivo.
-- #28 plano-ia-salas-controle.md

CREATE TABLE IF NOT EXISTS public.integrity_events (
    id              bigserial PRIMARY KEY,
    ts              timestamp(6) without time zone NOT NULL DEFAULT now(),
    player_id       bigint NOT NULL DEFAULT 0,
    username        varchar(64) NOT NULL DEFAULT '',
    ip              varchar(64) NOT NULL DEFAULT '',
    ok              boolean NOT NULL DEFAULT false,
    restored        boolean,
    invalid_files   jsonb NOT NULL DEFAULT '[]'::jsonb,
    extras_removed  jsonb NOT NULL DEFAULT '[]'::jsonb,
    launcher_ver    varchar(32) NOT NULL DEFAULT '',
    message         varchar(512) NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS ix_integrity_events_player_ts
    ON public.integrity_events (player_id, ts DESC);

CREATE INDEX IF NOT EXISTS ix_integrity_events_ts
    ON public.integrity_events (ts DESC);

CREATE INDEX IF NOT EXISTS ix_integrity_events_username_ts
    ON public.integrity_events (lower(username), ts DESC);
