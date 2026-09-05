// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_DENIAL_REQUEST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_DENIAL_REQUEST_REQ : GameClientPacket
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
                if (clan.Id > 0 && (player.ClanAccess >= 1 && player.ClanAccess <= 2 || clan.OwnerId == player.PlayerId))
                {
                    for (int index = 0; index < this.Field0.Count; ++index)
                    {
                        long num = this.Field0[index];
                        if (DaoManagerSQL.DeleteClanInviteDB(clan.Id, num))
                        {
                            if (DaoManagerSQL.GetMessagesCount(num) < 100)
                            {
                                MessageModel A_1 = this.Method0(clan, num, player.PlayerId);
                                if (A_1 != null)
                                {
                                    Account account = AccountManager.GetAccount(num, 31 /*0x1F*/);
                                    if (account != null && account.IsOnline)
                                        account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                                }
                            }
                            ++this.Field1;
                        }
                    }
                }
                this.Client.SendPacket(new PROTOCOL_CS_DENIAL_REQUEST_ACK(this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_DENIAL_REQUEST_REQ: " + ex.Message, LoggerType.Error, ex);
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
                ClanNote = NoteMessageClan.InviteDenial
            };
            return !DaoManagerSQL.CreateMessage(A_2, Message) ? (MessageModel)null : Message;
        }
    }
}