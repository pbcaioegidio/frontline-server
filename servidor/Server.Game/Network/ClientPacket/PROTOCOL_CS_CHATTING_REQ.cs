// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CHATTING_REQ
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
    public class PROTOCOL_CS_CHATTING_REQ : GameClientPacket
    {
        private ChattingType Field0;
        private string Field1;

        public override void Read()
        {
            this.Field0 = (ChattingType)this.ReadH();
            this.Field1 = this.ReadU((int)this.ReadH() * 2);
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastChatting) < 1.0)
                    return;

                if (player.IsBanned())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("Your account is banned."));
                    return;
                }
                if (player.IsMuted())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("You are muted and cannot use clan chat."));
                    return;
                }
                int length = this.Field1.Length;
                int Exception = -1;
                bool IsOnline = true;
                bool UseCache = true;
                if (length <= 60 && this.Field0 == ChattingType.Clan)
                {
                    using (PROTOCOL_CS_CHATTING_ACK Packet = new PROTOCOL_CS_CHATTING_ACK(this.Field1, player))
                        ClanManager.SendPacket(Packet, player.ClanId, (long)Exception, UseCache, IsOnline);
                }
                player.LastChatting = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}