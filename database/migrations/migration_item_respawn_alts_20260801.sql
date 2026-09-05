INSERT INTO system_coupon_effects (item_id, effect_flag) VALUES
    (1600163, 'Respawn30'),
    (1600164, 'Respawn20'),
    (1600182, 'Respawn100')
ON CONFLICT (item_id) DO UPDATE SET effect_flag = EXCLUDED.effect_flag;
