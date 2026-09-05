using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C6): opcode 6946 is a bare
    // S2MOPacketBaseT<0x1B22>, zero payload. Reply INVITE_ACCEPT_ACK (6947) is a
    // bare u32 result.
    //
    // Semantics change vs 068: in the 122 protocol 0x1B1E..0x1B25 is the MERCENARY
    // invite block (SENDER/RECEIVER/ACCEPT/DENIAL), so this is "the mercenary
    // accepts the team's invitation", not "the leader accepts a battle". The old
    // body pushed ENEMY_INFO (1574) and CREATE_ROOM (1564), neither of which the
    // 122 client parses; battle pairing now lives in MATCHMAKING_REQ (6932).
    public class PROTOCOL_CLAN_WAR_INVITE_ACCEPT_REQ : GameClientPacket
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
                MatchModel match = player.PendingMercenaryInvite;
                player.PendingMercenaryInvite = null;
                if (match == null || player.Match != null || player.Room != null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_ACCEPT_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                if (match.State != MatchState.Ready || match.GetCountPlayers() >= match.Training)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_ACCEPT_ACK(2147487889U));
                    return;
                }
                if (!match.AddPlayer(player))
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_ACCEPT_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                ChannelModel channel = player.GetChannel();
                if (channel != null)
                {
                    lock (channel.Mercenaries)
                        channel.Mercenaries.Remove(player);
                }
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_ACCEPT_ACK(0U));
                match.BroadcastRoster();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_INVITE_ACCEPT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
