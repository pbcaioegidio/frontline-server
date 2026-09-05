// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_CREATE_NICK_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_CREATE_NICK_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;
        private readonly PlayerInventory Field2;
        private readonly PlayerEquipment Field3;

        public PROTOCOL_BASE_CREATE_NICK_ACK(uint A_1, Account A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            if (A_2 == null)
                return;
            this.Field2 = A_2.Inventory;
            this.Field3 = A_2.Equipment;
        }

        public override void Write()
        {
            this.WriteH((short)2327);
            this.WriteH((short)0);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteC((byte)1);
            this.WriteB(this.Field2.EquipmentData(this.Field3.DinoItem));
            this.WriteC((byte)(this.Field1.Nickname.Length * 2));
            this.WriteU(this.Field1.Nickname, this.Field1.Nickname.Length * 2);
        }
    }
}