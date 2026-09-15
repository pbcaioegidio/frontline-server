# Plano: IA com visão e controle do jogo ao vivo (salas / partidas / treino)

> **Status:** plano — **não implementar ainda**.  
> Objetivo: a IA no Discord (canal `#ai` / DM do dono) ver e, depois, controlar o que está acontecendo **dentro do jogo** (lobby, salas, treino, partida), sem inventar dados.

Relacionado:

- StatusFeed atual: `docs/websocket-status-tempo-real.md` · porta `30001`
- RCON admin: `ws://127.0.0.1:30000` · já tem `SNAPSHOT` de salas
- Bot: `guardian-ai.js`, `admin-tools.js`, `status.js`

---

## O que a IA já controla / vê hoje

| Capacidade | Como | Limite |
|------------|------|--------|
| Contas, gold, cash, ban, vínculo Discord | Postgres | OK |
| Quem está **online** (logado) | NOTIFY + StatusFeed | Não diz em qual sala |
| Kick “destravar” | `UPDATE accounts.online = false` | Não tira da sala de verdade |
| Logs / Docker | `#logs` + docker.sock | Texto, não estrutura de sala |
| Salas / partida / treino | — | **Não existe** → IA inventa |

---

## Onde vivem as salas (estrutura real)

```
ChannelsXML.Channels[]
  └─ ChannelModel          (Public | Training | Clan | …)
       ├─ Rooms[]          ← salas de lobby/partida (SÓ RAM)
       └─ Matches[]        ← times de clan war (SÓ RAM)
```

Arquivos-chave:

- `Server.Game/Data/Models/RoomModel.cs` — nome, mapa, modo, state, slots, host, competitive…
- `Server.Game/Data/Models/ChannelModel.cs` — tipo do canal (ex.: **Training**)
- `Server.Game/Data/Models/MatchModel.cs` — clan war (campo `Training` = tamanho do time, **não** é canal treino)
- `Server.Game/Rcon/RconSnapshot.cs` — **já serializa salas em JSON** (usado pelo RCON `SNAPSHOT`)

**Importante:** salas **não** estão no Postgres. Gravar no DB seria pior (volatilidade, inconsistência). A fonte da verdade é a RAM do processo Game.

---

## Opções (sugestão ranqueada)

### A — Estender o StatusFeed (recomendado)

Mandar `rooms.snapshot` / eventos de sala no WS `30001` que o bot **já usa**.

| Prós | Contras |
|------|---------|
| Canal limpo, token próprio, interno | Precisa enriquecer o feed no C# |
| Bot já conectado | Hooks em create/join/leave |
| Não mistura com RCON admin | — |
| Reaproveita lógica do `RconSnapshot` | — |

### B — Bot chama RCON `SNAPSHOT`

| Prós | Contras |
|------|---------|
| Já funciona hoje no Game | Senha RCON no bot |
| Controle (`KICKHOST`, `ENDBATTLE`…) já existe | Sem push (poll) |
| | Mistura painel admin com Discord |

### C — Espelhar salas no Postgres + NOTIFY

| Prós | Contras |
|------|---------|
| Parecido com online | Write storm, atraso, fonte errada |
| | **Evitar** |

**Veredito:** fazer **A** para visão; na fase de controle, StatusFeed restrito **ou** RCON só para writes owner-only (B parcial).

---

## O que colocar na IA (payload sugerido)

No connect / a cada N segundos / quando sala muda:

```json
{
  "type": "rooms.snapshot",
  "lobbyPlayers": 12,
  "roomCount": 3,
  "ts": 1710000000,
  "rooms": [
    {
      "serverId": 1,
      "channelId": 0,
      "channelType": "Training",
      "roomId": 2,
      "name": "treino faca",
      "map": "Crackdown",
      "roomType": "DeathMatch",
      "state": "BATTLE",
      "competitive": false,
      "leader": "NickHost",
      "players": 6,
      "maxSlots": 16,
      "slots": [
        { "playerId": 42, "nickname": "Foo", "team": "FR", "kills": 3, "deaths": 1, "isLeader": true }
      ]
    }
  ]
}
```

Perguntas que a IA passa a acertar:

