using Server.Game.Data.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED9B5, handler 0xEF106F, store 0x9C8DEB):
    // opcode 6937 derives S2MOPacketBaseResultT<0x1B19>; wire order is
    //   u32 result
    //   u8 count, MERCENARY_INFO x count      (72B each, S2MOValue<...,50>)
    //   u16 pageIndex        — page 0 clears the board before appending (0x9C8DFF)
    //   u16 pageItemCount    — the count the ingest loop actually uses (0x9C8E16)
    //   u16 unused           — never read by the 122 client
    // As in 6917, the consumer ignores the S2MO count byte and trusts the trailing
    // count, so the two must agree.
    public class PROTOCOL_CLAN_WAR_MERCENARY_LIST_ACK : GameServerPacket
    {
        public const int MaxRows = 50;
        private const int NickWireBytes = 66;   // wchar_t[33] at MERCENARY_INFO+6

        private readonly uint Result;
        private readonly List<Account> Mercenaries;

        public PROTOCOL_CLAN_WAR_MERCENARY_LIST_ACK(List<Account> mercenaries)
        {
            this.Mercenaries = mercenaries ?? new List<Account>();
        }

        public PROTOCOL_CLAN_WAR_MERCENARY_LIST_ACK(uint result)
        {
            this.Result = result;
            this.Mercenaries = new List<Account>();
        }

        public override void Write()
        {
            this.WriteH((short)6937);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;
            int count = this.Mercenaries.Count > MaxRows ? MaxRows : this.Mercenaries.Count;
            this.WriteS2MOCount(count, MaxRows);
            for (int i = 0; i < count; i++)
            {
                Account merc = this.Mercenaries[i];
                // MERCENARY_INFO, 72 bytes.
                this.WriteH((ushort)i);                 // +0 u16 mercenary_list_id (board row key,
                                                        //    echoed by 6942/6950/6974)
                this.WriteC((byte)0);                   // +2 u8  UNKNOWN (stored, never displayed)
                this.WriteC((byte)0);                   // +3 u8  class_type (merc rank icon category)
                this.WriteC((byte)merc.Rank);           // +4 u8  mer_rank, clamped 0..9 by the client
                this.WriteC((byte)0);                   // +5 u8  UNKNOWN (stored, never displayed)
                this.WriteU(merc.Nickname, NickWireBytes);
            }
            this.WriteH((ushort)0);                     // pageIndex: single page, always clears
            this.WriteH((ushort)count);                 // pageItemCount: authoritative loop count
            this.WriteH((ushort)0);                     // unused by the 122 client
        }
    }
}
