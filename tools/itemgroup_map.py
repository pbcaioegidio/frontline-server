#!/usr/bin/env python3
"""Tenta mapear a estrutura de ItemGroup.dat como sequencia de tabelas."""
import struct
import sys


def u32(blob, off):
    return struct.unpack_from("<I", blob, off)[0]


def head_ints(blob, count=48):
    print("primeiros u32:")
    for i in range(count):
        off = i * 4
        print(f"   [{i:3d}] off={off:6d} = {u32(blob, off)}")


def walk(blob, rec):
    """Percorre o arquivo assumindo [u32 count][count*rec] repetido."""
    off = 0
    tables = []
    while off + 4 <= len(blob):
        n = u32(blob, off)
        size = n * rec
        if n > 100000 or off + 4 + size > len(blob):
            return tables, off, n
        tables.append((off, n))
        off += 4 + size
    return tables, off, None


if __name__ == "__main__":
    blob = open(sys.argv[1], "rb").read()
    print(f"== {sys.argv[1]} ({len(blob)} B)")
    head_ints(blob)
    for rec in (8, 12, 16, 20):
        tables, off, bad = walk(blob, rec)
        status = "COMPLETO" if off == len(blob) else f"parou em {off} (count={bad})"
        print(f"\nrec={rec}: {len(tables)} tabelas, {status}")
        if len(tables) < 40:
            for o, n in tables:
                print(f"   off={o:8d} count={n}")
