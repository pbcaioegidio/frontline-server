#!/usr/bin/env bash
# FrontLine VPS — firewall mínimo (Ubuntu/Debian + ufw)
# Uso: sudo bash harden-firewall.sh
# Ajuste IPs/portas se mudou Settings / .env

set -euo pipefail

# Portas do jogo (README)
AUTH_TCP=39190
GAME_TCP_1=39191
GAME_TCP_2=39192
GAME_TCP_3=39193
MATCH_UDP=40009
SOCKET_TCP=9000
SSH_TCP=22

echo "[FL] reset regras padrão"
ufw --force reset
ufw default deny incoming
ufw default allow outgoing

echo "[FL] SSH"
ufw allow "${SSH_TCP}/tcp" comment 'ssh'

echo "[FL] Auth/Game TCP"
ufw allow "${AUTH_TCP}/tcp" comment 'fl-auth'
ufw allow "${GAME_TCP_1}/tcp" comment 'fl-game-1'
ufw allow "${GAME_TCP_2}/tcp" comment 'fl-game-2'
ufw allow "${GAME_TCP_3}/tcp" comment 'fl-game-3'

echo "[FL] Match UDP"
ufw allow "${MATCH_UDP}/udp" comment 'fl-match'

echo "[FL] Socket launcher (login/HB/patch/evidence)"
ufw allow "${SOCKET_TCP}/tcp" comment 'fl-socket'

# NÃO abrir: RCON 30000, Postgres 5432/5433, painel HTTP — só localhost / depois
echo "[FL] RCON e Postgres ficam só em 127.0.0.1 (sem regra ufw pública)"

ufw --force enable
ufw status numbered
echo "[FL] OK. Confirme RconIp=127.0.0.1 e Postgres bind local."
