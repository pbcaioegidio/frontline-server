// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_JACKPOT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_JACKPOT_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly int Field1;
        private readonly int Field2;

        public PROTOCOL_AUTH_SHOP_JACKPOT_ACK(string A_1, int A_2, int A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        
        public override void Write()
        {
            this.WriteH((short)1073);
            this.WriteH((short)0);
            this.WriteC((byte)this.Field2);
            this.WriteD(this.Field1);
            this.WriteC((byte)this.Field0.Length);
            this.WriteN(this.Field0, this.Field0.Length, "UTF-16LE");
        }
    }
}
