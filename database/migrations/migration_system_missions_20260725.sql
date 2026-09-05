-- Migration: mission store list and mission card awards from XML to DB
-- Sources replaced: Data/MissionConfig.xml, Data/Cards/MissionAwards.xml
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-25.
-- MissionPage1/MissionPage2 stay derived: the loader rebuilds the bitmasks from `enabled`.

CREATE TABLE IF NOT EXISTS system_mission_stores (
    id      integer PRIMARY KEY,
    item_id integer NOT NULL DEFAULT 0,
    enabled boolean NOT NULL DEFAULT false
);

-- gold maps to the XML "Point" attribute (lobby point currency)
CREATE TABLE IF NOT EXISTS system_mission_awards (
    id           integer PRIMARY KEY,
    master_medal integer NOT NULL DEFAULT 0,
    exp          integer NOT NULL DEFAULT 0,
    gold         integer NOT NULL DEFAULT 0
);

INSERT INTO system_mission_stores (id, item_id, enabled) VALUES
    (1, 2200001, false),
    (2, 2200002, false),
    (3, 2200003, false),
    (5, 2600005, false),
    (6, 2600006, false),
    (7, 2600007, false),
    (8, 2600008, false),
    (9, 2600009, false),
    (10, 2600010, false),
    (11, 2600011, false),
    (12, 2600012, false),
    (13, 2600013, false),
    (14, 2600014, false),
    (15, 2600015, false),
    (16, 2600016, false),
    (17, 2600017, false)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_mission_awards (id, master_medal, exp, gold) VALUES
    (1, 0, 0, 2000),
    (2, 0, 0, 0),
    (3, 0, 0, 0),
    (5, 1, 500, 0),
    (6, 1, 500, 0),
    (7, 1, 500, 0),
    (8, 1, 700, 0),
    (9, 1, 1000, 0),
    (10, 1, 2000, 0),
    (11, 1, 3000, 0),
    (12, 1, 4000, 0),
    (14, 0, 0, 0),
    (15, 0, 0, 0),
    (16, 0, 0, 0),
    (17, 0, 0, 0)
ON CONFLICT (id) DO NOTHING;
