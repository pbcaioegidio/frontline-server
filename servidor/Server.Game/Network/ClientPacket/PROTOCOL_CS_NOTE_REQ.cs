// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_NOTE_REQ
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
    public class PROTOCOL_CS_NOTE_REQ : GameClientPacket
    {
        private int Field0;
        private string Field1;

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            this.Field1 = this.ReadU((int)this.ReadC() * 2);
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (this.Field1.Length > 120 || player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                int A_1_1 = 0;
                if (clan.Id > 0 && clan.OwnerId == this.Client.PlayerId)
                {
                    List<Account> clanPlayers = ClanManager.GetClanPlayers(clan.Id, this.Client.PlayerId, true);
                    for (int index = 0; index < clanPlayers.Count; ++index)
                    {
                        Account account = clanPlayers[index];
                        if ((this.Field0 == 0 || account.ClanAccess == 2 && this.Field0 == 1 || account.ClanAccess == 3 && this.Field0 == 2) && DaoManagerSQL.GetMessagesCount(account.PlayerId) < 100)
                        {
                            ++A_1_1;
                            MessageModel A_1_2 = this.Method0(clan, account.PlayerId, this.Client.PlayerId);
                            if (A_1_2 != null && account.IsOnline)
                                account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1_2), false);
                        }
                    }
                }
                this.Client.SendPacket(new PROTOCOL_CS_NOTE_ACK(A_1_1));
                if (A_1_1 <= 0)
                    return;
                this.Client.SendPacket(new PROTOCOL_MESSENGER_NOTE_SEND_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_NOTE_REQ: " + ex.Message, LoggerType.Error, ex);
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
                Text = this.Field1,
                State = NoteMessageState.Unreaded
            };
            return !DaoManagerSQL.CreateMessage(A_2, Message) ? (MessageModel)null : Message;
        }
    }
}