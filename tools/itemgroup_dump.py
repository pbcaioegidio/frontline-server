#!/usr/bin/env python3
"""Inspeciona ItemGroup.dat em torno das entradas do ExtraGrenade (1600035)."""
import struct
import sys

TARGET = 1600035


def hexdump(blob, start, length):
    for line in range(start, start + length, 16):
        chunk = blob[line:line + 16]
        text = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        print(f"   {line:08x}  {chunk.hex(' '):<47}  {text}")


def main(path):
    blob = open(path, "rb").read()
    print(f"== {path} ({len(blob)} B)")
    print("header:", blob[:32].hex(" "))
    hits = [o for o in range(len(blob) - 4)
            if struct.unpack_from("<I", blob, o)[0] == TARGET]
    print("hits:", [hex(o) for o in hits])
    for o in hits:
        print(f"\n-- contexto de {hex(o)} --")
        hexdump(blob, max(0, o - 64), 160)
        # tenta inferir stride: procura ids plausiveis de item em volta
        print("   ints proximos:")
        for d in range(-40, 44, 4):
            off = o + d
            if 0 <= off <= len(blob) - 4:
                val = struct.unpack_from("<I", blob, off)[0]
                print(f"     {d:+4d} -> {val}")


if __name__ == "__main__":
    main(sys.argv[1])
