// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_INVENTORY_GET_INFO_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_INVENTORY_GET_INFO_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly PlayerInventory Field1;
        private readonly List<ItemsModel> Field2;

        public PROTOCOL_INVENTORY_GET_INFO_ACK(int A_1, Account A_2, List<GoodsItem> A_3)
        {
            this.Field0 = A_1;
            if (A_2 != null)
            {
                this.Field1 = A_2.Inventory;
                this.Field2 = new List<ItemsModel>();
            }
            foreach (GoodsItem goodsItem in A_3)
            {
                ItemsModel Model = new ItemsModel(goodsItem.Item);
                if (Model != null)
                {
                    if (A_1 == 0)
                        ComDiv.TryCreateItem(Model, A_2.Inventory, A_2.PlayerId);
                    SendItemInfo.LoadItem(A_2, Model);
                    this.Field2.Add(Model);
                }
            }
        }

        public PROTOCOL_INVENTORY_GET_INFO_ACK(int A_1, Account A_2, List<ItemsModel> A_3)
        {
            this.Field0 = A_1;
            if (A_2 != null)
            {
                this.Field1 = A_2.Inventory;
                this.Field2 = new List<ItemsModel>();
            }
            foreach (ItemsModel itemsModel in A_3)
            {
                ItemsModel Model = new ItemsModel(itemsModel);
                if (Model != null)
                {
                    if (A_1 == 0)
                        ComDiv.TryCreateItem(Model, A_2.Inventory, A_2.PlayerId);
                    SendItemInfo.LoadItem(A_2, Model);
                    this.Field2.Add(Model);
                }
            }
        }

        public PROTOCOL_INVENTORY_GET_INFO_ACK(int A_1, Account A_2, ItemsModel A_3)
        {
            this.Field0 = A_1;
            if (A_2 != null)
            {
                this.Field1 = A_2.Inventory;
                this.Field2 = new List<ItemsModel>();
            }
            ItemsModel Model = new ItemsModel(A_3);
            if (Model == null)
                return;
            if (A_1 == 0)
                ComDiv.TryCreateItem(Model, A_2.Inventory, A_2.PlayerId);
            SendItemInfo.LoadItem(A_2, Model);
            this.Field2.Add(Model);
        }

        public override void Write()
        {
            // S2MO PACKET_INVENTORY_GET_INFO_ACK wire: the client walks the field list directly,
            // with NO status dword (unlike 2319). After the 6-byte client header
            // [len][opcode][WriteH(0)] the fields are
            //   [inventoryCount:H][INVEN_BUFFER: bufCount:H + bufCount x 13B rows][changeType:C].
            // The decompiled-original WriteD(0) was a spurious extra dword: it shifted the field
            // region so the client read inventoryCount/bufCount from zero bytes -> 0 items parsed,
            // and the new char was only registered in-session after a relog via 2319. Layout
            // confirmed live. (The 121 addresses that used to be cited here, 0xEC3F26 and
            // sub_895F74, do NOT map to this parser in the 122 dump - do not trust them.)
            this.WriteH((short)3334);
            this.WriteH((short)0);
            this.WriteH((ushort)this.Field1.Items.Count);
            this.WriteH((ushort)this.Field2.Count);
            foreach (ItemsModel itemsModel in this.Field2)
            {
                this.WriteD((uint)itemsModel.ObjectId);
                this.WriteD(itemsModel.Id);
                this.WriteC((byte)itemsModel.Equip);
                this.WriteD(itemsModel.Count);
            }
            this.WriteC((byte)this.Field0);
        }

    }
}
