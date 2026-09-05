using Plugin.Core.Models;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED6F4, handler 0xEF0DC6, consumer 0xD1F545):
    // opcode 6921 derives S2MOPacketBaseResultT<0x1B09>; wire order is
    //   u32 result
    //   u8 count, MATCH_MEMBER_INFO x count      (81B each, S2MOValue<...,8>)
    //   MATCH_TEAM_DETAIL_INFO                    (143B, single)
    //   s8  localSlot                             (applied only when hasLocalSlot)
    //   u8  hasLocalSlot
    //
    // This is also the team-roster push: the 122 REGIST_MERCENARY_ACK (6939) is a
    // bare result and carries no roster, so every "team changed" broadcast has to
    // be a 6921 to the whole team.
    public class PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK : GameServerPacket
    {
        public const int MaxMembers = 8;
        private const int NickWireBytes = 66;   // wchar_t[33] at MATCH_MEMBER_INFO+12
        private const int ClanNameWireBytes = 128; // wchar_t[64] at MATCH_TEAM_DETAIL_INFO+7

        private readonly uint Result;
        private readonly MatchModel Match;
        private readonly Account Viewer;

        public PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(uint result, MatchModel match = null, Account viewer = null)
        {
            this.Result = result;
            this.Match = match;
            this.Viewer = viewer;
        }

        public override void Write()
        {
            this.WriteH((short)6921);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;

            int seats = this.Match.Training;
            if (seats > MaxMembers)
                seats = MaxMembers;

            int members = 0;
            for (int slot = 0; slot < seats; slot++)
            {
                if (this.Match.Slots[slot].PlayerId > 0L)
                    members++;
            }

            this.WriteS2MOCount(members, MaxMembers);
            for (int slot = 0; slot < seats; slot++)
            {
                SlotMatch s = this.Match.Slots[slot];
                if (s.PlayerId <= 0L)
                    continue;
                Account member = this.Match.GetPlayerBySlot(s);
                this.WriteQ(s.PlayerId);                                   // +0  u64 player_id
                this.WriteC(member == null ? (byte)0 : (byte)member.Rank); // +8  u8  rank
                this.WriteC((byte)slot);                                   // +9  u8  slot_index
                this.WriteC((byte)0);                                      // +10 u8  rank_icon_ext
                this.WriteC((byte)0);                                      // +11 u8  UNKNOWN
                this.WriteU(member == null ? string.Empty : member.Nickname, NickWireBytes);
                this.WriteC((byte)0);                                      // +78 u8  UNKNOWN
                this.WriteC((byte)0);                                      // +79 u8  mmaking_rank_icon
                this.WriteC((byte)0);                                      // +80 u8  use_mmaking_rank_icon
            }

            // MATCH_TEAM_DETAIL_INFO, 143 bytes.
            this.WriteH((ushort)this.Match.MatchId);            // +0   u16 match_team_id
            this.WriteC((byte)this.Match.ChannelId);            // +2   u8  channel_id
            this.WriteC((byte)this.Match.ServerId);             // +3   u8  server_id
            this.WriteC((byte)this.Match.Training);             // +4   u8  UNKNOWN (capacity is the only value that fits)
            this.WriteC((byte)members);                         // +5   u8  current_player_count
            this.WriteC((byte)this.Match.Leader);               // +6   u8  leader_slot
            this.WriteU(this.Match.Clan == null ? string.Empty : this.Match.Clan.Name, ClanNameWireBytes);
            for (int slot = 0; slot < MaxMembers; slot++)       // +135 u8[8] slot_occupied_flags
                this.WriteC(slot < seats && this.Match.Slots[slot].PlayerId > 0L ? (byte)1 : (byte)0);

            bool hasLocalSlot = this.Viewer != null && this.Viewer.Match == this.Match && this.Viewer.MatchSlot >= 0;
            this.WriteC(hasLocalSlot ? (byte)this.Viewer.MatchSlot : (byte)0);
            this.WriteC(hasLocalSlot ? (byte)1 : (byte)0);
        }
    }
}
