-- Migration: title system and title awards from XML to DB
-- Sources replaced: Data/Titles/System.xml, Data/Titles/Rewards.xml
-- Idempotent: safe to re-run. Seed reflects the values running on 2026-07-25.
--
-- Note: the old Rewards.xml parser looked for <Title><Rewards><Item>, while the file
-- ships <Award TitleId=..><Counts><Item>, so it always loaded zero awards. The seed
-- below follows the file, so title awards start being granted for the first time.

CREATE TABLE IF NOT EXISTS system_titles (
    id           integer PRIMARY KEY,
    class_id     integer NOT NULL DEFAULT 0,
    ribbon       integer NOT NULL DEFAULT 0,
    ensign       integer NOT NULL DEFAULT 0,
    medal        integer NOT NULL DEFAULT 0,
    master_medal integer NOT NULL DEFAULT 0,
    rank         integer NOT NULL DEFAULT 0,
    slot         integer NOT NULL DEFAULT 0,
    req_title_1  integer NOT NULL DEFAULT 0,
    req_title_2  integer NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS system_title_awards (
    title_id   integer NOT NULL,
    ordinal    integer NOT NULL,
    item_id    integer NOT NULL,
    item_count bigint  NOT NULL DEFAULT 1,
    equip_type integer NOT NULL DEFAULT 0,
    item_name  text    NULL,
    PRIMARY KEY (title_id, ordinal)
);

INSERT INTO system_titles (id, class_id, ribbon, ensign, medal, master_medal, rank, slot, req_title_1, req_title_2) VALUES
    (1, 1, 1, 0, 0, 0, 1, 1, 0, 0),
    (2, 1, 2, 0, 0, 0, 2, 1, 1, 0),
    (3, 1, 3, 0, 0, 0, 3, 1, 2, 0),
    (4, 1, 4, 0, 0, 0, 4, 1, 3, 0),
    (5, 2, 0, 2, 0, 1, 5, 2, 4, 0),
    (6, 4, 0, 2, 0, 2, 5, 2, 4, 0),
    (7, 6, 0, 2, 0, 1, 5, 2, 4, 0),
    (8, 2, 0, 3, 0, 1, 8, 2, 5, 0),
    (9, 2, 0, 0, 6, 2, 12, 2, 8, 0),
    (10, 2, 0, 0, 14, 4, 17, 2, 9, 0),
    (11, 2, 0, 0, 13, 3, 21, 3, 10, 28),
    (12, 2, 0, 0, 21, 6, 26, 3, 11, 0),
    (13, 2, 0, 0, 26, 7, 31, 3, 11, 0),
    (14, 4, 0, 9, 0, 4, 8, 2, 6, 0),
    (15, 4, 0, 8, 0, 2, 12, 2, 14, 0),
    (16, 4, 0, 0, 14, 4, 17, 2, 15, 0),
    (17, 4, 0, 0, 13, 3, 21, 3, 16, 32),
    (18, 4, 0, 0, 21, 6, 26, 2, 17, 0),
    (19, 4, 0, 0, 26, 3, 31, 3, 18, 0),
    (20, 6, 0, 6, 0, 1, 8, 2, 7, 0),
    (21, 6, 0, 0, 20, 2, 12, 2, 20, 0),
    (22, 6, 0, 0, 20, 4, 17, 2, 21, 0),
    (23, 6, 0, 13, 23, 21, 21, 3, 22, 42),
    (24, 6, 0, 0, 28, 6, 26, 3, 23, 0),
    (25, 6, 0, 0, 28, 7, 31, 3, 24, 0),
    (26, 3, 0, 6, 1, 0, 8, 2, 5, 0),
    (27, 3, 0, 0, 6, 0, 12, 2, 26, 0),
    (28, 3, 0, 3, 14, 0, 17, 2, 27, 0),
    (29, 3, 0, 0, 12, 6, 31, 2, 28, 0),
    (30, 5, 0, 9, 0, 1, 8, 2, 6, 0),
    (31, 5, 0, 0, 6, 2, 12, 2, 30, 0),
    (32, 5, 0, 0, 14, 3, 17, 2, 31, 0),
    (33, 5, 0, 0, 12, 4, 26, 2, 32, 0),
    (34, 5, 0, 0, 14, 6, 31, 2, 33, 0),
    (35, 8, 0, 6, 0, 1, 8, 2, 7, 0),
    (36, 8, 0, 0, 8, 2, 12, 2, 35, 0),
    (37, 8, 0, 10, 0, 4, 17, 2, 36, 0),
    (38, 8, 0, 13, 0, 5, 26, 2, 37, 0),
    (39, 8, 0, 0, 15, 6, 31, 2, 38, 0),
    (40, 7, 0, 0, 6, 1, 8, 2, 7, 0),
    (41, 7, 0, 0, 7, 2, 12, 2, 40, 0),
    (42, 7, 0, 20, 0, 3, 17, 2, 41, 0),
    (43, 7, 0, 12, 0, 5, 21, 3, 42, 0),
    (44, 7, 0, 0, 15, 6, 31, 3, 43, 0)
ON CONFLICT (id) DO NOTHING;

INSERT INTO system_title_awards (title_id, ordinal, item_id, item_count, equip_type, item_name) VALUES
    (8, 0, 103014, 10, 1, 'SG 550 S. (10 Qty)'),
    (10, 0, 103013, 10, 1, 'G36C Ext. (10 Qty)'),
    (12, 0, 103036, 10, 1, 'AUG A3 (10 Qty)'),
    (12, 1, 103015, 10, 1, 'AK SOPMOD (10 Qty)'),
    (13, 0, 2700014, 1, 3, 'Beret Title Assault'),
    (14, 0, 105006, 10, 1, 'Dragunov G. (10 Qty)'),
    (16, 0, 105024, 10, 1, 'PSG1 G (10 Qty)'),
    (18, 0, 105005, 10, 1, 'L115A1 (10 Qty)'),
    (19, 0, 2700016, 1, 3, 'Beret Title Sniper'),
    (20, 0, 104007, 10, 1, 'MP5K G. (10 Qty)'),
    (20, 1, 104009, 10, 1, 'Spectre W. (10 Qty)'),
    (22, 0, 104011, 10, 1, 'P90 Ext. (10 Qty)'),
    (24, 0, 104013, 10, 1, 'Kriss S.V (10 Qty)'),
    (25, 0, 2700017, 1, 3, 'Beret Title Submachine'),
    (26, 0, 315001, 10, 1, 'Dual Knife (10 Qty)'),
    (28, 0, 301004, 10, 1, 'Amok Kukri (10 Qty)'),
    (29, 0, 301007, 10, 1, 'Mini Axe (10 Qty)'),
    (30, 0, 213001, 10, 1, 'P99 HAK (10 Qty)'),
    (32, 0, 214001, 10, 1, 'Dual Handgun (10 Qty)'),
    (32, 1, 214002, 10, 1, 'Dual D-Eagle (10 Qty)'),
    (34, 0, 2700015, 1, 3, 'Beret Title Handgun'),
    (35, 0, 106004, 10, 1, '870MCS W. (10 Qty)'),
    (37, 0, 106003, 10, 1, 'SPAS-15 (10 Qty)'),
    (39, 0, 2700018, 1, 3, 'Beret Title Shotgun'),
    (40, 0, 407002, 10, 1, 'C-5 (10 Qty)'),
    (42, 0, 527001, 10, 1, 'WP Smoke (10 Qty)')
ON CONFLICT (title_id, ordinal) DO NOTHING;
