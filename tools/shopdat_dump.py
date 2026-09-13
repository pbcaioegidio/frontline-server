#!/usr/bin/env python3
"""Dump dos registros de good (210B) e item (11B) do Shop.dat do client."""
import struct
import sys

GOOD = 210
ITEM = 11


def dump_good(blob, off, label):
    rec = blob[off:off + GOOD]
    gid = struct.unpack_from("<I", rec, 0)[0]
    print(f"-- good {label} GoodsID={gid} off={off}")
    print(f"   flag0={rec[4]} flag1={rec[5]} saletype(+22)={rec[22]}")
    for opt in range(12):
        o = 6 + opt * 17
        gold, cash, d2, period = struct.unpack_from("<IIII", rec, o)
        code = rec[o + 16]
        if gold or cash or d2 or period or code:
            print(f"   opt{opt:<2} gold={gold} cash={cash} d2={d2} period={period} code={code}")
    print("   hex:", rec[:60].hex())


def dump_item(blob, off, label):
    rec = blob[off:off + ITEM]
    iid = struct.unpack_from("<I", rec, 0)[0]
    print(f"-- item {label} ItemId={iid} off={off} rest={rec[4:].hex()}")


if __name__ == "__main__":
    blob = open(sys.argv[1], "rb").read()
    for off, label in ((59272, "170003501"), (59482, "170003502"),
                       (59692, "170003503"), (59902, "170003504")):
        dump_good(blob, off, label)
    for off, label in ((174542, "1600035"), (174696, "1600109"), (174707, "1600110")):
        dump_item(blob, off, label)
    # vizinhos do item 1600109 para ver a grade de 11B
    print("\n-- janela de items em torno de 174696 --")
    for off in range(174696 - 33, 174696 + 44, ITEM):
        iid = struct.unpack_from("<I", blob, off)[0]
        print(f"   off={off} ItemId={iid} rest={blob[off + 4:off + ITEM].hex()}")
