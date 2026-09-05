// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_MESSENGER_NOTE_SEND_REQ
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
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_MESSENGER_NOTE_SEND_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;
        private string Field2;
        private string Field3;
        private uint Field4;

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            this.Field1 = (int)this.ReadC();
            this.Field2 = this.ReadU(this.Field0 * 2);
            this.Field3 = this.ReadU(this.Field1 * 2);
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.Nickname.Length == 0 || player.Nickname == this.Field2)
                    return;

                if (player.IsBanned())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("Your account is banned."));
                    return;
                }
                if (player.IsMuted())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("You are muted and cannot send notes."));
                    return;
                }
                Account account = AccountManager.GetAccount(this.Field2, 1, 0);
                if (account != null)
                {
                    if (DaoManagerSQL.GetMessagesCount(account.PlayerId) >= 100)
                    {
                        this.Field4 = 2147487871U;
                    }
                    else
                    {
                        MessageModel A_1 = this.Method0(player.Nickname, account.PlayerId, this.Client.PlayerId);
                        if (A_1 != null)
                            account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                    }
                }
                else
                    this.Field4 = 2147487870U;
                this.Client.SendPacket(new PROTOCOL_MESSENGER_NOTE_SEND_ACK(this.Field4));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MESSENGER_NOTE_SEND_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private MessageModel Method0(string A_1, long A_2, long A_3)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = A_1,
                SenderId = A_3,
                Text = this.Field3,
                State = NoteMessageState.Unreaded
            };
            if (DaoManagerSQL.CreateMessage(A_2, Message))
                return Message;
            this.Field4 = 2147483648U /*0x80000000*/;
            return (MessageModel)null;
        }
    }
}