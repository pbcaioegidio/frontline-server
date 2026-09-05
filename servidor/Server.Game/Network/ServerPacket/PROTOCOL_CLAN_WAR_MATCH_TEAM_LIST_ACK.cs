using Server.Game.Data.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED8E7, handler 0xEF0F5D, consumer 0x9C95D9
    // ClanWar__UpdateMyMatchTeamList): opcode 6917 derives
    // S2MOPacketBaseResultT<0x1B05>; wire order is
    //   u32 result
    //   u8 count, MATCH_TEAM_INFO x count   (S2MOValue<MATCH_TEAM_INFO,26>)
    //   u8 listCount
    // The consumer iterates `listCount` entries over the array at stride 6
    // (imul eax,6 at 0x9C9621) and never looks at the S2MO count byte, so the two
    // counts MUST agree. The 068 body wrote neither count correctly.
    public class PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_ACK : GameServerPacket
    {
        public const int MaxTeams = 26;

        private readonly uint Result;
        private readonly List<MatchModel> Teams;

        public PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_ACK(List<MatchModel> teams)
        {
            this.Teams = teams ?? new List<MatchModel>();
        }

        public PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_ACK(uint result)
        {
            this.Result = result;
            this.Teams = new List<MatchModel>();
        }

        public override void Write()
        {
            this.WriteH((short)6917);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;
            int count = this.Teams.Count > MaxTeams ? MaxTeams : this.Teams.Count;
            this.WriteS2MOCount(count, MaxTeams);
            for (int i = 0; i < count; i++)
                this.WriteMatchTeamInfo(this.Teams[i]);
            this.WriteC((byte)count);
        }
    }
}
