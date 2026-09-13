#!/usr/bin/env python3
"""Lista entradas de ItemGroup.dat (12B: grupo, itemId, flag) na faixa 16000xx."""
import struct
import sys

LO, HI = 1600000, 1600400


def main(path):
    blob = open(path, "rb").read()
    print(f"== {path} ({len(blob)} B)")
    rows = []
    for off in range(0, len(blob) - 12, 4):
        grp, item, flag = struct.unpack_from("<III", blob, off)
        if LO <= item <= HI and grp < 100000 and flag in (0, 50):
            rows.append((off, grp, item, flag))
    print(f"entradas encontradas: {len(rows)}")
    for off, grp, item, flag in rows:
        mark = "  <<< ALVO" if item in (1600109, 1600110) else ""
        print(f"   off={off:08x} grupo={grp:<6} item={item:<9} flag={flag}{mark}")


if __name__ == "__main__":
    main(sys.argv[1])
