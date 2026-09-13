#!/usr/bin/env bash
# Liga/desliga o preenchimento de blocos do SYSTEM_INFO na VPS e reinicia o server.
# Uso: sysinfo-fill.sh "<lista de blocos>" [valor]
#   sysinfo-fill.sh "dismantle,shopctx,ticket,reserved19" 30
#   sysinfo-fill.sh ""            # desliga
set -euo pipefail

ROOT=/opt/frontline/servidor
FILL="${1-}"
VALUE="${2-30}"

cd "$ROOT"

set_var() {
  local key="$1" val="$2"
  if grep -q "^${key}=" .env; then
    sed -i "s|^${key}=.*|${key}=${val}|" .env
  else
    printf '%s=%s\n' "$key" "$val" >> .env
  fi
}

set_var PB_SYSINFO_FILL "$FILL"
set_var PB_SYSINFO_FILL_VALUE "$VALUE"

echo "--- .env ---"
grep -E '^PB_SYSINFO|^PB_THROW2' .env || true

COMPOSE=(docker compose -f docker-compose.vps.yml)
[ -f docker-compose.hostnet.yml ] && COMPOSE+=(-f docker-compose.hostnet.yml)
COMPOSE+=(--env-file .env)

"${COMPOSE[@]}" up -d --no-build server >/dev/null
sleep 6

echo "--- env no container ---"
"${COMPOSE[@]}" exec -T server env | grep -E 'PB_SYSINFO|PB_THROW2' || true
echo "OK"
