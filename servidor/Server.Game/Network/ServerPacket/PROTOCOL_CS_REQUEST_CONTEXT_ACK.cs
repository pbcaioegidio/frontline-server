// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_REQUEST_CONTEXT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_REQUEST_CONTEXT_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly int Field1;

        public PROTOCOL_CS_REQUEST_CONTEXT_ACK(int A_1)
        {
            if (A_1 > 0)
                this.Field1 = DaoManagerSQL.GetRequestClanInviteCount(A_1);
            else
                this.Field0 = uint.MaxValue;
        }

        
        public override void Write()
        {
            this.WriteH((short)817);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteC((byte)this.Field1);
            this.WriteC((byte)13);
            this.WriteC((byte)Math.Ceiling((double)this.Field1 / 13.0));
            this.WriteD(uint.Parse(DateTimeUtil.Now("MMddHHmmss")));
        }
    }
}