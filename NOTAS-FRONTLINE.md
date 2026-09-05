# FrontLine — Notas de config

Data: 2026-08-30

## Pacote aplicado (binary/Debug)

| Item | Valor / arquivo |
|---|---|
| Region | `Brazil` → `Config\Settings.ini` |
| Strings | `Config\Translate\Strings.ini` (PT + FRONTLINE) |
| Anúncios / Shop | `Data\ServerConfig.json` (PT, OfficialAddress = FrontLine) |
| ClientVersion | **`123.0`** — não mudar pra 122 (quebra login do pack BR) |
| Rcon | `Config\Rcon.ini` — senha `FrontLine2026` |

## Pendências

- Trocar `OfficialBanner` no JSON quando tiver arte FrontLine hospedada (hoje: URL genérica)
- Em produção: IPs públicos, `Test=False`, senha Rcon forte se exposto

## Após editar config

Reiniciar **FLMonitor** (ou recarregar config no painel).

## FLMonitor UI (1.0.1+)

- **WinExe** — sem terminal preto; logs na aba **Logs**
- Atualização automática a cada 1s (sem botão Atualizar)
- Abas: Monitor · Extra · Logs · Config · Serviços
- Extra: uptime real, portas, ProcessSplit, DB
- Ícone cavalheiro transparente (`FLMonitor.ico`) — se cache antigo: reiniciar Explorer ou trocar atalho

## CheatBlocker

Manter `client\CHEAT_BLOCKER` completo. Remoção/patch automático inviável (strings ofuscadas). Proteção = anti-cheat servidor + CB no client.
