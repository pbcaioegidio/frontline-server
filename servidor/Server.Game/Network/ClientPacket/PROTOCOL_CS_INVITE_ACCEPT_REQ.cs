// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_INVITE_ACCEPT_REQ
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_INVITE_ACCEPT_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field1 = (int)this.ReadC();
        }

        
        public override void Run()
        {
            Account player = this.Client.GetAccount();
            if (player == null || player.Nickname.Length == 0)
                return;
            ClanModel clan = ClanManager.GetClan(this.Field0);
            List<Account> clanPlayers = ClanManager.GetClanPlayers(this.Field0, -1L, true);
            if (clan.Id != 0)
            {
                if (player.ClanId <= 0)
                {
                    if (clan.MaxPlayers > clanPlayers.Count)
                    {
                        if (this.Field1 != 0 && this.Field1 != 1)
                            return;
                        try
                        {
                            uint A_1_1 = 0;
                            Account account = AccountManager.GetAccount(clan.OwnerId, 31 /*0x1F*/);
                            if (account != null)
                            {
                                if (DaoManagerSQL.GetMessagesCount(clan.OwnerId) < 100)
                                {
                                    MessageModel A_1_2 = this.Method0(clan, player.Nickname, this.Client.PlayerId);
                                    if (A_1_2 != null && account.IsOnline)
                                        account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1_2), false);
                                }
                                if (this.Field1 == 1)
                                {
                                    uint num = uint.Parse(DateTimeUtil.Now("yyyyMMdd"));
                                    if (ComDiv.UpdateDB("accounts", "player_id", (object)player.PlayerId, new string[3]
                                    {
                  "clan_id",
                  "clan_access",
                  "clan_date"
                                    }, (object)clan.Id, (object)3, (object)(long)num))
                                    {
                                        using (PROTOCOL_CS_MEMBER_INFO_INSERT_ACK Packet = new PROTOCOL_CS_MEMBER_INFO_INSERT_ACK(player))
                                            ClanManager.SendPacket(Packet, clanPlayers);
                                        player.ClanId = clan.Id;
                                        player.ClanDate = num;
                                        player.ClanAccess = 3;
                                        this.Client.SendPacket(new PROTOCOL_CS_MEMBER_INFO_ACK(clanPlayers));
                                        player.Room?.SendPacketToPlayers(new PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(player, clan));
                                        this.Client.SendPacket(new PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK(clan, account, clanPlayers.Count + 1));
                                    }
                                    else
                                        A_1_1 = 2147483648U /*0x80000000*/;
                                }
                            }
                            else
                                A_1_1 = 2147483648U /*0x80000000*/;
                            this.Client.SendPacket(new PROTOCOL_MESSENGER_NOTE_SEND_ACK(A_1_1));
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print(ex.Message, LoggerType.Error, ex);
                        }
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_CS_INVITE_ACCEPT_ACK(2147487830U));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_CS_INVITE_ACCEPT_ACK(2147487832U));
            }
            else
                this.Client.SendPacket(new PROTOCOL_CS_INVITE_ACCEPT_ACK(2147487835U));
        }

        private MessageModel Method0(ClanModel A_1, string A_2, long A_3)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = A_1.Name,
                SenderId = A_3,
                ClanId = A_1.Id,
                Type = NoteMessageType.Clan,
                Text = A_2,
                State = NoteMessageState.Unreaded,
                ClanNote = this.Field1 == 0 ? NoteMessageClan.JoinDenial : NoteMessageClan.JoinAccept
            };
            return !DaoManagerSQL.CreateMessage(A_1.OwnerId, Message) ? (MessageModel)null : Message;
        }
    }
}