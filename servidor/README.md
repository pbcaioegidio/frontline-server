# Servidor FrontLine (PRISMBLEED)

Auth, Game e Match em **.NET 8**, compartilhados no mesmo host. No Windows use o monitor WinForms `FLMonitor`; no Linux/Docker, o console `PRISMBLEED.Server`.

---

## Requisitos

| Ambiente | Precisa |
|----------|---------|
| Windows | .NET 8 SDK |
| Linux / VPS | Docker (ou publish self-contained) · PostgreSQL |
| Runtime | pastas `Config`, `Data`, `Logs` (não vão na imagem Docker) |

---

## Windows

```bat
build-debug.bat
```

Gera:

- monitor: `binary\Debug\FLMonitor.exe`
- console: `binary\Debug\console\PRISMBLEED.Server.exe`

```powershell
binary\Debug\console\PRISMBLEED.Server.exe --content-root binary\Debug
```

No dia a dia no Windows, prefira o **FLMonitor**.

---

## Linux e Docker (WSL)

Build e teste sempre pelo WSL:

```powershell
.\scripts\build-linux-wsl.ps1
.\scripts\test-linux-wsl.ps1
```

Compose:

1. Copie `.env.example` → `.env` (só local; não commit)
2. Prepare `runtime/Config`, `runtime/Data`, `runtime/Logs`
3. Suba:

```powershell
.\scripts\compose-up-wsl.ps1
.\scripts\compose-down-wsl.ps1
```

Portas padrão: Auth `39190`, Game `39191`–`39193`, Match UDP `40009`, RCON `30000`.

Em produção: **não exponha RCON**. Guia: [`linux/SECURITY.md`](linux/SECURITY.md) · script [`linux/harden-firewall.sh`](linux/harden-firewall.sh).

Ajuste no `.env`: `PB_AUTH_PORT`, `PB_GAME_PORT_*`, `PB_MATCH_PORT`, `PB_RCON_PORT`.

---

## Configuração

Caminhos relativos usam `PB_CONTENT_ROOT` ou `--content-root`.

Banco (preferência em deploy):

| Variável | Uso |
|----------|-----|
| `PB_DB_HOST` | Host PostgreSQL |
| `PB_DB_PORT` | Porta |
| `PB_DB_NAME` | Nome do DB |
| `PB_DB_USER` / `PB_DB_PASS` | Credenciais |

- `PB_BIND_HOST=0.0.0.0` — bind interno no container  
- `PB_ADVERTISE_HOST` — IP anunciado ao client (local: `127.0.0.1`; VPS: IP público)

---

## Publish Linux + systemd

```powershell
.\scripts\publish-linux-wsl.ps1
```

Saída em `artifacts\linux-x64` (ou arm64 no fluxo ARM). Copie para `/opt/prismbleed`, coloque `Config` / `Data` / `Logs`, instale `deploy/prismbleed.service` e salve `PB_DB_*` em `/etc/prismbleed/server.env`.

---

## Encerramento

Ctrl+C / SIGINT / SIGTERM fecham Auth/Game/Match (e filhos se `ProcessSplit`). Compose e systemd usam SIGTERM com ~20 s de graça.

---

## TestBot

`Server.TestBot` — client headless para login, opcodes e handlers. Precisa de runtime + DB válidos.
