-- FrontLine: pacote VIP Prata (Silver) / VIP Ouro (Gold) — lançamento
-- No jogo o enum continua Silver/Gold; no Discord os cargos são VIP Prata / VIP Ouro.

BEGIN;

DELETE FROM system_pc_cafe_rewards;
DELETE FROM system_pc_cafes;

-- Bônus passivos
INSERT INTO system_pc_cafes (cafe_type, exp_up, point_up) VALUES
('Silver', 300, 1500),   -- VIP Prata
('Gold',   600, 6000);   -- VIP Ouro

-- VIP Prata (Silver): arsenal sólido + boosts
INSERT INTO system_pc_cafe_rewards (cafe_type, seq, item_id, name) VALUES
('Silver', 0,  103218, 'SCAR-L FC PBNC 2015 US'),
('Silver', 1,  104214, 'OA-93 PBNC 2015 US'),
('Silver', 2,  106057, 'SPAS-15 MSC PBNC 2015 US'),
('Silver', 3,  105119, 'XM-2010 PBNC 2015 US'),
('Silver', 4,  202022, 'Colt 45'),
('Silver', 5,  301012, 'Mini Axe'),
('Silver', 6,  1600180, 'Skill Time Boost X2'),
('Silver', 7,  1600173, 'Bonus Control'),
('Silver', 8,  1600174, 'Bonus Accuracy'),
('Silver', 9,  1600176, 'Bonus Damage'),
('Silver', 10, 1600177, 'Bonus Defense');

-- VIP Ouro (Gold): arsenal premium (sem itens “meme”)
INSERT INTO system_pc_cafe_rewards (cafe_type, seq, item_id, name) VALUES
('Gold', 0,  103722, 'Pindad SS2 V5 SciFI Basic'),
('Gold', 1,  103643, 'M4 Azure Dragon'),
('Gold', 2,  104990, 'Pink Dragon SMG'),
('Gold', 3,  136065, 'OA-93 SciFI Basic'),
('Gold', 4,  105492, 'PGM Hecate2 SciFI Basic'),
('Gold', 5,  105447, 'AWP Vermilion Bird'),
('Gold', 6,  106232, 'Saiga Black Tortoise'),
('Gold', 7,  106115, 'SF Shotgun'),
('Gold', 8,  202228, 'C. Python SciFi Basic'),
('Gold', 9,  301356, 'Karambit SciFi'),
('Gold', 10, 2700013, 'PBTN Beret'),
('Gold', 11, 701106, 'Super Headgear'),
('Gold', 12, 528005, 'Medical Kit'),
('Gold', 13, 1600180, 'Skill Time Boost X2'),
('Gold', 14, 1600173, 'Bonus Control'),
('Gold', 15, 1600174, 'Bonus Accuracy'),
('Gold', 16, 1600175, 'Bonus Penetration'),
('Gold', 17, 1600176, 'Bonus Damage'),
('Gold', 18, 1600177, 'Bonus Defense'),
('Gold', 19, 1600178, 'Bonus Reduction');

COMMIT;

SELECT cafe_type, exp_up, point_up FROM system_pc_cafes ORDER BY cafe_type;
SELECT cafe_type, count(*) AS itens FROM system_pc_cafe_rewards GROUP BY cafe_type ORDER BY cafe_type;
