// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_GET_USER_ITEM_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_USER_ITEM_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly PlayerInventory Field1;
        private readonly PlayerEquipment Field2;

        public PROTOCOL_ROOM_GET_USER_ITEM_ACK(Account A_1)
        {
            this.Field0 = A_1;
            if (A_1 == null)
                return;
            this.Field1 = A_1.Inventory;
            this.Field2 = A_1.Equipment;
        }

        public override void Write()
        {
            this.WriteH((short)3647);
            this.WriteH((short)0);

            // [1] ITEM_INFO (8B, no count) = accessory
            this.WriteB(this.Field1.EquipmentDataChara(this.Field2.AccessoryId));

            // [2] S2MOValue<uint,1108> additional item ids: count is 2 bytes (N>=255), empty
            this.WriteH((short)0);

            // [3] S2MOValue<ITEM_INFO,3> = Dino/Spray/NameCard, count 1 byte
            this.WriteC((byte)3);
            this.WriteB(this.Field1.EquipmentDataChara(this.Field2.DinoItem));
            this.WriteB(this.Field1.EquipmentDataChara(this.Field2.SprayId));
            this.WriteB(this.Field1.EquipmentDataChara(this.Field2.NameCardId));

            // [4] S2MOValue<S2MO_CHAR_EQUIP_INFO,1> = 18 x 8B (no count). Mirror 3082 slot map.
            byte[][] block = new byte[18][];
            for (int i = 0; i < 18; i++)
                block[i] = new byte[8];
            block[0] = this.Field1.EquipmentDataChara(this.Field2.WeaponPrimary);
            block[1] = this.Field1.EquipmentDataChara(this.Field2.WeaponSecondary);
            block[2] = this.Field1.EquipmentDataChara(this.Field2.WeaponMelee);
            block[3] = this.Field1.EquipmentDataChara(this.Field2.WeaponExplosive);
            block[4] = this.Field1.EquipmentDataChara(this.Field2.WeaponSpecial);
            block[8] = this.Method0(this.Field0, this.Field2);   // team char (FR->Red / CT->Blue)
            block[9] = this.Field1.EquipmentDataChara(this.Field2.PartHead);
            block[10] = this.Field1.EquipmentDataChara(this.Field2.PartFace);
            block[11] = this.Field1.EquipmentDataChara(this.Field2.PartJacket);
            block[12] = this.Field1.EquipmentDataChara(this.Field2.PartPocket);
            block[13] = this.Field1.EquipmentDataChara(this.Field2.PartGlove);
            block[14] = this.Field1.EquipmentDataChara(this.Field2.PartBelt);
            block[15] = this.Field1.EquipmentDataChara(this.Field2.PartHolster);
            block[16] = this.Field1.EquipmentDataChara(this.Field2.PartSkin);
            block[17] = this.Field1.EquipmentDataChara(this.Field2.BeretItem);
            for (int i = 0; i < 18; i++)
                this.WriteB(block[i]);
        }

        private byte[] Method0(Account A_1, PlayerEquipment A_2)
        {
            int itemId = A_2.CharaRedId;
            RoomModel room = A_1.Room;
            SlotModel Slot;
            if (room != null && room.GetSlot(A_1.SlotId, out Slot))
                itemId = room.ValidateTeam(Slot.Team, Slot.CostumeTeam) == TeamEnum.FR_TEAM ? A_2.CharaRedId : A_2.CharaBlueId;
            return this.Field1.EquipmentDataChara(itemId);
        }
    }
}
