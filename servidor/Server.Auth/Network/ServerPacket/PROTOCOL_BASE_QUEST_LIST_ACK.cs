using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.IO;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_QUEST_LIST_ACK : AuthServerPacket
    {
        // The 121 client (cNetworkData<P_QUEST_PRESET,1>::FillPackedData) zlib-inflates the
        // payload into N fixed-size P_QUEST_PRESET blocks (sizeof = 0x3F60 = 16224) and reads
        // at most 8875 packed bytes per chunk. Wire: [total packed][offset][len][bytes]... with
        // one [offset][len][bytes] segment per chunk. Must stay wire-identical to the
        // Server.Game copy.
        private const int PresetSize = 16224;
        private const int MaxChunkLen = 8875;

        // zlib( 0x00 * 16224 ) = one empty P_QUEST_PRESET block; known-good fallback so the
        // quest panel builds instead of hanging on a zero-preset list.
        private static readonly byte[] EmptyPresetZlib =
        {
            0x78, 0xDA, 0xED, 0xC1, 0x01, 0x0D, 0x00, 0x00, 0x00, 0xC2, 0xA0, 0xF7, 0x4F, 0x6D, 0x0F, 0x07,
            0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0xFC, 0x1B, 0x3F, 0x60, 0x00, 0x01
        };

        private readonly byte[] _rawPresets;

        public PROTOCOL_BASE_QUEST_LIST_ACK() : this(LoadPreset())
        {
        }

        private static byte[] _cachedPreset;

        // One retail P_QUEST_PRESET block (16224B), loaded once from Data\Quest.bin (the client's
        // own cached copy of a real preset). Missing/misaligned -> empty -> blank board (old behavior).
        private static byte[] LoadPreset()
        {
            if (_cachedPreset != null)
                return _cachedPreset;

            byte[] data = new byte[0];
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Quest.bin");
                if (File.Exists(path))
                {
                    byte[] raw = File.ReadAllBytes(path);
                    if (raw.Length > 0 && raw.Length % PresetSize == 0)
                        data = raw;
                    else
                        CLogger.Print("QUEST_LIST: Data\\Quest.bin missing or misaligned; sending empty.", LoggerType.Warning);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("QUEST_LIST preset load: " + ex.Message, LoggerType.Error, ex);
            }

            _cachedPreset = data;
            return data;
        }

        // rawPresets = N concatenated 16224-byte preset blocks (opaque); null/empty/misaligned
        // falls back to a single all-zero preset.
        public PROTOCOL_BASE_QUEST_LIST_ACK(byte[] rawPresets)
        {
            this._rawPresets = rawPresets != null && rawPresets.Length > 0 && rawPresets.Length % PresetSize == 0
                ? rawPresets
                : new byte[PresetSize];
        }

        public override void Write()
        {
            byte[] packed = ZlibUtil.Compress(this._rawPresets);
            if (packed == null || packed.Length == 0)
                packed = EmptyPresetZlib;

            WriteH((short)8710);
            WriteD(packed.Length);
            for (int offset = 0; offset < packed.Length; offset += MaxChunkLen)
            {
                int len = Math.Min(MaxChunkLen, packed.Length - offset);
                WriteD(offset);
                WriteD(len);
                WriteB(packed, offset, len);
            }
        }
    }
}
