-- Migration: event data (login, boost, rank up, visit, quest, xmas) from XML to DB
-- Sources replaced: Data/Events/Login.xml, Boost.xml, Rank.xml, Visit.xml, Quest.xml, Xmas.xml
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-25.
-- Dates keep the client wire format yyMMddHHmm as a plain number.

CREATE TABLE IF NOT EXISTS system_event_login (
    id          integer PRIMARY KEY,
    begin_date  bigint  NOT NULL DEFAULT 0,
    ended_date  bigint  NOT NULL DEFAULT 0,
    name        text    NOT NULL DEFAULT '',
    description text    NOT NULL DEFAULT '',
    period      boolean NOT NULL DEFAULT false,
    priority    boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS system_event_login_rewards (
    event_id integer NOT NULL REFERENCES system_event_login(id) ON DELETE CASCADE,
    ordinal  integer NOT NULL,
    good_id  integer NOT NULL,
    PRIMARY KEY (event_id, ordinal)
);

CREATE TABLE IF NOT EXISTS system_event_boost (
    id          integer PRIMARY KEY,
    begin_date  bigint  NOT NULL DEFAULT 0,
    ended_date  bigint  NOT NULL DEFAULT 0,
    boost_type  text    NOT NULL DEFAULT 'None',
    boost_value integer NOT NULL DEFAULT 0,
    bonus_exp   integer NOT NULL DEFAULT 0,
    bonus_gold  integer NOT NULL DEFAULT 0,
    percent     integer NOT NULL DEFAULT 0,
    name        text    NOT NULL DEFAULT '',
    description text    NOT NULL DEFAULT '',
    period      boolean NOT NULL DEFAULT false,
    priority    boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS system_event_rankup (
    id          integer PRIMARY KEY,
    begin_date  bigint  NOT NULL DEFAULT 0,
    ended_date  bigint  NOT NULL DEFAULT 0,
    name        text    NOT NULL DEFAULT '',
    description text    NOT NULL DEFAULT '',
    period      boolean NOT NULL DEFAULT false,
    priority    boolean NOT NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS system_event_rankup_bonuses (
    event_id    integer NOT NULL REFERENCES system_event_rankup(id) ON DELETE CASCADE,
    rank_id     integer NOT NULL,
    bonus_exp   integer NOT NULL DEFAULT 0,
    bonus_point integer NOT NULL DEFAULT 0,
    percent     integer NOT NULL DEFAULT 0,
    PRIMARY KEY (event_id, rank_id)
);

CREATE TABLE IF NOT EXISTS system_event_visit (
    id         integer PRIMARY KEY,
    begin_date bigint  NOT NULL DEFAULT 0,
    ended_date bigint  NOT NULL DEFAULT 0,
    title      text    NOT NULL DEFAULT '',
    check_days integer NOT NULL DEFAULT 31
);

CREATE TABLE IF NOT EXISTS system_event_visit_boxes (
    event_id   integer NOT NULL REFERENCES system_event_visit(id) ON DELETE CASCADE,
    day        integer NOT NULL,
    good_id_1  integer NOT NULL DEFAULT 0,
    good_id_2  integer NOT NULL DEFAULT 0,
    is_both    boolean NOT NULL DEFAULT false,
    PRIMARY KEY (event_id, day)
);

CREATE TABLE IF NOT EXISTS system_event_quest (
    id         integer PRIMARY KEY,
    begin_date bigint NOT NULL DEFAULT 0,
    ended_date bigint NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS system_event_xmas (
    id         integer PRIMARY KEY,
    begin_date bigint  NOT NULL DEFAULT 0,
    ended_date bigint  NOT NULL DEFAULT 0,
    good_id    integer NOT NULL DEFAULT 0
);

INSERT INTO system_event_login (id, begin_date, ended_date, name, description, period, priority) VALUES
    (1, 2510010000, 2612012359, 'Login Event', 'Login', true, false)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_event_login_rewards (event_id, ordinal, good_id) VALUES
    (1, 0, 10310201)
ON CONFLICT (event_id, ordinal) DO NOTHING;

INSERT INTO system_event_boost (id, begin_date, ended_date, boost_type, boost_value, bonus_exp, bonus_gold, percent, name, description, period, priority) VALUES
    (1, 2509270000, 2610272359, 'Mode', 0, 5000, 10000, 10, 'Point Blank', 'Point Blank', false, false),
    (2, 2509270000, 2610272359, 'PcCafe', 1, 250, 1000, 10, 'Point Blank', 'Point Blank!', false, false),
    (3, 2509270000, 2610272359, 'PcCafe', 2, 500, 5000, 10, 'Point Blank', 'Point Blank', false, false)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_event_rankup (id, begin_date, ended_date, name, description, period, priority) VALUES
    (1, 2510010000, 2612012359, 'Rank Up Event', 'RANK', true, false)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_event_rankup_bonuses (event_id, rank_id, bonus_exp, bonus_point, percent) VALUES
    (1, 0, 150, 500, 10),
    (1, 1, 150, 500, 10),
    (1, 2, 150, 500, 10),
    (1, 3, 150, 500, 10),
    (1, 4, 150, 500, 10),
    (1, 5, 150, 500, 10),
    (1, 6, 150, 500, 10),
    (1, 7, 150, 500, 10),
    (1, 8, 150, 500, 10),
    (1, 9, 150, 500, 10),
    (1, 10, 500, 1000, 10),
    (1, 11, 500, 1000, 10),
    (1, 12, 500, 1000, 10),
    (1, 13, 500, 1000, 10),
    (1, 14, 500, 1000, 10),
    (1, 15, 500, 1000, 10),
    (1, 16, 500, 1000, 10),
    (1, 17, 500, 1000, 10),
    (1, 18, 500, 1000, 10),
    (1, 19, 500, 1000, 10),
    (1, 20, 500, 1000, 10),
    (1, 21, 10000, 15000, 10),
    (1, 22, 10000, 15000, 10),
    (1, 23, 10000, 15000, 10),
    (1, 24, 10000, 15000, 10),
    (1, 25, 10000, 15000, 10),
    (1, 26, 10000, 15000, 10),
    (1, 27, 10000, 15000, 10),
    (1, 28, 10000, 25000, 10),
    (1, 29, 10000, 25000, 10),
    (1, 30, 10000, 25000, 10),
    (1, 31, 10000, 25000, 10),
    (1, 32, 10000, 25000, 10),
    (1, 33, 50000, 75000, 10),
    (1, 34, 50000, 75000, 10),
    (1, 35, 50000, 75000, 10),
    (1, 36, 50000, 75000, 10),
    (1, 37, 50000, 75000, 10),
    (1, 38, 50000, 75000, 10),
    (1, 39, 50000, 75000, 10),
    (1, 40, 50000, 75000, 10),
    (1, 41, 50000, 75000, 10),
    (1, 42, 50000, 75000, 10),
    (1, 43, 50000, 75000, 10),
    (1, 44, 50000, 75000, 10),
    (1, 45, 50000, 75000, 10),
    (1, 46, 98000, 250000, 10),
    (1, 47, 98000, 250000, 10),
    (1, 48, 98000, 250000, 10),
    (1, 49, 98000, 250000, 10),
    (1, 50, 98000, 250000, 10),
    (1, 51, 98000, 250000, 10),
    (1, 52, 98000, 250000, 10),
    (1, 53, 98000, 250000, 10),
    (1, 54, 98000, 250000, 10),
    (1, 55, 98000, 250000, 10),
    (1, 56, 98000, 250000, 10)
ON CONFLICT (event_id, rank_id) DO NOTHING;

INSERT INTO system_event_visit (id, begin_date, ended_date, title, check_days) VALUES
    (1, 2607010000, 2612312359, 'ATTENDANCE TEST 31 DAYS', 31),
    (2, 2701010000, 2712312359, 'ATTENDANCE CHECK P2', 4),
    (3, 2701010000, 2712312359, 'ATTENDANCE CHECK P3', 4),
    (4, 2701010000, 2712312359, 'ATTENDANCE CHECK P4', 4),
    (5, 2701010000, 2712312359, 'POINT BLANK LATINO Visit Event!', 4),
    (6, 2701010000, 2712312359, 'POINT BLANK LATINO Visit Event!', 4),
    (7, 2701010000, 2712312359, 'POINT BLANK LATINO Visit Event!', 4)
ON CONFLICT (id) DO NOTHING;

-- Presenca: so goods item_visible=true (catalogo packed completo).
-- Goods limitados (Alien/Halloween hidden) crasham o client (Please Wait / 0xC0000005).
INSERT INTO system_event_visit_boxes (event_id, day, good_id_1, good_id_2, is_both) VALUES
    (1, 1, 10306704, 10400304, false),
    (1, 2, 10569404, 10602004, false),
    (1, 3, 10568404, 10632704, false),
    (1, 4, 10384106, 10389406, false),
    (1, 5, 10569904, 10633604, false),
    (1, 6, 10569204, 10633204, false),
    (1, 7, 20202204, 20231504, false),
    (1, 8, 10384108, 10389408, true),
    (1, 9, 10569406, 10602006, false),
    (1, 10, 10568406, 10632706, false),
    (1, 11, 10569906, 10633606, false),
    (1, 12, 10569206, 10633206, false),
    (1, 13, 10570506, 10633906, false),
    (1, 14, 10570906, 20231506, false),
    (1, 15, 10384108, 10389408, false),
    (1, 16, 10569408, 10602008, true),
    (1, 17, 10568408, 10632708, false),
    (1, 18, 10569908, 10633608, false),
    (1, 19, 10569208, 10633208, false),
    (1, 20, 10570508, 10633908, false),
    (1, 21, 10570908, 20231508, false),
    (1, 22, 10384112, 10389412, false),
    (1, 23, 10569412, 10602012, false),
    (1, 24, 10568412, 10632712, true),
    (1, 25, 10569912, 10633612, false),
    (1, 26, 10569212, 10633212, false),
    (1, 27, 10570512, 10633912, false),
    (1, 28, 10570912, 20231512, false),
    (1, 29, 10301312, 10373212, false),
    (1, 30, 10391912, 10601912, false),
    (1, 31, 10384112, 10569412, true),
    (2, 1, 10306704, 10400304, false),
    (2, 2, 10569404, 10602004, false),
    (2, 3, 10384106, 10389406, false),
    (2, 4, 10568408, 10632708, false),
    (3, 1, 10306704, 10400304, false),
    (3, 2, 10569404, 10602004, false),
    (3, 3, 10384106, 10389406, false),
    (3, 4, 10568408, 10632708, false),
    (4, 1, 10306704, 10400304, false),
    (4, 2, 10569404, 10602004, false),
    (4, 3, 10384106, 10389406, false),
    (4, 4, 10568408, 10632708, false),
    (5, 1, 10306704, 10400304, false),
    (5, 2, 10569404, 10602004, false),
    (5, 3, 10384106, 10389406, false),
    (5, 4, 10568408, 10632708, false),
    (6, 1, 10306704, 10400304, false),
    (6, 2, 10569404, 10602004, false),
    (6, 3, 10384106, 10389406, false),
    (6, 4, 10568408, 10632708, false),
    (7, 1, 10306704, 10400304, false),
    (7, 2, 10569404, 10602004, false),
    (7, 3, 10384106, 10389406, false),
    (7, 4, 10568408, 10632708, false)
ON CONFLICT (event_id, day) DO NOTHING;

INSERT INTO system_event_quest (id, begin_date, ended_date) VALUES
    (1, 2509281500, 2612012359)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_event_xmas (id, begin_date, ended_date, good_id) VALUES
    (1, 2512250000, 2601052359, 10310201)
ON CONFLICT (id) DO NOTHING;

