// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly uint Field1;
        private readonly int Field2;

        public PROTOCOL_BASE_QUEST_ACTIVE_IDX_CHANGE_ACK(uint A_1, int A_2, Account A_3)
        {
            this.Field1 = A_1;
            this.Field2 = A_2;
            this.Field0 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)2361);
            this.WriteD(this.Field1);
            this.WriteC((byte)this.Field2);
            if (((int)this.Field1 & 1) != 1)
                return;
            this.WriteD(this.Field0.Exp);
            this.WriteD(0);
            this.WriteD(this.Field0.Gold);
            this.WriteD(this.Field0.Ribbon);
            this.WriteD(this.Field0.Ensign);
            this.WriteD(this.Field0.Medal);
            this.WriteD(this.Field0.MasterMedal);
        }
    }
}