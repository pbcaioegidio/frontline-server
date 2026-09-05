// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_CHATTING_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CHATTING_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly int Field1;
        private readonly int Field2;
        private readonly bool Field3;

        public PROTOCOL_ROOM_CHATTING_ACK(int A_1, int A_2, bool A_3, string A_4)
        {
            this.Field1 = A_1;
            this.Field2 = A_2;
            this.Field3 = A_3;
            this.Field0 = A_4;
        }

        
        public override void Write()
        {
            this.WriteH((short)3606);
            this.WriteH((short)this.Field1);
            this.WriteD(this.Field2);
            this.WriteC(this.Field3 ? (byte)1 : (byte)0);
            this.WriteD(this.Field0.Length + 1);
            this.WriteN(this.Field0, this.Field0.Length + 2, "UTF-16LE");
        }
    }
}