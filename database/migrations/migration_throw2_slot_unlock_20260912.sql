-- Arma Especial 2 (item 1600109 / 1600110) — rollback.
--
-- O cadeado não é resolvível pelo servidor neste build de client: o
-- ItemGroup.dat do client lista os itens de efeito 16000xx e pula de 1600080
-- para 1600163, sem entrada para 1600109/1600110. Sem isso o client não
-- resolve o good do BuyExtend, o Aviso abre vazio (0 dias / 0 Gold) e nenhum
-- EXTEND_REQ é enviado. O ExtraGrenade (1600035) funciona porque está no
-- ItemGroup.dat e tem os goods 170003501..04 gravados no Shop.dat local.
--
-- Aqui só devolvemos o catálogo ao padrão do ExtraGrenade (invisível), para
-- não deixar cards sem PEF na loja.

UPDATE system_shop
SET item_visible = false,
    item_consume = 2,
    "Item_count_list" = '1,1,1,1',
    variant_code_list = '04,06,08,12',
    price_cash_list = '250,0,1200,4000',
    price_gold_list = '0,1,0,0'
WHERE item_id IN (1600109, 1600110);

DELETE FROM system_shop_effects WHERE coupon_id = 1700109;

SELECT item_id, item_visible, item_consume, variant_code_list, price_cash_list
FROM system_shop WHERE item_id IN (1600109, 1600110);
