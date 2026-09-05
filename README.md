<p align="center">
  <img src="docs/frontline-logo.png" alt="FrontLine" width="220" />
</p>

<h1 align="center">FrontLine Server</h1>

<p align="center">
  <b>Servidor · Launcher · Banco</b> para Point Blank (client FrontLine / 3.122 BR)
</p>

<p align="center">
  <img alt="Privado" src="https://img.shields.io/badge/repo-privado-111827?style=flat-square" />
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4?style=flat-square&logo=dotnet&logoColor=white" />
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-14+-4169E1?style=flat-square&logo=postgresql&logoColor=white" />
  <img alt="Docker" src="https://img.shields.io/badge/Docker-compose-2496ED?style=flat-square&logo=docker&logoColor=white" />
  <img alt="ARM64" src="https://img.shields.io/badge/Linux-x64%20%7C%20arm64-FCC624?style=flat-square&logo=linux&logoColor=black" />
</p>

---

## O que tem aqui

| Pasta | Conteúdo |
|:-----:|----------|
| [`servidor/`](servidor/) | Auth · Game · Match · FLMonitor · Docker · hardening VPS |
| [`launcher/`](launcher/) | FLLauncher · Socket (patch/login/heartbeat) · FLConfig |
| [`database/`](database/) | Dump + migrations PostgreSQL |

Client completo, packs e EXEs de publish **não** entram no Git — patch sobe pelo Socket.

---

## Arquitetura rápida

```text
  Jogador                         VPS
 ─────────                       ─────
  FLLauncher ──TCP :9000──► Socket (+ Evidence)
       │
  FrontLine ──TCP 39190───► Auth
            ──TCP 39191+──► Game
            ──UDP 40009───► Match
                              │
                         PostgreSQL (só rede local)
```

---

## Começar

1. **Banco** — veja [`database/README.md`](database/README.md)
2. **Servidor** — veja [`servidor/README.md`](servidor/README.md) (Windows / Docker / Linux)
3. **Launcher** — veja [`launcher/README.md`](launcher/README.md)
4. **Segurança VPS** — veja [`servidor/linux/SECURITY.md`](servidor/linux/SECURITY.md)

---

## Portas (produção)

| Serviço | Porta | Exposição |
|---------|-------|-----------|
| Socket (launcher) | `9000` TCP | Público |
| Auth | `39190` TCP | Público |
| Game | `39191`–`39193` TCP | Público |
| Match | `40009` UDP | Público |
| RCON | `30000` TCP | **Só localhost** |
| PostgreSQL | `5432` / `5433` | **Só localhost / Docker** |

---

## Segurança (resumo)

- OTP + heartbeat do launcher (FL Guard)
- Match anti-burst (FG-124 + clip + kick)
- Evidence na VPS, não no PC do jogador
- Secrets só em `.env` / INI local — nunca no Git

---

## Licença / uso

Repositório **privado**. Código e assets para operação do FrontLine — não redistribuir client oficial de terceiros.
