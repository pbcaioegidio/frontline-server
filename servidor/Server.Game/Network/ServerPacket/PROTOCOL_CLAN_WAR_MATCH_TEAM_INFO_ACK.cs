using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED891, handler 0xEF093E): opcode 6930 derives
    // plain S2MOPacketBaseT<0x1B12> — there is NO result dword. Wire order is
    //   u8 count, MATCHMAKING_RECORD x count        (21B each, max 18)
    //   u8 count, CLAN_MATCHING_TEAM_INFO x count   (59B each, max 2)
    // It feeds the clan-war matchmaking screen: the per-slot scoreboard rows and
    // the two facing teams (Blue_ClanTitle / Red_ClanTitle).
    //
    // The 068 body wrote opcode 1570 with a clan-detail blob; the 122 client has no
    // parser for 1570.
    public class PROTOCOL_CLAN_WAR_MATCH_TEAM_INFO_ACK : GameServerPacket
    {
        public const int MaxRecords = 18;
        public const int MaxTeams = 2;
        private const int ClanNameWireBytes = 34;   // wchar_t[17] at CLAN_MATCHING_TEAM_INFO+4

        private readonly List<MatchModel> Teams;

        public PROTOCOL_CLAN_WAR_MATCH_TEAM_INFO_ACK(params MatchModel[] teams)
        {
            this.Teams = new List<MatchModel>();
            if (teams == null)
                return;
            foreach (MatchModel team in teams)
            {
                if (team != null && this.Teams.Count < MaxTeams)
                    this.Teams.Add(team);
            }
        }

        public override void Write()
        {
            this.WriteH((short)6930);
            this.WriteH((short) 0);

            // MATCHMAKING_RECORD rows, one per occupied slot across both teams.
            // Only slotIndex/kills/deaths survive the client's copy at 0xD1F70B;
            // the +1/+5 dwords are read and then overwritten, so they are inert.
            List<SlotMatch> rows = new List<SlotMatch>();
            List<MatchModel> owners = new List<MatchModel>();
            foreach (MatchModel team in this.Teams)
            {
                for (int slot = 0; slot < team.Slots.Length && rows.Count < MaxRecords; slot++)
                {
                    if (team.Slots[slot].PlayerId <= 0L)
                        continue;
                    rows.Add(team.Slots[slot]);
                    owners.Add(team);
                }
            }
            this.WriteS2MOCount(rows.Count, MaxRecords);
            for (int i = 0; i < rows.Count; i++)
            {
                Account member = owners[i].GetPlayerBySlot(rows[i]);
                this.WriteC((byte)rows[i].Id);                                  // +0  i8  slot index
                this.WriteD(member == null ? 0 : member.Statistic.Clan.MatchWins);           // +1  u32 (inert)
                this.WriteD(member == null ? 0 : member.Statistic.Clan.MatchLoses);          // +5  u32 (inert)
                this.WriteD(member == null ? 0 : member.Statistic.Basic.KillsCount);          // +9  u32 kills
                this.WriteD(member == null ? 0 : member.Statistic.Basic.DeathsCount);         // +13 u32 deaths
                this.WriteD(0);                                                 // +17 u32 UNKNOWN
            }

            this.WriteS2MOCount(this.Teams.Count, MaxTeams);
            for (int i = 0; i < this.Teams.Count; i++)
            {
                ClanModel clan = this.Teams[i].Clan;
                this.WriteD(clan.Id);                       // +0x00 u32 clan id
                this.WriteU(clan.Name, ClanNameWireBytes);  // +0x04 wchar[17] clan name
                this.WriteH((ushort)0);                     // +0x26 u16 UNKNOWN
                this.WriteC((byte)0);                       // +0x28 u8  UNKNOWN
                this.WriteD(0);                             // +0x29 u32 UNKNOWN
                this.WriteD(clan.Logo);                     // +0x2D u32 clan mark id
                this.WriteH((ushort)0);                     // +0x31 u16 UNKNOWN
                this.WriteD(0);                             // +0x33 u32 UNKNOWN
                this.WriteH((ushort)1);                     // +0x37 u16 client default is 1
                this.WriteH((ushort)this.Teams[i].FriendId);// +0x39 u16 squad number
            }
        }
    }
}
