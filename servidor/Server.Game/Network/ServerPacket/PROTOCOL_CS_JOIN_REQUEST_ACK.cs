// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_JOIN_REQUEST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_JOIN_REQUEST_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly uint Field1;

        public PROTOCOL_CS_JOIN_REQUEST_ACK(uint A_1, int A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)813);
            this.WriteD(this.Field1);
            // The client tests this status SIGNED at 0xEE8BCA (`if (v9 >= 0)`) and then re-reads
            // into the SAME local, so with an unsigned `!= 0U` bail a positive non-zero status
            // left the error code in place and the client consumed it as the id.
            if ((int)this.Field1 < 0)
                return;
            this.WriteD(this.Field0);
        }
    }
}