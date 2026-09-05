// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_GET_RECORD_INFO_DB_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_RECORD_INFO_DB_REQ : GameClientPacket
    {
        private long Field0;

        public override void Read() => this.Field0 = this.ReadQ();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                Account account = AccountManager.GetAccount(this.Field0, 31 /*0x1F*/);
                if (account == null || player.PlayerId == account.PlayerId)
                    return;
                this.Client.SendPacket(new PROTOCOL_BASE_GET_RECORD_INFO_DB_ACK(account));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
