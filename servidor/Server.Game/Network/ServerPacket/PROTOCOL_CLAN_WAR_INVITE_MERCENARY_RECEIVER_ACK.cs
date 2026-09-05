using Plugin.Core.Models;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED606, handler 0xEF0C4B): opcode 6944 derives
    // S2MOPacketBaseResultT<0x1B20>; wire order is
    //   u32 result
    //   u8     clan_rank_grade   -> STBL_IDX_CLAN_RANK_*        (0xA93827)
    //   u8     clan_mark_effect  -> ImgBox_ClanEffect, 1-based  (0xAA4192)
    //   double clan_point_index  -> STR_CLAN_CLANPOINT_IDX      (0xAA3EE7)
    //   u8     clan_unit_level   -> STBL_IDX_CLAN_UNIT_*, 0..7  (0xA9373E)
    //   u32    clan_points       -> ProgressBar_ClanExp         (0xAA401B)
    //   u32    clan_mark_id      -> ImgBox_ClanMark             (0xAA4152)
    //   StrW17 inviter_nickname  -> STR_MERC_INVITE_POPUP       (0xC3E179)
    // This is the "a clan invited you as a mercenary" popup: everything but the
    // nickname describes the INVITING CLAN.
    //
    // The 068 body wrote opcode 1572 with a single byte; the 122 client has no
    // parser for 1572 at all.
    public class PROTOCOL_CLAN_WAR_INVITE_MERCENARY_RECEIVER_ACK : GameServerPacket
    {
        private readonly uint Result;
        private readonly ClanModel Clan;
        private readonly string InviterNickname;

        public PROTOCOL_CLAN_WAR_INVITE_MERCENARY_RECEIVER_ACK(uint result, ClanModel clan = null, string inviterNickname = null)
        {
            this.Result = result;
            this.Clan = clan;
            this.InviterNickname = inviterNickname ?? string.Empty;
        }

        public override void Write()
        {
            this.WriteH((short)6944);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;
            this.WriteC((byte)this.Clan.Rank);
            this.WriteC((byte)this.Clan.Effect);
            this.WriteF(this.Clan.Points);
            this.WriteC((byte)this.Clan.GetClanUnit());
            this.WriteD((uint)this.Clan.Exp);
            this.WriteD(this.Clan.Logo);
            this.WriteStrW(this.InviterNickname, 17);
        }
    }
}
