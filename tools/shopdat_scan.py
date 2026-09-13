#!/usr/bin/env python3
"""Varre Shop.dat procurando goods das familias do cadeado Arma Especial 2."""
import struct
import sys
import zlib

FAMILIES = (1600035, 1600109, 1600110, 1700035, 1700109, 1700110)


def sections(data):
    """Devolve (nome, bytes) — bruto + qualquer stream zlib embutido."""
    yield "raw", data
    for i in range(len(data) - 2):
        if data[i] == 0x78 and data[i + 1] in (0x9C, 0xDA, 0x01, 0x5E):
            try:
                out = zlib.decompressobj().decompress(data[i:])
            except zlib.error:
                continue
            if len(out) > 4096:
                yield f"zlib@{i}", out


def scan(path):
    data = open(path, "rb").read()
    print(f"== {path} ({len(data)} B) head={data[:16].hex()}")
    for name, blob in sections(data):
        hits = {}
        for off in range(0, len(blob) - 4):
            val = struct.unpack_from("<I", blob, off)[0]
            fam = val // 100
            if fam in FAMILIES and 0 <= val % 100 <= 99:
                hits.setdefault(val, []).append(off)
            elif val in FAMILIES:
                hits.setdefault(val, []).append(off)
        if not hits:
            continue
        print(f"  [{name}] {len(blob)} B")
        for val in sorted(hits):
            offs = hits[val]
            print(f"    {val}  x{len(offs)}  offs={offs[:6]}")


if __name__ == "__main__":
    for p in sys.argv[1:]:
        scan(p)
