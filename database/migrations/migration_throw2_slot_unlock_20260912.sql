-- Arma Especial 2 / Throwing 2 Point: good do cadeado (BuyExtend).
-- Client tipicamente manda GoodsId=160010901 (item 1600109 + variant 01).
-- Estava: sem nome, item_visible=false, gold=0 → popup "Aviso" vazio + "0 Gold".

UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot',
    item_visible = true,
    price_cash_list = '0',
    price_gold_list = '0',
    "Item_count_list" = COALESCE(NULLIF("Item_count_list", ''), '100')
WHERE item_id = 1600109;

UPDATE system_shop
SET item_name = 'Increase Throwing 2 Slot (Alt)',
    item_visible = true,
    price_cash_list = '0',
    price_gold_list = '0'
WHERE item_id = 1600110;

-- Concede permanente (equip=2 ativo) a quem ainda não tem — libera o cadeado na UI.
INSERT INTO player_items (owner_id, id, name, count, equip)
SELECT a.player_id, 1600109, 'Increase Throwing 2 Slot [Active]', 4212312359, 2
FROM accounts a
WHERE NOT EXISTS (
  SELECT 1 FROM player_items pi
  WHERE pi.owner_id = a.player_id AND pi.id = 1600109
);

SELECT item_id, item_name, item_visible, price_gold_list, price_cash_list, "Item_count_list", item_consume
FROM system_shop WHERE item_id IN (1600109, 1600110);
