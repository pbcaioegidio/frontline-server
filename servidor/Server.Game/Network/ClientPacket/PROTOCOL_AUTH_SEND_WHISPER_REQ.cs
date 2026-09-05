// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SEND_WHISPER_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SEND_WHISPER_REQ : GameClientPacket
    {
        private long Field0;
        private string Field1;
        private string Field2;

        public override void Read()
        {
            this.Field0 = this.ReadQ();
            this.Field1 = this.ReadU(66);
            this.Field2 = this.ReadU((int)this.ReadH() * 2);
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.Nickname == this.Field1 || ComDiv.GetDuration(player.LastChatting) < 1.0)
                    return;

                if (player.IsBanned())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("Your account is banned."));
                    return;
                }
                if (player.IsMuted())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("You are muted and cannot send whispers."));
                    return;
                }
                Account account = AccountManager.GetAccount(this.Field0, 31 /*0x1F*/);
                if (account != null && account.IsOnline)
                    account.SendPacket(new PROTOCOL_AUTH_RECV_WHISPER_ACK(player.Nickname, this.Field2, player.UseChatGM()), false);
                else
                    this.Client.SendPacket(new PROTOCOL_AUTH_SEND_WHISPER_ACK(this.Field1, this.Field2, 2147483648U /*0x80000000*/));
                player.LastChatting = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}