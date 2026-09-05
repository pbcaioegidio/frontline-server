using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Network.ServerPacket
{
    // Opcode 8455 (0x2107) — SeasonChallenge premium-pass purchase ACK.
    // 122 wire (client SeasonChallenge__HandleBuySeasonPassAck @0xEF28F9 + vf3,
    // extracted via tools/unicorn/season_buy_ack_emu.py; see SEASON_BUYACK_FACTS.md):
    //   H opcode, H pad, D error (SIGNED i32; < 0 => client shows msgbox and reads
    //   no body), B[21] USER_INFO_SEASON_CHALLENGE, D field1 (parsed then discarded).
    // Error must be 0 (success) or a negative SEASON_ERR_* HRESULT; a positive code
    // is read by the client as success and truncates the body.
    public class PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK : GameServerPacket
    {
        private readonly int Error;
        private readonly Account Player;

        public PROTOCOL_SEASON_CHALLENGE_BUY_SEASON_PASS_ACK(int error, Account player)
        {
            this.Error = error;
            this.Player = player;
        }

        public override void Write()
        {
            this.WriteH((short)8455);
            this.WriteH((short)0);
            this.WriteD(this.Error);
            if (this.Error < 0)
                return;

            this.WriteB(AllUtils.BuildSeasonUserInfo(this.Player)); // USER_INFO (21 bytes)
            this.WriteD(0);                                        // field1 (client-discarded)
        }
    }
}
