using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82BF): opcode 6934 is a bare
    // S2MOPacketBaseT<0x1B16> with no field nodes -> zero payload. The reply is
    // 6935, dispatched at 0xEF11C7, which parses a u32 result.
    //
    // The old body acknowledged unconditionally and never touched any queue, so a
    // cancelled team stayed pairable forever.
    public class PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_REQ : GameClientPacket
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
                MatchModel match = player.Match;
                ChannelModel channel = player.GetChannel();
                if (match == null || channel == null || player.MatchSlot != match.Leader || !match.InQueue)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                match.LeaveQueue(channel);
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
