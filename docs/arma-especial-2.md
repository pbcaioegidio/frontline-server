# Arma Especial 2 — cadeado do slot

## Sintoma

O slot **Arma Especial 2** aparece no inventário com cadeado e botão "Gold".
Ao clicar, o popup "Aviso" abre **vazio**: sem nome de item, seletor travado em
**0** e "Gold necessário **0 Gold**". Confirmar não faz nada e **nenhum pacote
chega ao servidor** (não há `EXTEND_REQ` op 1082 nos logs).

## O que já foi descartado

Dez tentativas no servidor não mudaram nada na UI:

| Tentativa | Resultado |
|---|---|
| Good `160010901` no packed catalog (op 1037) | popup continua 0/0 |
| Cupom `1700109` com goods `170010901..04` | idem |
| `price_cash_list` > 0 / `price_gold_list` > 0 | idem |
| `period` (+12) do record de good = dias | idem — e os goods que **funcionam** têm 0 aqui |
| ACK sintético de compra pós-catálogo | idem (e gerava exceção de DateTime no log) |
| Isenção de cobrança no `EXTEND_REQ` | nunca é alcançada, o client não envia |

Conclusão: **o popup não consulta o catálogo do servidor**.

## Comparação com o slot que funciona

O ExtraGrenade (item `1600035`) é o análogo funcional. Lendo os arquivos do
client byte a byte:

| | ExtraGrenade `1600035` | Arma Especial 2 `1600109` |
|---|---|---|
| Entrada no `ItemGroup.dat` | sim (grupos 633 e 644) | **não existe** |
| Goods no `Shop.dat` do client | sim (`170003501..04`) | **nenhum** |
| Vem do servidor? | não, é local | — |

No `ItemGroup.dat` a lista de itens de efeito da família 16000xx vai
`… 1600078, 1600079, 1600080` e **salta para 1600163**: este build de client
não conhece `1600109` nem `1600110`.

O `FrontLine.exe` não tem nenhum desses IDs escrito no binário, então o lookup
é feito por dados, não hardcoded.

## Pista atual: o seletor travado em zero

Teste no jogo: clicar na seta ▶ do popup **não** incrementa o valor. Logo o
máximo que o client tem é 0. Esse máximo deveria ser
`Throw2PointSlotMaxDays`, que o servidor manda no `SYSTEM_INFO` (op 2315) no
primeiro byte após o preamble — mas mandar 100 ali não destrava o seletor,
então o campo real está em outra posição do pacote.

O popup do "Passe de Batalha" (também com cadeado) se comporta de forma
diferente, o que indica que o dialog do Arma Especial 2 é específico do slot e
não um "recurso não configurado" genérico.

## Como testar posições do SYSTEM_INFO sem rebuild

`PROTOCOL_BASE_GET_SYSTEM_INFO_ACK` lê duas variáveis de ambiente:

- `PB_THROW2_MAXDAYS` — valor do byte `throw2PointSlotMax` (padrão 100)
- `PB_SYSINFO_FILL` — lista de blocos reservados a preencher, separados por vírgula
- `PB_SYSINFO_FILL_VALUE` — valor de preenchimento (padrão 99)

Blocos disponíveis: `dismantle` (4B), `shopctx` (5B), `ticket` (6B),
`reserved19` (3B), `penalty` (12B), `promotion` (373B), `match` (84B),
`gift` (109B), `clanseason` (120B).

Na VPS, sem deploy (só restart do container, ~10s):

```bash
/tmp/sysinfo-fill.sh "dismantle,shopctx,ticket,reserved19" 30
/tmp/sysinfo-fill.sh ""     # desliga
```

O script está em `scripts/sysinfo-fill.sh`. Depois de cada rodada é preciso
**sair da conta e entrar de novo** (o `SYSTEM_INFO` só é enviado no login) e
verificar se a seta ▶ passa a incrementar o seletor. Quando incrementar,
bissectar a lista até isolar o bloco e depois o byte.

Cuidado: `penalty` preenchido pode simular punição na conta, e `match` pode
afetar configuração de partida. Use por último e desligue depois.
