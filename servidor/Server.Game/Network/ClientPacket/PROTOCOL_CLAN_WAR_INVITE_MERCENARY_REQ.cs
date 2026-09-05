using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82C4, ctor 0xD12BBD): opcode
    // 6942 registers one S2MOValue<ushort,1> -> a single u16, the
    // mercenary_list_id from MERCENARY_INFO+0 of the clicked board row (lookup at
    // 0x9C5798). The team leader gets INVITE_MERCENARY_SENDER_ACK (6943) and the
    // target gets INVITE_MERCENARY_RECEIVER_ACK (6944).
    public class PROTOCOL_CLAN_WAR_INVITE_MERCENARY_REQ : GameClientPacket
    {
        private int MercenaryListId;

        public override void Read() => this.MercenaryListId = (int)this.ReadH();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel channel = player.GetChannel();
                MatchModel match = player.Match;
                if (channel == null || channel.Type != ChannelType.Clan || match == null || player.MatchSlot != match.Leader)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                if (match.State != MatchState.Ready || match.GetCountPlayers() >= match.Training)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK(2147487889U));
                    return;
                }
                Account target;
                lock (channel.Mercenaries)
                {
                    target = this.MercenaryListId >= 0 && this.MercenaryListId < channel.Mercenaries.Count
                        ? channel.Mercenaries[this.MercenaryListId]
                        : null;
                }
                if (target == null || target.Match != null || target.Room != null || !target.IsOnline)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                target.PendingMercenaryInvite = match;
                target.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_RECEIVER_ACK(
                    0U, ClanManager.GetClan(match.Clan.Id), player.Nickname));
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_INVITE_MERCENARY_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
