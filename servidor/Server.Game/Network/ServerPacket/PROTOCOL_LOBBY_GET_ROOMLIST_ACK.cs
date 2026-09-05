// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_LOBBY_GET_ROOMLIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_LOBBY_GET_ROOMLIST_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly int Field1;
        private readonly int Field2;
        private readonly int Field3;
        private readonly int Field4;
        private readonly int Field5;
        private readonly byte[] Field6;
        private readonly byte[] Field7;

        public PROTOCOL_LOBBY_GET_ROOMLIST_ACK(
          int A_1,
          int A_2,
          int A_3,
          int A_4,
          int A_5,
          int A_6,
          byte[] A_7,
          byte[] A_8)
        {
            this.Field3 = A_1;
            this.Field2 = A_2;
            this.Field0 = A_3;
            this.Field1 = A_4;
            this.Field6 = A_7;
            this.Field7 = A_8;
            this.Field4 = A_5;
            this.Field5 = A_6;
        }

        public override void Write()
        {
            this.WriteH((short)2588);
            this.WriteD(this.Field3);
            this.WriteD(this.Field0);
            this.WriteD(this.Field4);
            this.WriteB(this.Field6);
            this.WriteD(this.Field2);
            this.WriteD(this.Field1);
            this.WriteD(this.Field5);
            this.WriteB(this.Field7);
        }
    }
}