// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CLAN_LIST_FILTER_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CLAN_LIST_FILTER_ACK : GameServerPacket
    {
        private readonly byte Field0;
        private readonly int Field1;
        private readonly int Field2;
        private readonly byte[] Field3;

        public PROTOCOL_CS_CLAN_LIST_FILTER_ACK(byte A_1, int A_2, int A_3, byte[] A_4)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
            this.Field3 = A_4;
        }

        public override void Write()
        {
            this.WriteH((short)1000);
            this.WriteD(0);
            this.WriteH((ushort)this.Field1);
            this.WriteH((ushort)this.Field2);
            this.WriteH((ushort)this.Field0);
            this.WriteB(this.Field3);
        }
    }
}
