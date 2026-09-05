using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SHOP_FLASH_SALE_LIST_ACK : GameServerPacket
    {
        private const int Opcode = 1120;
        private const int EntrySize = 121;
        private const int StartDateOffset = 4;
        private const int EndMinuteOffset = 22;
        private const int CountOffset = 26;
        private const int GoodsOffset = 27;
        private const int MaxGoods = 3;

        private readonly List<int> _goodIds;
        private readonly uint _startDate;
        private readonly int _endMinute;

        public PROTOCOL_SHOP_FLASH_SALE_LIST_ACK(IEnumerable<int> goodIds, uint startDate, int endMinute)
        {
            _goodIds = goodIds == null ? new List<int>() : new List<int>(goodIds);
            _startDate = startDate;
            _endMinute = endMinute;
        }

        public override void Write()
        {
            WriteH((short)Opcode);

            if (_goodIds.Count == 0 || _startDate == 0 || _endMinute <= 0)
            {
                WriteD(0);
                return;
            }

            WriteD(1);

            byte[] entry = new byte[EntrySize];

            // Header bytes preserved verbatim from a live PBBR 1120 capture; the client reads
            // startDate at +4 (packed YYMMDDHHMM), the deadline as minute-of-day at +22,
            // numGoods at +26 and a u32 goodId[] at +27.
            entry[0] = 0x0D;                                        // flashSaleId
            entry[18] = 0x01;
            entry[20] = 0x01;
            entry[120] = 0x38;

            WriteLE(entry, StartDateOffset, (int)_startDate);
            WriteLE(entry, EndMinuteOffset, _endMinute);

            int count = _goodIds.Count < MaxGoods ? _goodIds.Count : MaxGoods;
            entry[CountOffset] = (byte)count;
            for (int i = 0; i < count; i++)
                WriteLE(entry, GoodsOffset + i * 4, _goodIds[i]);

            WriteB(entry);
        }

        private static void WriteLE(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }
    }
}
