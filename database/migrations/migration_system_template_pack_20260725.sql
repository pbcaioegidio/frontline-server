-- Template pack: itens iniciais, itens de premiacao e bonus de PC Cafe

CREATE TABLE IF NOT EXISTS system_template_items (
    kind       VARCHAR(16)  NOT NULL,
    seq        INTEGER      NOT NULL,
    item_id    INTEGER      NOT NULL,
    name       VARCHAR(128) NOT NULL DEFAULT '',
    item_count BIGINT       NOT NULL DEFAULT 1,
    PRIMARY KEY (kind, seq)
);

CREATE TABLE IF NOT EXISTS system_pc_cafes (
    cafe_type VARCHAR(16) PRIMARY KEY,
    exp_up    INTEGER NOT NULL DEFAULT 0,
    point_up  INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS system_pc_cafe_rewards (
    cafe_type VARCHAR(16)  NOT NULL,
    seq       INTEGER      NOT NULL,
    item_id   INTEGER      NOT NULL,
    name      VARCHAR(128) NOT NULL DEFAULT '',
    PRIMARY KEY (cafe_type, seq)
);

DELETE FROM system_pc_cafe_rewards;
DELETE FROM system_pc_cafes;
DELETE FROM system_template_items;

INSERT INTO system_template_items (kind, seq, item_id, name, item_count) VALUES
('basic', 0, 103004, 'K-2', 1),
('basic', 1, 104006, 'K-1', 1),
('basic', 2, 105003, 'SSG-69', 1),
('basic', 3, 106001, '870-MCS', 1),
('basic', 4, 202022, 'Colt 45', 1),
('basic', 5, 301012, 'Mini Axe', 1),
('basic', 6, 407056, 'K-400', 1),
('basic', 7, 508002, 'Special', 1),
('basic', 8, 601005, 'Viper Red', 1),
('basic', 9, 601666, 'Natasha', 1),
('basic', 10, 602002, 'Acid Pool', 1),
('basic', 11, 602011, 'Chou', 1),
('basic', 12, 1500511, 'Dino Raptor', 1),
('basic', 13, 1500512, 'Dino Sting', 1),
('basic', 14, 1500513, 'Dino Acid', 1),
('basic', 15, 1600205, 'Name Card Border', 1),
('award', 0, 103324, 'AUG A3 Beyond (3 days)', 259200),
('award', 1, 104357, 'Kriss S.V Beyond (3 days)', 259200),
('award', 2, 104359, 'OA-93 Beyond (3 days)', 259200),
('award', 3, 105199, 'Cheytac M200 Beyond (3 days)', 259200),
('award', 4, 105200, 'AS-50 Beyond (3 days)', 259200),
('award', 5, 106090, 'M1887 Beyond (3 days)', 259200),
('award', 6, 315016, 'Dual Bone Knife Beyond (3 days)', 259200),
('award', 7, 800357, 'Mask Beyond (3 days)', 259200);

INSERT INTO system_pc_cafes (cafe_type, exp_up, point_up) VALUES
('Silver', 250, 1000),
('Gold', 500, 5000);

INSERT INTO system_pc_cafe_rewards (cafe_type, seq, item_id, name) VALUES
('Silver', 0, 103218, 'SCAR-L FC PBNC 2015 US'),
('Silver', 1, 104214, 'OA-93 PBNC 2015 US'),
('Silver', 2, 106057, 'SPAS-15 MSC PBNC 2015 US'),
('Silver', 3, 105119, 'XM-2010 PBNC 2015 US'),
('Silver', 4, 1600180, 'Skill Time Boos X2'),
('Silver', 5, 1600173, 'Bonus Control Item'),
('Silver', 6, 1600174, 'Bonus Accuracy Item'),
('Silver', 7, 1600175, 'Bonus Penetration Item'),
('Silver', 8, 1600176, 'Bonus Damage Item'),
('Silver', 9, 1600177, 'Bonus Defense Item'),
('Silver', 10, 1600178, 'Bonus Reduction Item'),
('Gold', 0, 103722, 'Pindad SS2 V5 SciFI Basic'),
('Gold', 1, 105492, 'PGM Hecate2 SciFI Basic'),
('Gold', 2, 136065, 'OA-93 SciFI Basic'),
('Gold', 3, 202228, 'C. Python SciFi Basic'),
('Gold', 4, 301356, 'Karambit SciFi'),
('Gold', 5, 301098, 'Hair Dryer'),
('Gold', 6, 323010, 'Zombie Tooth Knuckle'),
('Gold', 7, 2700013, 'PBTN Beret'),
('Gold', 8, 701106, 'Super Headgear'),
('Gold', 9, 528005, 'Medical Kit Opor Ayam'),
('Gold', 10, 103677, 'Penguin Gun'),
('Gold', 11, 105447, 'AWP Vermilion Bird'),
('Gold', 12, 106115, 'SF Shotgun'),
('Gold', 13, 106232, 'Saiga Black Tortoise'),
('Gold', 14, 136010, 'P90 Ext. Bike'),
('Gold', 15, 103643, 'M4 Azure Dragon'),
('Gold', 16, 104990, 'Pink Dragon SMG');
