# Arma Especial 2 — slot escondido

## Decisão

Neste build de client o slot **Arma Especial 2** foi **escondido**
(`Throw2PointSlotMaxDays = 0` no `SYSTEM_INFO`).

## Por quê

O cadeado abria um popup "Aviso" vazio (0 dias / 0 Gold) e o Confirmar não
enviava nada ao servidor. Causa: o `ItemGroup.dat` do client (FrontLine e
também PB Sea v3.123) lista efeitos `16000xx` e **pula** de `1600080` para
`1600163` — sem entrada para `1600109` / `1600110`. Sem isso o client não
resolve o good do BuyExtend.

O ExtraGrenade (`1600035`) funciona porque está no `ItemGroup.dat` e tem goods
`170003501..04` no `Shop.dat` local.

O `Shop.dat` do PB Sea tem `160010901`, mas o `ItemGroup.dat` continua sem o
item — incompleto para unlock via servidor.

## O que ficou limpo

- Byte do slot = `0` (some do inventário)
- Cupom `1700109` removido; goods `1600109`/`1600110` invisíveis no banco
- Removidos ACK sintético, isenção de `EXTEND_REQ`, helpers `PB_SYSINFO_FILL`
  e script de diagnóstico
