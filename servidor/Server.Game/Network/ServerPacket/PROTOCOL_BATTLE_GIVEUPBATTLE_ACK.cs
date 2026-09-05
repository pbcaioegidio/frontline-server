// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_GIVEUPBATTLE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_GIVEUPBATTLE_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly int Field1;

        public PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(Account A_1, int A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)5134);
            this.WriteD(this.Field0.SlotId);
            this.WriteC((byte)this.Field1);
            this.WriteQ((ulong)this.Field0.Exp);
            this.WriteD(this.Field0.Rank);
            this.WriteD(this.Field0.Gold);
            this.WriteD(this.Field0.Statistic.Season.EscapesCount);
            this.WriteD(this.Field0.Statistic.Basic.EscapesCount);
            this.WriteD(0);
            this.WriteC((byte)0);
        }
    }
}