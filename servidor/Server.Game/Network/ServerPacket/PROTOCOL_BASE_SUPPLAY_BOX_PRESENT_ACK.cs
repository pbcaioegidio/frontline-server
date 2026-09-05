// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_SUPPLAY_BOX_PRESENT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_SUPPLAY_BOX_PRESENT_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly List<ItemsModel> Field1 = new List<ItemsModel>();
        private readonly List<ItemsModel> Field2 = new List<ItemsModel>();
        private readonly List<ItemsModel> Field3 = new List<ItemsModel>();

        public PROTOCOL_BASE_SUPPLAY_BOX_PRESENT_ACK(uint A_1, ItemsModel A_2, Account A_3)
        {
            this.Field0 = A_1;
            ItemsModel Model = new ItemsModel(A_2);
            if (Model == null)
                return;
            ComDiv.TryCreateItem(Model, A_3.Inventory, A_3.PlayerId);
            SendItemInfo.LoadItem(A_3, Model);
            if (Model.Category != ItemCategory.Weapon)
            {
                if (Model.Category == ItemCategory.Character)
                {
                    this.Field1.Add(Model);
                }
                else
                {
                    if (Model.Category != ItemCategory.Coupon)
                        return;
                    this.Field3.Add(Model);
                }
            }
            else
                this.Field2.Add(Model);
        }

        public override void Write()
        {
            this.WriteH((short)2411);
            this.WriteD(this.Field1.Count);
            this.WriteD(this.Field2.Count);
            this.WriteD(this.Field3.Count);
            this.WriteD(0);
            foreach (ItemsModel itemsModel in this.Field1)
            {
                this.WriteQ(itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            foreach (ItemsModel itemsModel in this.Field2)
            {
                this.WriteQ(itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            foreach (ItemsModel itemsModel in this.Field3)
            {
                this.WriteQ(itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
        }
    }
}