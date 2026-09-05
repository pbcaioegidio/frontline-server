// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_GMCHAT_START_CHAT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GMCHAT_START_CHAT_REQ : GameClientPacket
    {
        private string Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = this.ReadU((int)this.ReadC() * 2);
            this.Field1 = this.ReadD();
            int num = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player1 = this.Client.GetAccount();
                if (player1 == null)
                    return;
                PlayerSession player2 = player1.GetChannel().GetPlayer(this.Field1);
                if (player2 != null && player1.Session != player2)
                {
                    Account account = AccountManager.GetAccount(this.Field0, 1, 31 /*0x1F*/);
                    if (account != null && account.Session == player2)
                        this.Client.SendPacket(new PROTOCOL_GMCHAT_START_CHAT_ACK(0U, account));
                    else
                        this.Client.SendPacket(new PROTOCOL_GMCHAT_START_CHAT_ACK(2147483648U /*0x80000000*/, (Account)null));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_GMCHAT_START_CHAT_ACK(2147483648U /*0x80000000*/, (Account)null));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_GMCHAT_START_CHAT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}