using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED586, handler 0xEF0B52, consumer 0x9C9719):
    // opcode 6919 derives S2MOPacketBaseResultT<0x1B07> and registers a single
    // S2MOValue<MATCH_TEAM_INFO,1>. Body is `u32 result` + exactly 6 bytes.
    // The 068 body wrote 15 bytes of team+clan blob; the client only ever read
    // the first 6 of them, so every field landed on the wrong slot.
    public class PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK : GameServerPacket
    {
        private readonly uint Result;
        private readonly MatchModel Match;

        public PROTOCOL_CLAN_WAR_CREATE_TEAM_ACK(uint result, MatchModel match = null)
        {
            this.Result = result;
            this.Match = match;
        }

        public override void Write()
        {
            this.WriteH((short)6919);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            // The client reads MATCH_TEAM_INFO unconditionally, so the 6 bytes have to be
            // on the wire even on the error paths, which all construct this packet with a
            // null match. WriteMatchTeamInfo stays strict for its other caller.
            if (this.Match != null)
                this.WriteMatchTeamInfo(this.Match);
            else
                this.WriteB(new byte[6]);
        }
    }
}
