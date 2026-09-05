// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CANCEL_REQUEST_REQ
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
    public class PROTOCOL_CS_CANCEL_REQUEST_REQ : GameClientPacket
    {
        private uint Field0;

        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                if (this.Client.GetAccount() == null || !DaoManagerSQL.DeleteClanInviteDB(this.Client.PlayerId))
                    this.Field0 = 2147487835U;
                this.Client.SendPacket(new PROTOCOL_CS_CANCEL_REQUEST_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}