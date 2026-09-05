using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Network.ServerPacket
{
    // Opcode 8456 (0x2108) — SeasonChallenge "+N season exp" battle-end push.
    // 122 wire (client SeasonChallenge__HandleSeasonAck @0xEF2C54, body deser
    // @0xEF2824; schema tools/pb-dev/client_schema/8456.json). Unlike 8455/8458
    // there is NO signed-i32 error gate; the client uses a generic list walk:
    //   H opcode, H pad, C field0 (walk-required, handler never reads it),
    //   B[21] USER_INFO_SEASON_CHALLENGE, D field2 (walk-required, never read).
    // The client qmemcpys USER_INFO into the season global (+4) and, on a level
    // delta, shows the level-up popup; otherwise the "+N exp" toast (it derives
    // +N itself from the exp delta, so no explicit gain field is on the wire).
    public class PROTOCOL_SEASON_CHALLENGE_PLUS_SEASON_EXP_ACK : GameServerPacket
    {
        private readonly Account Player;

        public PROTOCOL_SEASON_CHALLENGE_PLUS_SEASON_EXP_ACK(Account player) => this.Player = player;

        public override void Write()
        {
            this.WriteH((short)8456);
            this.WriteH((short)0);
            this.WriteC((byte)0);                                  // field0 (client-discarded)
            this.WriteB(AllUtils.BuildSeasonUserInfo(this.Player)); // USER_INFO (21 bytes)
            this.WriteD(0);                                        // field2 (client-discarded)
        }
    }
}
