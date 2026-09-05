// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SHOP_REPAIR_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SHOP_REPAIR_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly List<ItemsModel> Field1;
        private readonly Account Field2;

        public PROTOCOL_SHOP_REPAIR_ACK(uint A_1, List<ItemsModel> A_2, Account A_3)
        {
            this.Field0 = A_1;
            this.Field2 = A_3;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1084);
            this.WriteH((short)0);
            if (this.Field0 == 1U)
            {
                this.WriteC((byte)this.Field1.Count);
                foreach (ItemsModel itemsModel in this.Field1)
                {
                    this.WriteD((uint)itemsModel.ObjectId);
                    this.WriteD(itemsModel.Id);
                }
                this.WriteD(this.Field2.Cash);
                this.WriteD(this.Field2.Gold);
            }
            else
                this.WriteD(this.Field0);
        }
    }
}
