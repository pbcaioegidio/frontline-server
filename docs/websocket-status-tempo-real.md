# WebSocket no jogo → Discord `#status` (tempo real)

> **Status produção (2026-09):**
> 1. **Postgres `LISTEN`/`NOTIFY`** em `accounts.online` — ativo (`migration_notify_frontline_online_20260909.sql` + bot `status.js`).
> 2. **IA no bot** — `/ai` com Groq (principal) + OpenRouter free (fallback) em `ai.js`.
> 3. **WebSocket StatusFeed (C#)** — porta interna `30001`, token próprio; bot conecta e usa LISTEN como fallback.

Relacionado:

- Bot: `frontline-discord-bot/src/status.js` (+ `ai.js`)
- Online no DB: `accounts.online` (`SetOnlineStatus` em Auth/Game)
- Feed WS: `Server.Game/StatusFeed/` · hub `Plugin.Core/StatusFeed/`
- RCON atual (WS admin, **não** misturar): `ws://127.0.0.1:30000` · `Server.Game/Rcon/`

---

## Por que dois canais (NOTIFY + WS)

| Peça | Papel |
|------|--------|
| **Gateway Discord** | Tempo real *dentro* do Discord (discord.js). |
| **Postgres NOTIFY** | Push quando `accounts.online` muda (Auth **e** Game, sem porta extra). |
| **WebSocket StatusFeed** | Snapshot no connect, heartbeat, eventos do processo Game; bot pode priorizar WS e cair no LISTEN se WS cair. |
| **Poll Docker** | Só saúde de containers (~30s). |

Fluxo:

```
Jogador loga/desloga
        ↓
SetOnlineStatus → UPDATE accounts.online
        ↓
      ┌─── NOTIFY frontline_online ───┐
      │                               │
      ▼                               ▼
Bot LISTEN                      StatusFeed WS (Game)
      │                               │
      └──────────► edita #status ◄────┘
```

---

## O que **não** reutilizar

1. **Docker `socket` (:9000)** — launcher TCP.
2. **RCON Fleck (:30000)** — admin; token/IP sensíveis.

StatusFeed = porta **30001**, token **`STATUS_FEED_TOKEN`**, bind interno Docker.

---

## Desenho

### 1. Servidor (C# — Game)

- `StatusFeedServer` (Fleck) em `ws://0.0.0.0:30001`
- Auth na 1ª mensagem ou query `?token=`
- `StatusFeedHub.PublishPlayer` / `PublishSnapshot` a partir de `SetOnlineStatus` (Game) e boot
- No **connect**: `snapshot` com contagem atual
- Heartbeat `snapshot` a cada N minutos

**Eventos:**

```json
{ "type": "player.online",  "playerId": 123, "nickname": "Foo", "online": 42, "ts": 1710000000 }
{ "type": "player.offline", "playerId": 123, "nickname": "Foo", "online": 41, "ts": 1710000000 }
{ "type": "snapshot",       "online": 41, "server": "up", "ts": 1710000000 }
```

Auth também chama o hub; no processo Auth (sem servidor WS) o publish é no-op — o NOTIFY cobre o Discord.

### 2. Bot

- `STATUS_FEED_URL=ws://servidor-server-1:30001`
- `STATUS_FEED_TOKEN=...`
- Cliente WS + reconexão; se cair → só LISTEN + poll Docker
- Coalesce edits Discord (~1,5s)

### 3. Segurança

- Token forte; porta **não** no firewall público
- Sem senha/HWID/IP/cash no feed
- Rate-limit / coalesce no bot

### 4. Compose

```env
PB_STATUS_FEED_ENABLE=true
PB_STATUS_FEED_BIND_HOST=0.0.0.0
PB_STATUS_FEED_PORT=30001
PB_STATUS_FEED_TOKEN=...
# bot:
STATUS_FEED_URL=ws://servidor-server-1:30001
STATUS_FEED_TOKEN=...  # mesmo token
```

Porta 30001 **sem** publish na internet (só rede `servidor_default`).

---

## Critérios de pronto

- [x] LISTEN/NOTIFY + bot `#status`
- [x] IA Groq → OpenRouter
- [x] WS dedicado (não RCON), token próprio, bind interno
- [x] Eventos `player.online` / `player.offline` / `snapshot`
- [x] Bot conecta, reconecta, fallback LISTEN
- [x] Porta não acessível da internet (sem map host)
- [x] Sem regressão no RCON / launcher socket
