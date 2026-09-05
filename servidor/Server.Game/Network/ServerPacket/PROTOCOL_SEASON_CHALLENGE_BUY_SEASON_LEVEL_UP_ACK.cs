using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Network.ServerPacket
{
    // Opcode 8458 (0x210A) — SeasonChallenge level-skip purchase ACK.
    // 122 wire (client SeasonChallenge__HandleBuyAck @0xEF2B37 + vf3 @0xEF26F5,
    // extracted via tools/unicorn/season_buy_ack_emu.py; see SEASON_BUYACK_FACTS.md):
    //   H opcode, H pad, D error (SIGNED i32; < 0 => client shows msgbox and reads
    //   no body), D field0 (parsed then discarded), B[21] USER_INFO_SEASON_CHALLENGE,
    //   D field2 (parsed then discarded).
    public class PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_LEVEL_UP_ACK : GameServerPacket
    {
        private readonly int Error;
        private readonly Account Player;

        public PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_LEVEL_UP_ACK(int error, Account player)
        {
            this.Error = error;
            this.Player = player;
        }

        public override void Write()
        {
            this.WriteH((short)8458);
            this.WriteH((short)0);
            this.WriteD(this.Error);
            if (this.Error < 0)
                return;

            this.WriteD(0);                                        // field0 (client-discarded)
            this.WriteB(AllUtils.BuildSeasonUserInfo(this.Player)); // USER_INFO (21 bytes)
            this.WriteD(0);                                        // field2 (client-discarded)
        }
    }
}
