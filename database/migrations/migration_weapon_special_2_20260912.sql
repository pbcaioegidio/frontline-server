-- Coluna de equipamento para Arma Especial 2 (slot de combate).
-- O cadeado da UI do inventário está escondido (SYSTEM_INFO = 0); esta coluna
-- só guarda o item equipado se o client/servidor usarem o slot no futuro.
ALTER TABLE player_equipments
    ADD COLUMN IF NOT EXISTS weapon_special_2 integer NOT NULL DEFAULT 0;
