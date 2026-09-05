// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly List<ItemsModel> Field1 = new List<ItemsModel>();
        private readonly List<ItemsModel> Field2 = new List<ItemsModel>();
        private readonly List<ItemsModel> Field3 = new List<ItemsModel>();
        private readonly List<ItemsModel> Field4 = new List<ItemsModel>();

        public PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK(uint A_1, ItemsModel A_2 = null, Account A_3 = null)
        {
            this.Field0 = A_1;
            ItemsModel Model = new ItemsModel(A_2);
            if (Model == null)
                return;
            ComDiv.TryCreateItem(Model, A_3.Inventory, A_3.PlayerId);
            if (Model.Category == ItemCategory.Weapon)
                this.Field2.Add(Model);
            else if (Model.Category != ItemCategory.Character)
            {
                if (Model.Category == ItemCategory.Coupon)
                {
                    this.Field3.Add(Model);
                }
                else
                {
                    if (Model.Category != ItemCategory.NewItem)
                        return;
                    this.Field4.Add(Model);
                }
            }
            else
                this.Field1.Add(Model);
        }

        public override void Write()
        {
            this.WriteH((short)1057);
            this.WriteD(this.Field0);
            if (this.Field0 != 1U)
                return;
            this.WriteH((short)0);
            this.WriteH((ushort)(this.Field1.Count + this.Field2.Count + this.Field3.Count + this.Field4.Count));
            foreach (ItemsModel itemsModel in this.Field1)
            {
                this.WriteD((uint)itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            foreach (ItemsModel itemsModel in this.Field2)
            {
                this.WriteD((uint)itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            foreach (ItemsModel itemsModel in this.Field3)
            {
                this.WriteD((uint)itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            foreach (ItemsModel itemsModel in this.Field4)
            {
                this.WriteD((uint)itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
        }
    }
}
