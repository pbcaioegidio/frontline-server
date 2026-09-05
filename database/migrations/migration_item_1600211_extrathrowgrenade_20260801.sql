INSERT INTO system_coupon_effects (item_id, effect_flag) VALUES
    (1600211, 'ExtraThrowGrenade')
ON CONFLICT (item_id) DO UPDATE SET effect_flag = EXCLUDED.effect_flag;
