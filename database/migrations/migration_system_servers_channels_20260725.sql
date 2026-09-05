-- Migration: server/channel/sync topology from XML to DB
-- Sources replaced: Data/Server/SChannels.xml, Data/Server/Channels.xml, Data/Synchronize.xml
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-25.

CREATE TABLE IF NOT EXISTS system_servers (
    id              integer     PRIMARY KEY,
    state           boolean     NOT NULL DEFAULT false,
    host            text        NOT NULL,
    port            integer     NOT NULL,
    type            text        NOT NULL DEFAULT 'Public',
    is_mobile       boolean     NOT NULL DEFAULT false,
    max_players     integer     NOT NULL DEFAULT 2000,
    channel_players integer     NOT NULL DEFAULT 180
);

CREATE TABLE IF NOT EXISTS system_channels (
    server_id  integer NOT NULL REFERENCES system_servers(id) ON DELETE CASCADE,
    id         integer NOT NULL,
    type       text    NOT NULL DEFAULT 'Public',
    max_rooms  integer NOT NULL DEFAULT 20,
    exp_bonus  integer NOT NULL DEFAULT 100,
    gold_bonus integer NOT NULL DEFAULT 1000,
    cash_bonus integer NOT NULL DEFAULT 10,
    password   text    NULL,
    PRIMARY KEY (server_id, id)
);

CREATE TABLE IF NOT EXISTS system_sync_endpoints (
    remote_port integer PRIMARY KEY,
    host        text    NOT NULL,
    port        integer NOT NULL
);

-- Seed: servers (SChannels.xml)
INSERT INTO system_servers (id, state, host, port, type, is_mobile, max_players, channel_players) VALUES
    (0, false, '127.0.0.1', 39190, 'All',    false, 2000, 180),
    (1, true,  '127.0.0.1', 39191, 'Public', false, 2000, 180),
    (2, true,  '127.0.0.1', 39192, 'Public', false, 2000, 180),
    (3, true,  '127.0.0.1', 39193, 'Clan',   false, 2000, 180)
ON CONFLICT (id) DO NOTHING;

-- Seed: channels (Channels.xml) -- servers 1 and 2 are Public, server 3 is Clan; channel 10 is Unk_14 on all three
INSERT INTO system_channels (server_id, id, type, max_rooms, exp_bonus, gold_bonus, cash_bonus, password)
SELECT s.server_id,
       c.id,
       CASE WHEN c.id = 10 THEN 'Unk_14' ELSE s.base_type END,
       20, 100, 1000, 10, NULL
FROM (VALUES (1, 'Public'), (2, 'Public'), (3, 'Clan')) AS s(server_id, base_type)
CROSS JOIN generate_series(0, 10) AS c(id)
ON CONFLICT (server_id, id) DO NOTHING;

-- Seed: sync endpoints (Synchronize.xml)
INSERT INTO system_sync_endpoints (remote_port, host, port) VALUES
    (39190, '127.0.0.1', 1910),
    (39191, '127.0.0.1', 1911),
    (39192, '127.0.0.1', 1912),
    (39193, '127.0.0.1', 1913),
    (40009, '127.0.0.1', 2729)
ON CONFLICT (remote_port) DO NOTHING;
