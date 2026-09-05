// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_MEMBER_LIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_LIST_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly byte[] Field1;
        private readonly byte Field2;
        private readonly byte Field3;

        public PROTOCOL_CS_MEMBER_LIST_ACK(uint A_1, byte A_2, byte A_3, byte[] A_4)
        {
            this.Field0 = A_1;
            this.Field2 = A_2;
            this.Field3 = A_3;
            this.Field1 = A_4;
        }

        public override void Write()
        {
            this.WriteH((short)805);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteC(this.Field2);
            this.WriteC(this.Field3);
            this.WriteB(this.Field1);
        }
    }
}