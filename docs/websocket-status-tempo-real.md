# WebSocket no jogo → Discord `#status` (tempo real)

> **Status:** plano futuro (WebSocket no jogo) — **ainda não implementar**.  
> **Agora em produção:** Postgres `LISTEN`/`NOTIFY` em `accounts.online`  
> (`database/migrations/migration_notify_frontline_online_20260909.sql` + bot `status.js`).  
> Poll continua só para saúde Docker. Este doc é o passo seguinte (WS dedicado no C#).

Relacionado:

- Bot: `frontline-discord-bot/src/status.js`
- Online no DB: `accounts.online` (`SetOnlineStatus` em Auth/Game)
- RCON atual (WS admin, **não** reutilizar para público): `ws://127.0.0.1:30000` · `Server.Game/Rcon/`

---

## Por que WebSocket no jogo (e não só Gateway Discord)

| Peça | Papel |
|------|--------|
| **Gateway Discord** | Tempo real *dentro* do Discord (já usado pelo bot via discord.js). Não sabe se o jogador logou no FrontLine. |
| **Poll Postgres** (atual) | Simples; atraso de até ~15s. Suficiente para um embed. |
| **WebSocket no jogo** (este plano) | O *game* avisa o bot no instante do login/logout/crash. O bot edita o embed; o Discord entrega via Gateway. |

Fluxo alvo:

```
Jogador loga/desloga no FrontLine
        ↓
Servidor (Auth/Game) emite evento WS
        ↓
Bot Discord recebe evento
        ↓
Edita a mesma mensagem em #status (REST)
        ↓
Players veem o embed atualizado (Gateway MESSAGE_UPDATE)
```

---

## O que **não** reutilizar

1. **Docker `socket` (:9000)** — launcher TCP (versão, login do launcher, anticheat). Não é canal de status Discord.
2. **RCON Fleck (:30000)** — WebSocket **admin** (kick, cash, rank…). Bind em `127.0.0.1`, token sensível. Misturar status público com RCON aumenta superfície de ataque e acopla o bot a comandos de admin.

Preferência: **serviço WS dedicado** (ex.: porta interna `30001` ou path `/status` em outro host), só eventos de presença/saúde, token separado do RCON.

---

## Desenho proposto

### 1. Servidor (C# — Auth + Game)

Novo módulo leve, por exemplo `Server.Game/StatusFeed/` ou compartilhado em `Plugin.Core`:

- Servidor WebSocket (Fleck já existe no projeto, ou `System.Net.WebSockets`)
- Bind **só rede Docker interna** (ex.: `0.0.0.0:30001` publicado **sem** mapear para a internet; bot conecta pelo nome do container)
- Auth na conexão: header/`?token=` com `STATUS_FEED_TOKEN` (env), diferente do RCON
- Ao chamar `SetOnlineStatus(true|false)` (e no reset de boot `ValidateAllPlayersAccount`), publicar evento JSON

**Eventos sugeridos:**

```json
{ "type": "player.online",  "playerId": 123, "nickname": "Foo", "online": 42, "ts": 1710000000 }
{ "type": "player.offline", "playerId": 123, "nickname": "Foo", "online": 41, "ts": 1710000000 }
{ "type": "snapshot",       "online": 41, "server": "up", "ts": 1710000000 }
{ "type": "server.health",  "auth": true, "game": true, "ts": 1710000000 }
```

- No **connect** do bot: mandar um `snapshot` com contagem atual (evita estado inicial errado).
- Heartbeat periódico (`ping`/`pong` ou `snapshot` a cada N minutos) para detectar WS morto.

Pontos de ganchos no código existente:

- `Server.Auth/Data/Models/Account.cs` → `SetOnlineStatus`
- `Server.Game/Data/Models/Account.cs` → `SetOnlineStatus`
- Disconnect `AuthClient` / `GameClient`
- Boot: `ComDiv.ValidateAllPlayersAccount()` (todos offline)

Auth e Game podem ambos emitir; o bot **deduplica** por `playerId` + `ts` ou confia na contagem do `snapshot`/`online` no payload.

### 2. Bot (`frontline-discord-bot`)

Em `src/status.js` (ou `src/status-feed.js`):

- Cliente WS → `ws://servidor-server-1:30001` (nome do container na rede Docker)
- Em cada evento: atualizar contadores em memória + **editar** embed (já existe lógica de mesma `STATUS_MESSAGE_ID`)
- Se WS cair: **fallback** para o poll atual (Postgres + Docker) até reconectar
- Docker health (containers up/down) continua por `docker.sock` **ou** entra no evento `server.health` se o game exportar

Env novos (VPS / compose):

```env
STATUS_FEED_URL=ws://servidor-server-1:30001
STATUS_FEED_TOKEN=...
# manter CHANNEL_STATUS / STATUS_MESSAGE_ID
```

Rede: bot e `servidor-server-1` já precisam compartilhar rede Docker (hoje o bot usa `DATABASE_URL` no host `db` — mesma ideia).

### 3. Segurança

- Token forte; sem exposição pública da porta
- Não enviar senha, HWID, IP, cash, etc. no feed
- Rate-limit no bot: coalescer edições Discord (ex.: no máximo 1 edit / 2s se houver burst de login)
- Logs do feed sem dados sensíveis

### 4. Compose / deploy

- Variáveis no container `server` (e Auth se separado)
- Porta **não** no firewall da VPS; só bridge interna
- Documentar no `docker-compose.vps.yml` quando for a hora

---

## Alternativa mais barata (se mudar de ideia)

**Postgres `LISTEN`/`NOTIFY`** com trigger em `UPDATE accounts SET online`:

- Zero (ou quase) mudança no C#
- Bot faz `LISTEN frontline_online`
- Ainda é “push”, sem WS no jogo

Fica como plano B. O plano A deste doc é **WS no jogo**, como preferido pelo time.

---

## Critérios de pronto (quando for implementar)

- [ ] WS dedicado (não RCON), token próprio, bind interno
- [ ] Eventos `player.online` / `player.offline` / `snapshot`
- [ ] Bot conecta, reconecta, fallback poll
- [ ] Embed `#status` atualiza em &lt; ~2s após login/logout (com coalescing)
- [ ] Porta não acessível da internet
- [ ] Sem regressão no RCON / launcher socket

---

## Estimativa grosseira

| Parte | Esforço |
|-------|---------|
| Feed WS no C# + ganchos `SetOnlineStatus` | médio |
| Cliente + fallback no bot | baixo–médio |
| Rede Docker / env / testes | baixo |
| **Total** | ~1–2 sessões focadas |

---

## Decisão atual

- **Agora:** manter poll em `status.js`.
- **Depois:** implementar este WebSocket de status feed conforme acima.
- Não misturar com Gateway Discord (já é tempo real só na entrega do embed).
