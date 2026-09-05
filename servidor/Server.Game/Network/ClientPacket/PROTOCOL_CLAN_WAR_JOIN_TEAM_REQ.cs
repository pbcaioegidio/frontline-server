using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, ctor 0xD12BFD): PACKET_CLAN_WAR_JOIN_TEAM_REQ (6920)
    // registers exactly one S2MOValue<ushort,1> -> the payload is a single u16,
    // the match_id taken from MATCH_TEAM_INFO+0 of the clicked team-list row
    // (0xAA0B21 UIPhaseClanWarfareLobby__HandleTeamListClick).
    // The 068 reader consumed u16,u16,u8 and treated the extra fields as a target
    // channel plus a same-clan flag; neither exists on the 122 wire.
    public class PROTOCOL_CLAN_WAR_JOIN_TEAM_REQ : GameClientPacket
    {
        private int MatchId;

        public override void Read() => this.MatchId = (int)this.ReadH();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.Match != null || player.Room != null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                if (player.ClanId == 0)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147487835U));
                    return;
                }
                ChannelModel channel = player.GetChannel();
                if (channel == null || channel.Type != ChannelType.Clan)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                // The wire only carries a match id, so the team is always looked up in
                // the player's own channel — that is the only channel whose team list
                // the client can be showing.
                MatchModel match = channel.GetMatch(this.MatchId);
                if (match == null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                // A different clan's team is only joinable by a registered mercenary.
                if (match.Clan.Id != player.ClanId)
                {
                    bool isMercenary;
                    lock (channel.Mercenaries)
                        isMercenary = channel.Mercenaries.Contains(player);
                    if (!isMercenary)
                    {
                        this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147483648U /*0x80000000*/));
                        return;
                    }
                }
                if (!match.AddPlayer(player))
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                lock (channel.Mercenaries)
                    channel.Mercenaries.Remove(player);
                match.BroadcastRoster();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