- “Tem treino rolando?”
- “Quem tá em partida?”
- “Onde está o zRhydm?”
- “Quantas salas no canal Training?”

**Nunca** no feed: senha da sala, IP, HWID, tokens.

---

## Fases sugeridas

### Fase 1 — Só visão (prioridade)

1. Reusar/enriquecer `RconSnapshot` → publicar no StatusFeed
2. Bot: cache `getRooms()` / `findPlayerInRoom(nick)`
3. `guardian-ai` + `#ai`: injetar salas no contexto (sem inventar)
4. Comandos dono: `salas?`, `onde está X?`

**Pronto quando:** pergunta “tem treino?” usa dados reais, não chute.

### Fase 2 — Controle (owner-only)

| Ação | Base | Nota |
|------|------|------|
| Kick do servidor | RCON `KICKPLAYER` | Sessão de verdade |
| Tirar da sala | `RemovePlayer` | Novo opcode / feed control |
| Kick host | `KICKHOST` | Já existe |
| Encerrar partida | `ENDBATTLE` | Já existe |
| Balancear | `BALANCETEAMS` | Já existe |

Regras de segurança:

- Só `OWNER_DISCORD_ID`
- ID composto `(serverId, channelId, roomId)` — **nunca** só `roomId`
- Confirmação em ação destrutiva
- Audit no `#logs`
- Distinguir: unstuck DB ≠ kick sessão ≠ kick da sala

### Fase 3 — Polimento de salas (opcional)

- Embed `#status` com “X salas ativas” (sem lista pública de nicks)
- Clan war (`clanMatches`) no mesmo snapshot
- Delta events (`room.created` / `player.room`) além do snapshot

---

## Backlog — outras ideias para autonomia da IA (não perder)

> Escopo além de salas. Implementar **depois** das Fases 1–2.  
> Regra geral: **visão ampla** + **execução só sob ordem do dono** no `#ai`/DM (não autonomia solta no chat público).

### Ordem sugerida (após salas)

| # | Ideia | O que a IA ganha | Valor | Depende de |
|---|--------|------------------|-------|------------|
| 4 | **Localizar jogador** | “Onde está o X?” → canal + sala + slot (ou lobby) | Alto | Fase 1 |
| 5 | **Anti-cheat assist** | Resume `security_events` / `live_sessions` (suspect, capture_blocked) e **sugere** kick/ban — tu confirma | Alto | Já tem DB |
| 6 | **Histórico rico (só `#ai`)** | Últimos kicks FL Guard, logins, IPs, motivo | Alto | Já tem DB |
| 7 | **Economia auditada** | +cash/+gold com log obrigatório no `#logs` + quem mandou | Alto | Já tem admin-tools |
| 8 | **Whitelist de ações** | IA só executa verbs permitidos (`kick`, `unstuck`, `+cash`, `endbattle`…) | Segurança | Fase 2 |
| 9 | **Anúncios / manutenção** | Um comando → aviso Discord + (opcional) announce no jogo via RCON | Médio | RCON announce |
| 10 | **Fila / lobby** | Quem está no canal sem sala | Médio | Fase 1 (lobby no snapshot) |
| 11 | **Clan war ao vivo** | Times CF no mesmo feed das salas | Médio | Fase 3 |
| 12 | **Relatório diário** | DM ao dono: pico online, bans, shop, salas criadas, flags AC | Médio | Agregações DB |
| 13 | **Memória por jogador** | Expandir `player_ai_memories` (“já ajudou X com antivírus”) | Baixo–médio | Já existe base |
| 14 | **Mute in-game via Discord** | “IA, muta o X por 30m” no jogo (chat/whisper/nota) — ver nota abaixo | Alto | Mute **já existe** no PB; falta bridge Discord/RCON |
| 15 | **Chat do jogo → Discord** | Staff/IA vê falas de lobby/sala (moderação) | Médio | Hook chat packets / log |
| 16 | **Watchlist** | Nick marcado → alerta no `#ai` ao logar / entrar em sala | Alto | Tabela + StatusFeed |
| 17 | **Detecção stack/smurf** | Mesmo HWID/IP → aviso (não ban auto) | Alto | `live_sessions` / login_audit |
| 18 | **Loja / passe** | Compras do dia, status battle pass | Médio | shop_audit / player_battlepass |
| 19 | **Clã** | Membros online, CF, cargos | Médio | clan_* + Fase 3 |
| 20 | **Replay / evidência AC** | Link/clip quando FL Guard flagar | Médio | capture / storage |
| 21 | **Filas Discord ↔ jogo** | Quem pediu `#filas` vs quem entrou na sala | Médio | Fase 1 + canal Discord |
| 22 | **Manutenção programada** | Countdown + kick suave + status manutenção | Médio | RCON announce + Fase 2 |
| 23 | **Backup / wipe assist** | Lembra checklist SQL — **não executa sozinho** | Baixo | Docs/scripts |
| 24 | **Health avançado** | CPU/RAM VPS, lag, salas travadas em BATTLE | Médio | Docker metrics + rooms |
| 25 | **Ticket interno** | Reclamação → card no `#ai` com nick + logs | Médio | guardian + logs |
| 26 | **Comandos GM proxy** | Host-command / GM log room sob ordem do dono | Médio | RCON / GM packets |
| 27 | **FAQ vivo / guia** | Respostas alinhadas ao `#guia-servidor` sempre atualizadas | Baixo | Prompt + canal |
| 28 | **Integridade → servidor** | FileCheck reporta arquivos inválidos/extras + restore sim/não no DB/`#logs`/IA | Alto | Socket + tabela |

