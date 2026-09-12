// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK : GameServerPacket
    {
        private readonly PlayerInventory Field0;
        private readonly PlayerCharacters Field1;
        private readonly PlayerEquipment Field2;
        private readonly PlayerEquipment Field3;
        private readonly List<ItemsModel> Field4;
        private readonly List<int> Field5;
        private readonly List<int> Field6;
        private readonly int Field7;

        public PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK(Account A_1)
        {
            this.Field6 = new List<int>();
            this.Field5 = new List<int>();
            if (A_1 == null)
                return;
            this.Field3 = A_1.Equipment;
            this.Field0 = A_1.Inventory;
            this.Field1 = A_1.Character;
            this.Field4 = A_1.Inventory.GetItemsByType(ItemCategory.Coupon);
            CharacterModel red = this.Field1.GetCharacter(this.Field3.CharaRedId);
            CharacterModel blue = this.Field1.GetCharacter(this.Field3.CharaBlueId);
            if (red != null)
                this.Field6.Add(red.Slot);
            if (blue != null)
                this.Field6.Add(blue.Slot);
            this.Field7 = this.Field3.CharaRedId;
        }

        public PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK(Account A_1, SlotModel A_2)
        {
            this.Field6 = new List<int>();
            this.Field5 = new List<int>();
            if (A_1 == null)
                return;
            this.Field3 = A_1.Equipment;
            this.Field0 = A_1.Inventory;
            this.Field1 = A_1.Character;
            this.Field4 = A_1.Inventory.GetItemsByType(ItemCategory.Coupon);
            RoomModel room = A_1.Room;
            if (room == null || A_2 == null)
                return;
            this.Field2 = A_2.Equipment;
            if (this.Field2.CharaRedId != this.Field3.CharaRedId)
                this.Field6.Add(this.Field1.GetCharacter(this.Field3.CharaRedId).Slot);
            if (this.Field2.CharaBlueId != this.Field3.CharaBlueId)
                this.Field6.Add(this.Field1.GetCharacter(this.Field3.CharaBlueId).Slot);
            this.Field7 = room.ValidateTeam(A_2.Team, A_2.CostumeTeam) == TeamEnum.FR_TEAM ? this.Field3.CharaRedId : this.Field3.CharaBlueId;
            if (this.Field2.DinoItem != this.Field3.DinoItem)
                this.Field5.Add(this.Field3.DinoItem);
            if (this.Field2.SprayId != this.Field3.SprayId)
                this.Field5.Add(this.Field3.SprayId);
            if (this.Field2.NameCardId == this.Field3.NameCardId)
                return;
            this.Field5.Add(this.Field3.NameCardId);
        }

        // Schema reconstructed from the 121 client deserializer (PACKET_SERVER_MESSAGE_CHANGE_INVENTORY::vf3 @0xEA671A).
        // Member wire order = linked-list head->next walk. Each S2MOValue<T,N> array prefixes a count
        // (1 byte if N<255, 2 bytes if N>=255). ITEM_INFO is 8 bytes = EquipmentDataChara order [Id][ObjId]
        // (same as the working 2453). 6-byte sub-header consumed by the packet base vf before any member.
        public override void Write()
        {
            CLogger.Print($"[3082 CHANGE_INV] handles={this.Field6.Count} red={this.Field3.CharaRedId} blue={this.Field3.CharaBlueId} field7={this.Field7}", LoggerType.Info);

            // --- header: opcode + reserved only. The client base vf consumes 6 bytes starting at
            // the 2-byte length prefix that GetCompleteBytes prepends ([length][opcode][reserved]),
            // then members start right after. Do NOT add a 3rd header word here or everything shifts. ---
            this.WriteH((short)3082);
            this.WriteH((short)0);

            // [1] S2MOValue<ITEM_INFO,6> -> loadout+164 = the 6 EMOTICON slots (emote wheel), count 1 byte.
            //     RE-PROVEN (dump121): loadout+164 is the emoticon block, NOT weapons. Confirmed by
            //     CharaEmotion::GetItemInfo (sub_D6E152, +8*idx, cap 6 = CHAR_EQUIPMENT_EMOTION_COUNT),
            //     the equip popup confirm (sub_951617 @0x951617, "Equip.cpp:1510" writes 6 pairs to
            //     this+164), and CharChangeEquip121Parser reading ITEM_INFO[6] as the FIRST block.
            //     Weapons live in the 18-block at +44 (section [8] below), so writing emoticons here
            //     does NOT affect the weapon preview. Old code wrote [primary..special,char] here, which
            //     is why the emoticon panel showed the equipped guns.
            this.WriteC((byte)6);
            int[] emoticons = this.Field3.Emoticons;
            for (int i = 0; i < 6; i++)
                this.WriteB(this.Field0.EquipmentDataChara(emoticons != null && i < emoticons.Length ? emoticons[i] : 0)); // +164 + 8*i  [Id][ObjId]

            // [2] S2MOValue<int,2> = equipped char handles -> loadout+36 (FR) / +40 (CT), count 1 byte
            this.WriteC((byte)this.Field6.Count);
            foreach (int slot in this.Field6)
                this.WriteD((uint)slot);

            // [3] S2MOValue<enum MATCH_VERSION,1> = gate. <2 applies the char-equip path. 0 = lobby.
            this.WriteC((byte)0);

            // [4] S2MOValue<ITEM_INFO,1> = accessory (no count, fixed 8 bytes)
            this.WriteB(this.Field0.EquipmentDataChara(this.Field3.AccessoryId));

            // [5] S2MOValue<unsigned int,39> = slot-invalid list, count 1 byte (empty)
            this.WriteC((byte)0);

            // [6] S2MOValue<unsigned short,1> = numSlotItemInvalid
            this.WriteH((short)0);

            // [7] S2MOValue<S2MO_CHAR_STATE,255> = char-state list, count 2 bytes (empty)
            this.WriteH((short)0);

            // [8] S2MOValue<S2MO_CHAR_EQUIP_INFO,1> = 18x8 equip block (144 bytes fixed) -> loadout+44/+108.
            //     Must mirror the 2453 block exactly or it wipes the loadout the 2453 set.
            byte[][] block = new byte[18][];
            for (int i = 0; i < 18; i++)
                block[i] = new byte[8];
            block[0] = this.Field0.EquipmentDataChara(this.Field3.WeaponPrimary);
            block[1] = this.Field0.EquipmentDataChara(this.Field3.WeaponSecondary);
            block[2] = this.Field0.EquipmentDataChara(this.Field3.WeaponMelee);
            block[3] = this.Field0.EquipmentDataChara(this.Field3.WeaponExplosive);
            block[4] = this.Field0.EquipmentDataChara(this.Field3.WeaponSpecial);
            block[5] = this.Field0.EquipmentDataChara(this.Field3.WeaponSpecial2);
            block[8] = this.Field0.EquipmentDataChara(this.Field7);
            block[9] = this.Field0.EquipmentDataChara(this.Field3.PartHead);
            block[10] = this.Field0.EquipmentDataChara(this.Field3.PartFace);
            block[11] = this.Field0.EquipmentDataChara(this.Field3.PartJacket);
            block[12] = this.Field0.EquipmentDataChara(this.Field3.PartPocket);
            block[13] = this.Field0.EquipmentDataChara(this.Field3.PartGlove);
            block[14] = this.Field0.EquipmentDataChara(this.Field3.PartBelt);
            block[15] = this.Field0.EquipmentDataChara(this.Field3.PartHolster);
            block[16] = this.Field0.EquipmentDataChara(this.Field3.PartSkin);
            block[17] = this.Field0.EquipmentDataChara(this.Field3.BeretItem);
            for (int i = 0; i < 18; i++)
                this.WriteB(block[i]);

            // [9] S2MOValue<ITEM_INFO,3> -> loadout+132 block. RPM-PROVEN (dump121, live pid 35412) the 121
            //     client reads this block as 3 pairs [Id][ObjId] = [Dino @+132][Spray @+140][NameCard @+148],
            //     NOT head/face/beret. Client equip-flow sub_94EC88 @0x94EC88 writes the same 3 offsets, and
            //     UITopMenu__SetMyDefaultInfo/sub_9BFA00 @0x9BFA00 read the name card from loadout+148.
            //     Old code sent [PartHead,PartFace,BeretItem] here => wig showed in Dino, mask in Spray, and
            //     the name card/border never appeared on the bottom-left bar at login (loadout+148 stayed 0).
            //     Live proof: correct equip => +132=1500511(dino) +140=3199508(spray) +148=3400001(namecard).
            this.WriteC((byte)3);
            this.WriteB(this.Field0.EquipmentDataChara(this.Field3.DinoItem));    // +132 Dino
            this.WriteB(this.Field0.EquipmentDataChara(this.Field3.SprayId));     // +140 Spray
            this.WriteB(this.Field0.EquipmentDataChara(this.Field3.NameCardId));  // +148 NameCard (bar reads here)
        }
    }
}
