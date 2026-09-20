#!/usr/bin/env python3
from pathlib import Path
p = Path(".env")
text = p.read_text(encoding="utf-8")
pairs = {
  "LAVALINK_HOST": "lava-v4.ajieblogs.eu.org",
  "LAVALINK_PORT": "443",
  "LAVALINK_PASS": "https://dsc.gg/ajidevserver",
  "LAVALINK_SECURE": "true",
}
lines = text.splitlines()
keys = set()
out = []
for line in lines:
  if "=" in line and not line.strip().startswith("#"):
    k = line.split("=", 1)[0].strip()
    if k in pairs:
      out.append(f"{k}={pairs[k]}")
      keys.add(k)
      continue
  out.append(line)
for k, v in pairs.items():
  if k not in keys:
    out.append(f"{k}={v}")
p.write_text("\n".join(out) + "\n", encoding="utf-8")
print("env lavalink -> publico ajieblogs")