### Detalhe rápido de cada item

#### 4. Localizar jogador
- Índice no bot a partir de `rooms.snapshot` + `player.room`
- Resposta: `nick → Training / sala "treino faca" / slot 3 / BATTLE`
- Se offline: “não está online”; se online sem sala: “no lobby do canal X”

#### 5. Anti-cheat assist
- Fontes: `security_events`, `live_sessions` (status suspect / capture_blocked)
- IA monta resumo + botões: Desconectar / Banir / Dispensar (já existe padrão no dossiê)
- **Nunca** ban automático sem dono

#### 6. Histórico rico
- Só canal `#ai` / DM: IP, login_audit, kicks, ban_identifiers
- Chat público continua sem vazar IP/UID

#### 7. Economia auditada
- Todo `+cash` / `+gold` → linha no `#logs` + (opcional) tabela `shop_audit` / audit própria
- Prompt: nunca inventar saldo; sempre ler do banco depois da ação

#### 8. Whitelist de ações
- Lista fixa no código: o que a IA pode pedir ao “executor”
- Qualquer verbo fora da lista → recusa (“não tenho essa ação”)
- Evita prompt injection pedindo “formata o VPS”

#### 9. Anúncios / manutenção
- `IA, manda aviso oficial: …` (já parcial no Discord)
- Estender: RCON announce in-game + countdown manutenção

#### 10. Fila / lobby
- Incluir `lobbyPlayers[]` no snapshot (nick + channelId)
- “Quem tá parado no lobby sem sala?”

#### 11. Clan war ao vivo
- `clanMatches[]` no StatusFeed (ver Fase 3)
- Cuidado: `MatchModel.Training` = tamanho do time ≠ canal Training

#### 12. Relatório diário
- Cron no bot (ex.: 09:00) ou Discord scheduled
- Métricas: online pico 24h, novos cadastros, bans, eventos AC, compras shop
- DM só para `OWNER_DISCORD_ID`

#### 13. Memória por jogador
- Já existe `player_ai_memories`
- Expandir: tags (sniper, antivírus OK, recidiva kick) + injetar no diagnóstico

#### 14. Mute in-game via Discord
- **No servidor PB o mute JÁ EXISTE** — não precisa inventar mecânica nova:
  - `Account.IsMuted()` em `Server.Game/Data/Models/Account.cs`
  - Bloqueia: chat geral (`PROTOCOL_BASE_CHATTING_REQ`), clan chat, whisper, notas
  - Aplicação GM: `PROTOCOL_GMCHAT_APPLY_PENALTY_ACK` → `SaveBanHistory(..., "MUTE", …)` com duração
  - Coluna relacionada: `accounts.mute_expire` / histórico em `base_ban_history` tipo `MUTE`
