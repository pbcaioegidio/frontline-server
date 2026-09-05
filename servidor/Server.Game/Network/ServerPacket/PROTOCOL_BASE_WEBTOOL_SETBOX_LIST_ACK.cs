using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.IO;

namespace Server.Game.Network.ServerPacket
{
    // Client (122) handler opcode 0x9C7=2503 (sub_EC3AFE -> cNetworkData<WEBTOOL_SETBOX_DATA,100>):
    // accumulates the compressed payload, zlib-inflates it (sub_EAD689, zlib "1.2.11") into N fixed
    // WEBTOOL_SETBOX_DATA blocks (sizeof = 0x268 = 616), asserts size%616==0 and count<=100, then
    // rewrites client\WebtoolSetBox.dat and builds the lobby promo card. Wire = [total packed][ (offset)
    // (len)(bytes) per chunk of <=8875 ]. Same packed-stream shape as PROTOCOL_BASE_QUEST_LIST_ACK.
    public class PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_ACK : GameServerPacket
    {
        private const int EntrySize = 616;
        private const int MaxChunkLen = 8875;

        private static byte[] _cachedEntries;

        private readonly byte[] _entries;

        public PROTOCOL_BASE_WEBTOOL_SETBOX_LIST_ACK()
        {
            this._entries = LoadEntries();
        }

        // N concatenated 616-byte WEBTOOL_SETBOX_DATA blocks, loaded once from Data\WebtoolSetBox.bin
        // (retail dump, count-header stripped). Misaligned/missing file -> empty list (card hidden).
        private static byte[] LoadEntries()
        {
            if (_cachedEntries != null)
                return _cachedEntries;

            byte[] data = new byte[0];
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "WebtoolSetBox.bin");
                if (File.Exists(path))
                {
                    byte[] raw = File.ReadAllBytes(path);
                    if (raw.Length > 0 && raw.Length % EntrySize == 0 && raw.Length / EntrySize <= 100)
                        data = raw;
                    else
                        CLogger.Print("WEBTOOL_SETBOX: Data\\WebtoolSetBox.bin misaligned or >100 entries; sending empty.", LoggerType.Warning);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("WEBTOOL_SETBOX load: " + ex.Message, LoggerType.Error, ex);
            }

            _cachedEntries = data;
            return data;
        }

        public override void Write()
        {
            byte[] packed = ZlibUtil.Compress(this._entries);

            this.WriteH((short)2503);
            this.WriteD(packed.Length);
            for (int offset = 0; offset < packed.Length; offset += MaxChunkLen)
            {
                int len = Math.Min(MaxChunkLen, packed.Length - offset);
                this.WriteD(offset);
                this.WriteD(len);
                this.WriteB(packed, offset, len);
            }
        }
    }
}
