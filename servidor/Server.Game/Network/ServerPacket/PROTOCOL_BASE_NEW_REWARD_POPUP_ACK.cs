// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_NEW_REWARD_POPUP_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_NEW_REWARD_POPUP_ACK : GameServerPacket
    {
        private readonly ItemsModel Field0;
        private readonly PlayerInventory Field1;
        private readonly List<ItemsModel> Field2;

        public PROTOCOL_BASE_NEW_REWARD_POPUP_ACK(Account A_1, ItemsModel A_2)
        {
            this.Field0 = A_2;
            if (A_1 == null)
                return;
            this.Field1 = A_1.Inventory;
            this.Field2 = new List<ItemsModel>();
            if (A_2 == null)
                return;
            this.Field2.Add(A_2);
        }

        public override void Write()
        {
            this.WriteH((short)2430);
            this.WriteH((short)0);
            this.WriteH((ushort)this.Field1.Items.Count);
            this.WriteC((byte)1);
            this.WriteD(this.Field0.Id);
        }
    }
}