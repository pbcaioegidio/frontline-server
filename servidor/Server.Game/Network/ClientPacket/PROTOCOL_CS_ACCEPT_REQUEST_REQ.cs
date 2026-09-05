// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_ACCEPT_REQUEST_REQ
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
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_ACCEPT_REQUEST_REQ : GameClientPacket
    {
        private List<long> Field0 = new List<long>();
        private int Field1;

        public override void Read()
        {
            int num = (int)this.ReadC();
            for (int index = 0; index < num; ++index)
                this.Field0.Add(this.ReadQ());
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                if (clan.Id > 0 && (player.ClanAccess >= 1 && player.ClanAccess <= 2 || player.PlayerId == clan.OwnerId))
                {
                    List<Account> clanPlayers = ClanManager.GetClanPlayers(clan.Id, -1L, true);
                    if (clanPlayers.Count >= clan.MaxPlayers)
                    {
                        this.Field1 = -1;
                        return;
                    }
                    for (int index = 0; index < this.Field0.Count; ++index)
                    {
                        Account account = AccountManager.GetAccount(this.Field0[index], 31 /*0x1F*/);
                        if (account != null && clanPlayers.Count < clan.MaxPlayers && account.ClanId == 0 && DaoManagerSQL.GetRequestClanId(account.PlayerId) > 0)
                        {
                            using (PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK Packet = new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(account))
                                ClanManager.SendPacket(Packet, clanPlayers);
                            account.ClanId = player.ClanId;
                            account.ClanDate = uint.Parse(DateTimeUtil.Now("yyyyMMdd"));
                            account.ClanAccess = 3;
                            SendClanInfo.Load(account, (Account)null, 3);
                            ComDiv.UpdateDB("accounts", "player_id", (object)account.PlayerId, new string[3]
                            {
              "clan_access",
              "clan_id",
              "clan_date"
                            }, (object)account.ClanAccess, (object)account.ClanId, (object)(long)account.ClanDate);
                            DaoManagerSQL.DeleteClanInviteDB(player.ClanId, account.PlayerId);
                            if (account.IsOnline)
                            {
                                account.SendPacket(new PROTOCOL_CS_MEMBER_INFO_ACK(clanPlayers), false);
                                account.Room?.SendPacketToPlayers(new PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(account, clan));
                                account.SendPacket(new PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK(clan, clanPlayers.Count + 1), false);
                            }
                            if (DaoManagerSQL.GetMessagesCount(account.PlayerId) < 100)
                            {
                                MessageModel A_1 = this.Method0(clan, account.PlayerId, this.Client.PlayerId);
                                if (A_1 != null && account.IsOnline)
                                    account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                            }
                            ++this.Field1;
                            clanPlayers.Add(account);
                        }
                    }
                }
                else
                    this.Field1 = -1;
                this.Client.SendPacket(new PROTOCOL_CS_ACCEPT_REQUEST_ACK((uint)this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_ACCEPT_REQUEST_RESULT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private MessageModel Method0(ClanModel A_1, long A_2, long A_3)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = A_1.Name,
                SenderId = A_3,
                ClanId = A_1.Id,
                Type = NoteMessageType.Clan,
                State = NoteMessageState.Unreaded,
                ClanNote = NoteMessageClan.InviteAccept
            };
            return DaoManagerSQL.CreateMessage(A_2, Message) ? Message : (MessageModel)null;
        }
    }
}