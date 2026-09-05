// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_ACK : GameServerPacket
    {
        private readonly int Field0;

        public PROTOCOL_CS_CLAN_MATCH_RESULT_CONTEXT_ACK(int A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)1955);
            this.WriteC((byte)this.Field0);
            this.WriteC((byte)13);
            this.WriteC((byte)Math.Ceiling((double)this.Field0 / 13.0));
        }
    }
}