using System;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED838): opcode 6915 derives plain
    // S2MOPacketBaseT<0x1B03> (no result dword) and registers exactly two
    // S2MOValue<int,1> nodes -> body is two little-endian int32, in list order.
    // The 122 client has no 6914 sender, so this is a server push: it rides along
    // with MATCH_TEAM_LIST_ACK (6917) to feed the team-list pager.
    // Field meaning is INFERENCE from the 068 body it replaces (total, pages).
    public class PROTOCOL_CLAN_WAR_MATCH_TEAM_COUNT_ACK : GameServerPacket
    {
        public const int TeamsPerPage = 13;

        private readonly int Total;
        private readonly int Pages;

        public PROTOCOL_CLAN_WAR_MATCH_TEAM_COUNT_ACK(int total)
        {
            this.Total = total;
            this.Pages = (int)Math.Ceiling(total / (double)TeamsPerPage);
        }

        public override void Write()
        {
            this.WriteH((short)6915);
            this.WriteH((short) 0);
            this.WriteD(this.Total);
            this.WriteD(this.Pages);
        }
    }
}