- **O que falta para a IA:** bridge Discord → jogo
  - Hoje o bot só faz **timeout Discord** (`muta @user`) — não mute in-game
  - RCON **não** tem opcode de MUTE (grep zerado em `Rcon/`)
  - Implementar: `MUTEPLAYER` / `UNMUTEPLAYER` no RCON **ou** StatusFeed control, reusando `SaveBanHistory` + refresh do player online
- Comando dono: `IA, muta o nick X por 30m no jogo` / `desmuta o X`

#### 15. Chat do jogo → Discord
- Espelhar (ou sample) chat lobby/sala para canal staff / contexto da IA
- Privacidade: só staff; sem gravar forever se não precisar

#### 16. Watchlist
- Tabela `ai_watchlist (player_id, reason, created_by)`
- No `player.online` / join room → DM ou `#ai` “watchlist: X logou”

#### 17. Detecção stack/smurf
- Cruzar HWID/IP em `live_sessions` / `login_audit`
- Alerta informativo; ação só com dono

#### 20. Replay / evidência AC (como fazer — domínio)

**Já existe no FrontLine (não reinventar):**

```
Kick/flag/RCON SCREENSHOT|CLIP
        ↓
capture_requests (Postgres: pending)
        ↓
FL Guard / Launcher (poll ~2.5s) captura
  • screenshot JPEG da janela do jogo
  • clip = ring buffer ~20s → ZIP/MP4 (ffmpeg)
        ↓
Arquivo na VPS: Evidence/{player_id}/…
        ↓
capture_requests.status = completed + evidence_path
```

Código: `SecurityDao.RequestCapture`, RCON `SCREENSHOT`/`CLIP`, `FL.Guard.Core/Capture/ScreenCapture.cs`, `EvidenceFfmpeg.cs`.  
`capture_blocked` = jogador bloqueou captura (ex.: ring sem frames) — já cai em `security_events`.

**O que falta (usar `frontlinebattle.com.br`):**

1. Pasta estável na VPS, ex.: `/var/frontline/evidence/` (`FL_EVIDENCE_ROOT`)
2. Nginx (mesmo domínio):
   - `location /evidence/` → alias dessa pasta
   - **NÃO** público aberto: Basic Auth staff, ou URL assinada (token curto), ou só bot lê e manda no Discord
3. Bot / IA:
   - No dossiê ou `#ai`: lista últimas `capture_requests` completed
   - Botão “Pedir clip” / “Pedir print” → `RequestCapture` (RCON ou INSERT)
   - Quando `completed`: embed com link `https://www.frontlinebattle.com.br/evidence/...` **ou** upload do arquivo na DM/`#logs` (Discord CDN)
