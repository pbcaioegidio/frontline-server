using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C3): opcode 6940 is a bare
    // S2MOPacketBaseT<0x1B1C> with a null field-list head -> zero payload, so this
    // is always a self-removal. (The 119 doc's "1.ushort" came from the shared
    // empty-ctor address and is noise.) Reply 6941 is a bare u32 result.
    public class PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_REQ : GameClientPacket
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
                if (channel == null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                bool removed;
                lock (channel.Mercenaries)
                    removed = channel.Mercenaries.Remove(player);
                player.PendingMercenaryInvite = null;
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_ACK(removed ? 0U : 2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
