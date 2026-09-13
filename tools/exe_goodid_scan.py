#!/usr/bin/env python3
"""Procura GoodsIDs hardcoded no binario do client.

Calibracao: o ExtraGrenade (item 1600035 / cupom 1700035) funciona, entao se os
IDs dele aparecerem no binario sabemos que o client hardcoda esse tipo de lookup.
"""
import struct
import sys

BASES = (1600035, 1600109, 1600110, 1700035, 1700109, 1700110, 1707109, 1730109)


def scan(path):
    data = open(path, "rb").read()
    print(f"== {path} ({len(data)} B)")
    found = {}
    for off in range(0, len(data) - 4):
        val = struct.unpack_from("<I", data, off)[0]
        if val in BASES:
            found.setdefault(val, []).append(off)
            continue
        base = val // 100
        if base in BASES and val >= 100000000:
            found.setdefault(val, []).append(off)
    if not found:
        print("   (nenhum hit)")
    for val in sorted(found):
        offs = found[val]
        print(f"   {val:<12} x{len(offs):<4} offs={[hex(o) for o in offs[:8]]}")


if __name__ == "__main__":
    for p in sys.argv[1:]:
        scan(p)
