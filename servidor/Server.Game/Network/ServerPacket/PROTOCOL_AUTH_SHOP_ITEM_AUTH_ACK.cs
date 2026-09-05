// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_ITEM_AUTH_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_ITEM_AUTH_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly ItemsModel Field1;

        
        public PROTOCOL_AUTH_SHOP_ITEM_AUTH_ACK(uint A_1, ItemsModel A_2 = null, Account A_3 = null)
        {
            this.Field1 = A_2;
            if (A_2 != null && A_1 == 1U)
            {
                switch (ComDiv.GetIdStatics(A_2.Id, 1))
                {
                    case 17:
                    case 18:
                    case 20:
                    case 37:
                        if (A_2.Count > 1U && A_2.Equip == ItemEquipType.Durable)
                        {
                            ComDiv.UpdateDB("player_items", "count", (object)(long)--A_2.Count, "object_id", (object)A_2.ObjectId, "owner_id", (object)A_3.PlayerId);
                            break;
                        }
                        DaoManagerSQL.DeletePlayerInventoryItem(A_2.ObjectId, A_3.PlayerId);
                        A_3.Inventory.RemoveItem(A_2);
                        A_2.Id = 0;
                        A_2.Count = 0U;
                        break;
                    default:
                        A_2.Equip = ItemEquipType.Temporary;
                        break;
                }
            }
            else
                A_1 = 2147483648U /*0x80000000*/;
            this.Field0 = A_1;
        }

        
        public override void Write()
        {
            this.WriteH((short)1051);
            this.WriteD(this.Field0);
            if (this.Field0 != 1U)
                return;
            this.WriteD(uint.Parse(DateTimeUtil.Now("yyMMddHHmm")));
            this.WriteD((uint)this.Field1.ObjectId);
            if (this.Field1.Category == ItemCategory.Coupon && this.Field1.Equip == ItemEquipType.Temporary)
            {
                this.WriteD(0);
                this.WriteC((byte)1);
                this.WriteD(0);
            }
            else
            {
                this.WriteD(this.Field1.Id);
                this.WriteC((byte)this.Field1.Equip);
                this.WriteD(this.Field1.Count);
            }
        }
    }
}
