INSERT INTO system_coupon_effects (item_id, effect_flag) VALUES
    (1600165, 'QuickChangeWeapon'),
    (1600166, 'QuickChangeReload')
ON CONFLICT (item_id) DO UPDATE SET effect_flag = EXCLUDED.effect_flag;
