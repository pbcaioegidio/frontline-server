// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_COMMISSION_MASTER_REQ
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
    public class PROTOCOL_CS_COMMISSION_MASTER_REQ : GameClientPacket
    {
        private long Field0;
        private uint Field1;

        public override void Read() => this.Field0 = this.ReadQ();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.ClanAccess != 1)
                    return;
                Account account = AccountManager.GetAccount(this.Field0, 31 /*0x1F*/);
                int clanId = player.ClanId;
                if (account != null && account.ClanId == clanId)
                {
                    if (account.Rank <= 10)
                    {
                        this.Field1 = 2147487928U;
                    }
                    else
                    {
                        ClanModel clan = ClanManager.GetClan(clanId);
                        if (clan.Id > 0 && clan.OwnerId == this.Client.PlayerId && account.ClanAccess == 2 && ComDiv.UpdateDB("system_clan", "owner_id", (object)this.Field0, "id", (object)clanId) && ComDiv.UpdateDB("accounts", "clan_access", (object)1, "player_id", (object)this.Field0) && ComDiv.UpdateDB("accounts", "clan_access", (object)2, "player_id", (object)player.PlayerId))
                        {
                            account.ClanAccess = 1;
                            player.ClanAccess = 2;
                            clan.OwnerId = this.Field0;
                            if (DaoManagerSQL.GetMessagesCount(account.PlayerId) < 100)
                            {
                                MessageModel A_1 = this.Method0(clan, account.PlayerId, player.PlayerId);
                                if (A_1 != null && account.IsOnline)
                                    account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                            }
                            if (account.IsOnline)
                                account.SendPacket(new PROTOCOL_CS_COMMISSION_MASTER_RESULT_ACK(), false);
                        }
                        else
                            this.Field1 = 2147487744U /*0x80001000*/;
                    }
                }
                else
                    this.Field1 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_CS_COMMISSION_MASTER_ACK(this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_COMMISSION_MASTER_REQ: " + ex.Message, LoggerType.Error, ex);
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
                ClanNote = NoteMessageClan.Master
            };
            return !DaoManagerSQL.CreateMessage(A_2, Message) ? (MessageModel)null : Message;
        }
    }
}