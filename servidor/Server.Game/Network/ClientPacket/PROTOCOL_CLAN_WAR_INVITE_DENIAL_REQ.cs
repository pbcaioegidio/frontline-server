using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C7): opcode 6948 is a bare
    // S2MOPacketBaseT<0x1B24>, zero payload. (The 119 doc lists eight fields, but
    // that entry points at the shared empty ctor and is noise.)
    // The refusing mercenary sends this; the inviting leader receives 6949 with the
    // board row id so it can grey the row out.
    public class PROTOCOL_CLAN_WAR_INVITE_DENIAL_REQ : GameClientPacket
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
                if (match == null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_DENIAL_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                ChannelModel channel = player.GetChannel();
                int listId = -1;
                if (channel != null)
                {
                    lock (channel.Mercenaries)
                        listId = channel.Mercenaries.IndexOf(player);
                }
                Account leader = match.GetLeader();
                if (leader != null && leader.IsOnline)
                    leader.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_DENIAL_ACK(0U, listId < 0 ? 0 : listId));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_INVITE_DENIAL_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
