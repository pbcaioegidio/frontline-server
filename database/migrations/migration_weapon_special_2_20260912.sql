-- Arma Especial 2: coluna de equipamento + unlock via SYSTEM_INFO Throw2PointSlotMaxDays.
ALTER TABLE player_equipments
    ADD COLUMN IF NOT EXISTS weapon_special_2 integer NOT NULL DEFAULT 0;
