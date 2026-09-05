-- El bloque comun de 341 bytes del portal de eventos (cliente 122) lleva TRES textos
-- UTF-16 (+0x13 60B, +0x4F 60B, +0x8B 200B) y el indice de imagen de fondo es el u8 en
-- +0x153, leido con movzx en BoostEvent__BuildDescriptionList @0xBF8BF0. Ese indice es la
-- clave del mapa de Gui/EventPortal/*.i3i que BuildSubEventView @0xC00B10 consulta, con
-- fallback a 1 si la clave no existe. Hasta ahora el servidor mandaba literales fijos y
-- dejaba el segundo texto siempre vacio.
-- Los DEFAULT reproducen exactamente los literales que estaban en
-- PROTOCOL_BASE_EVENT_PORTAL_ACK.BuildBlock, asi que la migracion no cambia lo que ve el
-- cliente hasta que alguien edite el valor en el panel.

ALTER TABLE system_event_boost  ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_boost  ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 0;

ALTER TABLE system_event_login  ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_login  ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 1;

ALTER TABLE system_event_rankup ADD COLUMN IF NOT EXISTS subtitle text     NOT NULL DEFAULT '';
ALTER TABLE system_event_rankup ADD COLUMN IF NOT EXISTS image    smallint NOT NULL DEFAULT 1;
