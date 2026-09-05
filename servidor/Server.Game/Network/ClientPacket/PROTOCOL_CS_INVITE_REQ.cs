// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_INVITE_REQ
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


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_INVITE_REQ : GameClientPacket
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
                if (player == null || player.ClanId == 0 || ComDiv.GetDuration(player.LastClanInvite) < 1.0 || player.FindPlayer == "" || player.FindPlayer.Length == 0)
                    return;
                Account account = AccountManager.GetAccount(player.FindPlayer, 1, 0);
                if (account == null)
                    this.Field0 = 2147483648U /*0x80000000*/;
                else if (account.ClanId == 0 && player.ClanId != 0)
                    this.Method0(account, player.ClanId);
                else
                    this.Field0 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_CS_INVITE_ACK(this.Field0));
                player.LastClanInvite = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        private void Method0(Account A_1, int A_2)
        {
            if (DaoManagerSQL.GetMessagesCount(A_1.PlayerId) >= 100)
            {
                this.Field0 = 2147483648U /*0x80000000*/;
            }
            else
            {
                MessageModel A_1_1 = this.Method1(A_2, A_1.PlayerId, this.Client.PlayerId);
                if (A_1_1 == null || !A_1.IsOnline)
                    return;
                A_1.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1_1), false);
            }
        }

        private MessageModel Method1(int A_1, long A_2, long A_3)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = ClanManager.GetClan(A_1).Name,
                ClanId = A_1,
                SenderId = A_3,
                Type = NoteMessageType.ClanAsk,
                State = NoteMessageState.Unreaded,
                ClanNote = NoteMessageClan.Invite
            };
            return !DaoManagerSQL.CreateMessage(A_2, Message) ? (MessageModel)null : Message;
        }
    }
}