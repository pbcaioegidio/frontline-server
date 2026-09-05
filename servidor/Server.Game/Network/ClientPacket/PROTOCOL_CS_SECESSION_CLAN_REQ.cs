// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_SECESSION_CLAN_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_SECESSION_CLAN_REQ : GameClientPacket
    {
        private uint Field0;

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
                if (player.ClanId > 0)
                {
                    ClanModel clan = ClanManager.GetClan(player.ClanId);
                    if (clan.Id > 0 && clan.OwnerId != player.PlayerId)
                    {
                        if (ComDiv.UpdateDB("accounts", "player_id", (object)player.PlayerId, new string[2]
                        {
            "clan_id",
            "clan_access"
                        }, (object)0, (object)0))
                        {
                            if (ComDiv.UpdateDB("player_stat_clans", "owner_id", (object)player.PlayerId, new string[2]
                            {
              "clan_matches",
              "clan_match_wins"
                            }, (object)0, (object)0))
                            {
                                using (PROTOCOL_CS_MEMBER_INFO_DELETE_ACK Packet = new PROTOCOL_CS_MEMBER_INFO_DELETE_ACK(player.PlayerId))
                                    ClanManager.SendPacket(Packet, player.ClanId, player.PlayerId, true, true);
                                long ownerId = clan.OwnerId;
                                if (DaoManagerSQL.GetMessagesCount(ownerId) < 100)
                                {
                                    MessageModel A_1 = this.Method0(clan, player);
                                    if (A_1 != null)
                                    {
                                        Account account = AccountManager.GetAccount(ownerId, 31 /*0x1F*/);
                                        if (account != null && account.IsOnline)
                                            account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                                    }
                                }
                                player.ClanId = 0;
                                player.ClanAccess = 0;
                                goto label_18;
                            }
                        }
                        this.Field0 = 2147487851U;
                    }
                    else
                        this.Field0 = 2147487838U;
                }
                else
                    this.Field0 = 2147487835U;
                label_18:
                this.Client.SendPacket(new PROTOCOL_CS_SECESSION_CLAN_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_SECESSION_CLAN_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private MessageModel Method0(ClanModel A_1, Account A_2)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = A_1.Name,
                SenderId = A_2.PlayerId,
                ClanId = A_1.Id,
                Type = NoteMessageType.Clan,
                Text = A_2.Nickname,
                State = NoteMessageState.Unreaded,
                ClanNote = NoteMessageClan.Secession
            };
            return !DaoManagerSQL.CreateMessage(A_1.OwnerId, Message) ? (MessageModel)null : Message;
        }
    }
}