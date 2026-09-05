// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_GMCHAT_END_CHAT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GMCHAT_END_CHAT_REQ : GameClientPacket
    {
        private long Field0;

        public override void Read() => this.Field0 = this.ReadQ();

        
        public override void Run()
        {
            try
            {
                if (this.Client.GetAccount() == null)
                    return;
                Account account = AccountManager.GetAccount(this.Field0, 31 /*0x1F*/);
                if (account == null)
                    this.Client.SendPacket(new PROTOCOL_GMCHAT_END_CHAT_ACK(2147483648U /*0x80000000*/, (Account)null));
                else
                    this.Client.SendPacket(new PROTOCOL_GMCHAT_END_CHAT_ACK(0U, account));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_GMCHAT_START_CHAT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}