using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C2): opcode 6938 is a bare
    // S2MOPacketBaseT<0x1B1A>, zero payload. Reply 6939 is a bare u32 result.
    // Puts the player on the channel's mercenary board so other clans' team
    // leaders can invite them (6942) and so a cross-clan JOIN_TEAM (6920) passes.
    public class PROTOCOL_CLAN_WAR_REGIST_MERCENARY_REQ : GameClientPacket
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
                if (channel == null || channel.Type != ChannelType.Clan || player.ClanId == 0 || player.Room != null || player.Match != null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_REGIST_MERCENARY_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                lock (channel.Mercenaries)
                {
                    if (channel.Mercenaries.Contains(player))
                    {
                        this.Client.SendPacket(new PROTOCOL_CLAN_WAR_REGIST_MERCENARY_ACK(2147483648U /*0x80000000*/));
                        return;
                    }
                    channel.Mercenaries.Add(player);
                }
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_REGIST_MERCENARY_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_REGIST_MERCENARY_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
