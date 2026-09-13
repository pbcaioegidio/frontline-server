#!/usr/bin/env python3
"""Delimita as tabelas de 12B do ItemGroup.dat a partir de um offset conhecido."""
import struct
import sys


def u32(b, o):
    return struct.unpack_from("<I", b, o)[0]


def valid(b, o):
    if o < 0 or o + 12 > len(b):
        return False
    grp, item, flag = struct.unpack_from("<III", b, o)
    return grp < 5000 and 100000 <= item <= 9999999 and flag in (0, 50)


def bounds(b, seed):
    start = seed
    while valid(b, start - 12):
        start -= 12
    end = seed
    while valid(b, end + 12):
        end += 12
    end += 12
    n = (end - start) // 12
    print(f"\n-- tabela do seed {hex(seed)}: {hex(start)}..{hex(end)} = {n} registros")
    print(f"   u32 antes do inicio: {[u32(b, start - 4 * k) for k in (1, 2, 3)]}")
    print("   primeiros 4:")
    for i in range(min(4, n)):
        print("     ", struct.unpack_from("<III", b, start + i * 12))
    print("   ultimos 4:")
    for i in range(max(0, n - 4), n):
        print("     ", struct.unpack_from("<III", b, start + i * 12))
    return start, end, n


if __name__ == "__main__":
    blob = open(sys.argv[1], "rb").read()
    print(f"== {sys.argv[1]} ({len(blob)} B)")
    for seed in (0x39AC, 0x801C, 0x9048, 0x174A8):
        bounds(blob, seed)
