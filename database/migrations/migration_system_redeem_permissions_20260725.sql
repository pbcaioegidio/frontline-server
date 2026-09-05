-- Migration: redeem codes and access permissions from XML to DB
-- Sources replaced: Data/RedeemCodes.xml, Data/Access/Permission.xml,
-- Data/Access/PermissionLevel.xml, Data/Access/PermissionRight.xml
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-25.

CREATE TABLE IF NOT EXISTS system_redeem_codes (
    token         text    NOT NULL,
    type          text    NOT NULL,
    ticket_count  bigint  NOT NULL DEFAULT 0,
    player_ration bigint  NOT NULL DEFAULT 1,
    gold_reward   integer NOT NULL DEFAULT 0,
    cash_reward   integer NOT NULL DEFAULT 0,
    tags_reward   integer NOT NULL DEFAULT 0,
    PRIMARY KEY (token, type)
);

CREATE TABLE IF NOT EXISTS system_redeem_code_rewards (
    token   text    NOT NULL,
    type    text    NOT NULL,
    ordinal integer NOT NULL,
    good_id integer NOT NULL,
    PRIMARY KEY (token, type, ordinal),
    FOREIGN KEY (token, type) REFERENCES system_redeem_codes(token, type) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS system_permissions (
    key         integer PRIMARY KEY,
    name        text    NOT NULL,
    description text    NULL
);

CREATE TABLE IF NOT EXISTS system_access_levels (
    key         integer PRIMARY KEY,
    name        text    NOT NULL,
    description text    NULL,
    fake_rank   integer NOT NULL DEFAULT 255
);

CREATE TABLE IF NOT EXISTS system_access_rights (
    level_key      integer NOT NULL REFERENCES system_access_levels(key) ON DELETE CASCADE,
    permission_key integer NOT NULL REFERENCES system_permissions(key) ON DELETE CASCADE,
    PRIMARY KEY (level_key, permission_key)
);

-- Seed: vouchers (RedeemCodes.xml)
INSERT INTO system_redeem_codes (token, type, ticket_count, player_ration, gold_reward, cash_reward, tags_reward) VALUES
    ('HAZE50KCASHDOIS',  'VOUCHER', 150, 1,    0, 50000, 300),
    ('HAZE3PHA73GA8SH',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE9X8A2B4C6D8',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE5Y7C3D9E1F5',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE2Z4X6V8B9N3',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE8Q1W3E5R7T9',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE3P7L9K2J4H6',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE6M8N1B2V3C4',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE4Z7X2C9V8B3',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE1Q5W9E3R7T2',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE7P3L6K9J2H4',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZE9M2N4B6V8C1',  'VOUCHER', 100, 1, 3000,  3000,  10),
    ('HAZEPBIC20192837', 'COUPON',  100, 3,    0,     0,   0),
    ('HAZERIVALYXJAUDSH','COUPON',  100, 3,    0,     0,   0),
    ('HAZEREDSTARGAHSD', 'COUPON',  100, 3,    0,     0,   0),
    ('HAZEPROMOCOMENT',  'COUPON',   30, 1,    0,     0,   0)
ON CONFLICT (token, type) DO NOTHING;

-- Seed: coupon reward goods, in file order
INSERT INTO system_redeem_code_rewards (token, type, ordinal, good_id) VALUES
    ('HAZEPBIC20192837', 'COUPON', 0, 10359803),
    ('HAZEPBIC20192837', 'COUPON', 1, 10359903),
    ('HAZEPBIC20192837', 'COUPON', 2, 10360003),
    ('HAZEPBIC20192837', 'COUPON', 3, 10485803),
    ('HAZEPBIC20192837', 'COUPON', 4, 10486003),
    ('HAZEPBIC20192837', 'COUPON', 5, 10541303),
    ('HAZEPBIC20192837', 'COUPON', 6, 10541403),
    ('HAZEPBIC20192837', 'COUPON', 7, 10619703),
    ('HAZEPBIC20192837', 'COUPON', 8, 20219203),
    ('HAZEPBIC20192837', 'COUPON', 9, 30130803),
    ('HAZERIVALYXJAUDSH','COUPON', 0, 10358003),
    ('HAZERIVALYXJAUDSH','COUPON', 1, 30138003),
    ('HAZERIVALYXJAUDSH','COUPON', 2, 20224703),
    ('HAZERIVALYXJAUDSH','COUPON', 3, 13615503),
    ('HAZERIVALYXJAUDSH','COUPON', 4, 13615303),
    ('HAZERIVALYXJAUDSH','COUPON', 5, 10552303),
    ('HAZERIVALYXJAUDSH','COUPON', 6, 10376903),
    ('HAZERIVALYXJAUDSH','COUPON', 7, 10376803),
    ('HAZEREDSTARGAHSD', 'COUPON', 0, 10358003),
    ('HAZEREDSTARGAHSD', 'COUPON', 1, 10358103),
    ('HAZEREDSTARGAHSD', 'COUPON', 2, 10376703),
    ('HAZEREDSTARGAHSD', 'COUPON', 3, 10482803),
    ('HAZEREDSTARGAHSD', 'COUPON', 4, 10483003),
    ('HAZEREDSTARGAHSD', 'COUPON', 5, 10540103),
    ('HAZEREDSTARGAHSD', 'COUPON', 6, 10540203),
    ('HAZEREDSTARGAHSD', 'COUPON', 7, 10619003),
    ('HAZEREDSTARGAHSD', 'COUPON', 8, 10619103),
    ('HAZEREDSTARGAHSD', 'COUPON', 9, 13614803),
    ('HAZEREDSTARGAHSD', 'COUPON', 10, 13615003),
    ('HAZEREDSTARGAHSD', 'COUPON', 11, 13615203),
    ('HAZEREDSTARGAHSD', 'COUPON', 12, 20224603),
    ('HAZEREDSTARGAHSD', 'COUPON', 13, 30129903),
    ('HAZEREDSTARGAHSD', 'COUPON', 14, 30130003),
    ('HAZEPROMOCOMENT',  'COUPON', 0, 10386304),
    ('HAZEPROMOCOMENT',  'COUPON', 1, 13629304),
    ('HAZEPROMOCOMENT',  'COUPON', 2, 20226904),
    ('HAZEPROMOCOMENT',  'COUPON', 3, 30141504),
    ('HAZEPROMOCOMENT',  'COUPON', 4, 60283204),
    ('HAZEPROMOCOMENT',  'COUPON', 5, 60183104)
ON CONFLICT (token, type, ordinal) DO NOTHING;

-- Seed: permissions (Permission.xml)
INSERT INTO system_permissions (key, name, description) VALUES
    (1, 'observer_enabled',   'Enable observer check box in rooms'),
    (2, 'helpcommand',        'Allow to use :help %page% command'),
    (3, 'hostcommand',        'Allow to use room master commands'),
    (4, 'testcommand',        'Allow to use vip commands for speed-up dev phase'),
    (5, 'moderatorcommand',   'Allow to use all the commands for moderator'),
    (6, 'gamemastercommand',  'Allow to use all the commands for gamemaster'),
    (7, 'developercommand',   'Allow to use all the commands for developer')
ON CONFLICT (key) DO NOTHING;

-- Seed: access levels (PermissionLevel.xml); key matches the AccessLevel enum
INSERT INTO system_access_levels (key, name, description, fake_rank) VALUES
    (0,   'NORMAL',     'Access level for Normal Users',      255),
    (50,  'MODERATOR',  'Access level for MOD (Moderator)',    59),
    (100, 'GAMEMASTER', 'Access level for GM (GameMaster)',    58)
ON CONFLICT (key) DO NOTHING;

-- Seed: rights per level (PermissionRight.xml)
INSERT INTO system_access_rights (level_key, permission_key) VALUES
    (0, 2), (0, 3),
    (50, 1), (50, 2), (50, 3), (50, 4), (50, 5),
    (100, 1), (100, 2), (100, 3), (100, 4), (100, 5), (100, 6), (100, 7)
ON CONFLICT (level_key, permission_key) DO NOTHING;
