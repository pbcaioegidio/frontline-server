using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C1): opcode 6936 is a bare
    // S2MOPacketBaseT<0x1B18>, zero payload. Reply is MERCENARY_LIST_ACK (6937).
    public class PROTOCOL_CLAN_WAR_MERCENARY_LIST_REQ : GameClientPacket
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
                if (channel == null || channel.Type != ChannelType.Clan)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MERCENARY_LIST_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                List<Account> board;
                lock (channel.Mercenaries)
                    board = new List<Account>(channel.Mercenaries);
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MERCENARY_LIST_ACK(board));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_MERCENARY_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
