using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82BD, ctor 0xD12AFD): opcode
    // 6926 registers one S2MOValue<uchar,1> -> a single byte, the team's new max
    // player count. Reply is CHANGE_MAX_PER_ACK (6927) = u32 result + that byte,
    // which the client writes straight into its own team header slot (0x9C63B6,
    // the same field MATCH_TEAM_INFO+4 feeds).
    public class PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_REQ : GameClientPacket
    {
        private int MaxPlayers;

        public override void Read() => this.MaxPlayers = (int)this.ReadC();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                MatchModel match = player.Match;
                if (match == null || player.MatchSlot != match.Leader || match.State != MatchState.Ready || match.InQueue)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                // The team header only has 8 member slots on the wire
                // (S2MOValue<MATCH_MEMBER_INFO,8> in JOIN_TEAM_ACK), and the seat
                // array is 9 long, so never let the cap exceed 8.
                if (this.MaxPlayers < 1 || this.MaxPlayers > PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK.MaxMembers || this.MaxPlayers < match.GetCountPlayers())
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                match.Training = this.MaxPlayers;
                using (PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK Packet = new PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK(0U, (byte)this.MaxPlayers))
                    match.SendPacketToPlayers(Packet);
                match.BroadcastRoster();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
