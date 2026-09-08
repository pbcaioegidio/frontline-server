# Segurança na VPS

Guia de exposição de portas, Socket/Evidence e checklist antes de abrir para jogadores.

## O que NÃO abrir na internet

| Serviço | Porta | Exposição |
|---------|-------|-----------|
| RCON | 30000 | **Só** `127.0.0.1` ([Rcon.ini](out/linux-arm64/Config/Rcon.ini) `RconIp=127.0.0.1`) |
| Postgres | 5432/5433 | **Só** localhost / rede Docker interna |
| Auth | 39190 TCP | Público |
| Game | 39191–39193 TCP | Público |
| Match | 40009 UDP | Público |
| Socket | 9000 TCP | Público (launcher remoto) |

Script: [`harden-firewall.sh`](harden-firewall.sh) (`sudo bash harden-firewall.sh`).

## Socket + Evidence na VPS

1. Rode `Socket.exe` / `Socket` **na VPS** (não no PC do jogador).
2. Evidências em `Evidence/{player_id}/` ao lado do Socket, ou pasta custom:

```bash
export FL_EVIDENCE_ROOT=/var/frontline/Evidence
```

3. No client do jogador, o IP do launcher (`IP_ADRESS` / config) aponta para o **IP da VPS**, não `127.0.0.1`.
4. Com host remoto, o FLLauncher **não** sobe Socket local ([`SocketBootstrap`](../../launcher/Point Blank Launcher/Launcher.PointBlank/Services/SocketBootstrap.cs)).

## Flags Security (Settings.ini)

Já ligadas no build Debug / linux:

- `RequireOtpToken`, `OtpOneShot`, `RequireLauncherHeartbeat`
- `AntiScript`, `AutoBan` (AutoBan = hardban genérico; **não** liga aimbot sozinho)
- `KillBurstWindowSeconds=8`, `KillBurstMaxKills=5` (FG-124 + clip)

### Aimbot / FOV (Match)

| Chave | Default | Efeito |
|-------|---------|--------|
| `AimbotDetect` | true | Liga HS% + snap |
| `AimbotHsMinHits` | 14 | Mínimo de hits antes de avaliar HS% |
| `AimbotHsRatioFlag` | 0.88 | HS/hits para armas normais → FG-130 |
| `AimbotHsRatioFlagHighDmg` | 0.96 | Limiar **mais alto** p/ sniper/shotgun/RPG/dano≥180 |
| `AimbotHighDamageThreshold` | 180 | Damage base ItemStatistic |
| `AimbotSnapDegrees` / `AimbotSnapMaxIntervalMs` | 62° / 90ms | FG-132 (+18° se high-dmg) |
| `AimbotFreezeViolations` | 3 | Congela na partida + clip (high-dmg precisa +1) |
| `AimbotAutoBan` | **true** | Hardban automático após N violações (não high-dmg) |
| `AimbotBanViolations` | 8 | Só se `AimbotAutoBan=true` e **não** high-dmg |

Automático: `flag` em `security_events` + `RequestCapture` (clip) + freeze na partida.  
**Ban permanente:** GM chat / RCON / `ApplyHardBan` manual. Não banir à toa sniper/RPG.

### Match UDP flood

| Chave | Default | Efeito |
|-------|---------|--------|
| `MatchUdpMaxPacketsPerSecond` | 140 | Drop por IP; log FG-140 a cada ~30s (0 = off) |

## Checklist antes de jogadores reais

- [ ] Firewall aplicado
- [ ] RCON só localhost
- [ ] Postgres sem porta pública
- [ ] Socket na VPS + Evidence gravando
- [ ] Launcher do player com IP VPS
- [ ] Patch: tag `client-v*` (Actions) → FileList + `Data/Client` + bump `ClientVersion` (ver [`docs/deploy-github-actions.md`](../../docs/deploy-github-actions.md))
- [ ] Teste: matar launcher → kick HB; burst kills → FG-124
- [ ] Teste: HS% absurdo → FG-130 + clip; com `AimbotAutoBan=true` hardban após N violações (não high-dmg)
- [ ] Revisar `security_events` / Evidence e banir via GM se confirmar cheat
