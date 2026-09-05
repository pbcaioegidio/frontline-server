// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_USE_ITEM_CHECK_NICK_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_USE_ITEM_CHECK_NICK_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadU(66);

        public override void Run()
        {
            try
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_USE_ITEM_CHECK_NICK_ACK(!DaoManagerSQL.IsPlayerNameExist(this.Field0) ? 0U : 2147483923U));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}