// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_MEMBER_CONTEXT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_CONTEXT_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly int Field1;

        public PROTOCOL_CS_MEMBER_CONTEXT_ACK(int A_1, int A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public PROTOCOL_CS_MEMBER_CONTEXT_ACK(int A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)803);
            this.WriteD(this.Field0);
            if (this.Field0 != 0)
                return;
            this.WriteC((byte)this.Field1);
            this.WriteC((byte)14);
            this.WriteC((byte)Math.Ceiling((double)this.Field1 / 14.0));
            this.WriteD(uint.Parse(DateTimeUtil.Now("MMddHHmmss")));
        }
    }
}