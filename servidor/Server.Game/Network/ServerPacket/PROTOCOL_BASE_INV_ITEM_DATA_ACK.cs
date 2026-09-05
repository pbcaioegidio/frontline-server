// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_INV_ITEM_DATA_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_INV_ITEM_DATA_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly Account Field1;

        public PROTOCOL_BASE_INV_ITEM_DATA_ACK(int A_1, Account A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)2395);
            this.WriteC((byte)this.Field0);
            this.WriteC((byte)this.Field1.NickColor);
            this.WriteD(this.Field1.Bonus.FakeRank);
            this.WriteD(this.Field1.Bonus.FakeRank);
            this.WriteU(this.Field1.Bonus.FakeNick, 66);
            this.WriteH((short)this.Field1.Bonus.CrosshairColor);
            this.WriteH((short)this.Field1.Bonus.MuzzleColor);
            this.WriteC((byte)this.Field1.Bonus.NickBorderColor);
        }
    }
}