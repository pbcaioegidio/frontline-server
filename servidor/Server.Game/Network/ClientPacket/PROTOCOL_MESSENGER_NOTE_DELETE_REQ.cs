// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_MESSENGER_NOTE_DELETE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_MESSENGER_NOTE_DELETE_REQ : GameClientPacket
    {
        private uint Field0;
        private List<object> Field1 = new List<object>();

        public override void Read()
        {
            int num = (int)this.ReadC();
            for (int index = 0; index < num; ++index)
                this.Field1.Add((object)(long)this.ReadUD());
        }

        
        public override void Run()
        {
            try
            {
                if (!DaoManagerSQL.DeleteMessages(this.Field1, this.Client.PlayerId))
                    this.Field0 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_MESSENGER_NOTE_DELETE_ACK(this.Field0, this.Field1));
                this.Field1 = (List<object>)null;
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MESSENGER_NOTE_DELETE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
