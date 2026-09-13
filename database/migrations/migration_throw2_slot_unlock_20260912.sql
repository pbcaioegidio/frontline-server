-- Arma Especial 2 (item 1600109 / 1600110) — catálogo limpo.
--
-- O slot foi escondido no SYSTEM_INFO (Throw2PointSlotMaxDays=0). Este build de
-- client não tem 1600109/1600110 no ItemGroup.dat (pula 1600080 → 1600163), então
-- o cadeado só abria Aviso vazio. Aqui só garantimos que os goods ficam
-- invisíveis e removemos o cupom 1700109 criado nas tentativas de unlock.

UPDATE system_shop
SET item_visible = false,
    item_consume = 2,
    "Item_count_list" = '1,1,1,1',
    variant_code_list = '04,06,08,12',
    price_cash_list = '250,0,1200,4000',
    price_gold_list = '0,1,0,0'
WHERE item_id IN (1600109, 1600110);

DELETE FROM system_shop_effects WHERE coupon_id = 1700109;