4. Anti-cheat assist (#5): se `capture_blocked` ou clip pronto → card no `#ai` com link

**Recomendação com teu domínio:**  
`https://www.frontlinebattle.com.br/evidence/{player_id}/{arquivo}` protegido (token ou só rede interna + bot anexa no Discord). Staff vê no Discord; jogador comum não lista a pasta.

**Futuro no site (`frontline-web`):**  
página staff (login / só equipe) listando clips/prints do jogador, player no dossiê web, player embutido.  
Enquanto isso: Discord basta (link ou anexo). O Vite **não** versiona os arquivos de evidence — só consome URL da VPS.

**Arquitetura (não esquecer):** tudo na **mesma VPS**, repos separados:

| Peça | Repo local | Na VPS | Deploy (GitHub Actions) |
|------|------------|--------|-------------------------|
| Game + Socket + DB | `frontline-server` | `/opt/frontline` | **Com tag** `server-v*` |
| Launcher | `frontline-server` | VPS (build/upload) | **Com tag** `launcher-v*` |
| Client patch (delta) | `frontline-server` | Socket / FileList | **Com tag** `client-v*` |
| Bot Discord | `frontline-discord-bot` | `~/frontline-discord-bot` | **Sem tag** — push em `main` (ou `workflow_dispatch`) → rsync + `docker compose up -d --build` |
| Site | `frontline-web` | `/var/www/frontlinebattle` | **Com tag** `site-v*` |
| Evidence (arquivos) | — (runtime) | ex. `/var/frontline/evidence/` | — |
| Domínio | nginx | `frontlinebattle.com.br` | site + `/downloads/` + futuro `/evidence/` |

Tags no monorepo `frontline-server`: `server-v*` · `launcher-v*` · `client-v*`.  
Site: `site-v*` no repo `frontline-web`. Bot: sem tag.

**Não fazer:** pasta `/evidence/` com `autoindex on` sem senha.

1. FileCheck acha hash errado / faltando  
2. Popup: lista o arquivo + *“Deseja restaurar o padrão baixando estes arquivos do servidor?”*  
3. **Sim** → `RestoreIntegrityFilesAsync` baixa do Socket → VERIFICAR de novo → pode INICIAR  
4. **Não** / falha → status com mensagem + botões VERIFICAR / Update  

Extras fora da lista: o Guard **apaga sozinho** e segue (“removeu N extra”).

**Status #28 (2026-09-15):** código launcher/socket/bot pronto — `integrity_events` + opcodes 5100/5101 + painel/`#logs` + IA.  
**Deploy:** migration na VPS; bot via `main`; reports ao vivo só após **`launcher-v*`** (+ rebuild socket).  
**JoTaZera (arquivo quebrado):** patch/`Update` + restore no launcher basta — **não** precisa Full no R2 / reinstalar.

| Item | Estado |
|------|--------|
| Report ao Socket | feito |
| Tabela `integrity_events` | feito |
| `#logs` / IA / dossiê | feito |
| UX opcional | backlog |

**Sugestões extras (cliente ↔ staff):**

1. Logar falha de restore (path ausente no Socket / FileList desatualizada)  
2. Versão do `UserFileList` do client vs servidor  
3. Extra/delete falhou (AV travou arquivo) → reportar  
4. N fails de integridade em 24h → alerta `#ai` (mod/crack?)  
5. Motivo do Update (quais arquivos baixou)

---

## O que **não** colocar na autonomia (lista negra)

| Proibido | Motivo |
|----------|--------|
| Ban permanente sem confirmação do dono | Irreversível / falso positivo |
| Expor senha de conta, token, RCON, `.env` | Segurança |
| Kick/ban em massa “porque a IA achou” | Abuso / erro de modelo |
| Salas no Postgres como fonte da verdade | Inconsistência |
| RCON password dentro do prompt do LLM | Vazamento |
| Autonomia admin no chat **público** | Jogador pode tentar manipular |

---

## Modelo de autonomia (combinado)

| Camada | Comportamento |
|--------|----------------|
| **Visão** | 100% dos dados permitidos (salas, AC, contas) no `#ai` |
| **Controle** | Executa só sob ordem do dono + whitelist |
| **Público** | Suporte ao jogador; sem kick/ban/cash; sem UID/IP |

Assim a IA é **braço direito com poder total**, não “IA solta no servidor”.

---

## Riscos a lembrar

1. `roomId` não é global (é por canal) → kick errado se usar só o número
2. Clan “Training” (tamanho do time) ≠ canal Training
3. Prompt da IA hoje promete controle demais → atualizar limites após Fase 1
4. Não colocar senha RCON no prompt do LLM

---

## Sugestão prática (roadmap completo)

1. **Fase 1** — visão de salas via StatusFeed  
2. **Fase 2** — controle owner-only (kick / endbattle / tirar da sala)  
3. **Fase 3** — polimento salas + clan war + lobby  
4. **#4–5** — localizar jogador + anti-cheat assist  
5. **#6–8** — histórico rico, economia auditada, whitelist  
6. **#9–13** — anúncios, relatório diário, memória  
7. **#14** — mute in-game via Discord (mecânica já no PB; falta RCON/bridge)  
8. **#15–27** — watchlist, smurf, loja, clã, chat→Discord, etc. (puxar por prioridade)  
9. **#28** — integridade FileCheck → Socket/DB/`#logs` (controle do que o jogador restaura)  

**Não fazer:** salas no Postgres; RCON no fluxo público da IA; ban automático sem dono.

Quando quiser implementar, começar pela **Fase 1**.
