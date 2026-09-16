-- FrontLine: rollback controlado da vitrine (teste A/B do Please Wait / crash 0x4897EF).
--
-- Contexto: o crash acontece ao ABRIR a loja (SHOP_ENTER op=1025), ~4s depois, sem o
-- client mandar mais nenhum pacote. O RandomBox.dat que geramos ja e identico ao retail,
-- entao o dado da caixa nao explica mais o crash. O que sobrou de diferente do baseline
-- sao as 88 emotes + as caixas que publicamos: Shop.dat saltou de 426 para 555 itens.
--
-- Este passo devolve a vitrine ao baseline (nada de caixa, nada de emote). Se a loja
-- voltar a abrir sem crash, confirmamos que o gatilho esta no que adicionamos e daí
-- religamos em lotes (emotes primeiro) para achar o item exato.
BEGIN;

-- Esconde todas as caixas aleatorias
UPDATE system_shop s
SET item_visible = false
WHERE EXISTS (SELECT 1 FROM system_random_boxes b WHERE b.box_id = s.item_id);

-- Esconde as emotes publicadas no lote de hoje
UPDATE system_shop
SET item_visible = false
WHERE item_id BETWEEN 4100000 AND 4199999;

COMMIT;
