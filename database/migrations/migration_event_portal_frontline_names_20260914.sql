-- FrontLine: títulos do Portal de Evento (boost) sem marca Point Blank
BEGIN;
UPDATE system_event_boost SET name = 'FRONTLINE', description = 'FRONTLINE' WHERE id = 1;
UPDATE system_event_boost SET name = 'FRONTLINE', description = 'VIP Prata (PC Café)' WHERE id = 2;
UPDATE system_event_boost SET name = 'FRONTLINE', description = 'VIP Ouro (PC Café)' WHERE id = 3;
UPDATE system_event_boost
SET name = replace(name, 'Point Blank', 'FRONTLINE'),
    description = replace(description, 'Point Blank', 'FRONTLINE')
WHERE name ILIKE '%Point Blank%' OR description ILIKE '%Point Blank%';
COMMIT;
