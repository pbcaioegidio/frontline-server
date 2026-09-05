using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82B8): opcode 6916 is allocated
    // as a plain 10-byte S2MOPacketBase (vftable 0x121505C) whose write returns
    // immediately -> header only, zero payload. This is the only clan-war list REQ
    // the 122 UI still sends; there is no 6914 (MATCH_TEAM_COUNT_REQ) prototype
    // anywhere in the client, so the count ACK rides along with the list.
    public class PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel channel = player.GetChannel();
                // No `player.Match != null` guard: a mercenary with no team browses
                // this same list before joining one.
                if (channel == null || channel.Type != ChannelType.Clan)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                List<MatchModel> teams;
                lock (channel.Matches)
                    teams = new List<MatchModel>(channel.Matches);
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCH_TEAM_COUNT_ACK(teams.Count));
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_ACK(teams));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_MATCH_TEAM_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
