// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CHAR_CREATE_CHARA_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CHAR_CREATE_CHARA_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;
        private readonly CharacterModel Field2;
        private readonly PlayerInventory Field3;
        private readonly PlayerEquipment Field4;
        private readonly byte Field5;

        public PROTOCOL_CHAR_CREATE_CHARA_ACK(uint A_1, byte A_2, CharacterModel A_3, Account A_4)
        {
            this.Field0 = A_1;
            this.Field1 = A_4;
            if (A_4 != null)
            {
                this.Field3 = A_4.Inventory;
                this.Field4 = A_4.Equipment;
            }
            this.Field2 = A_3;
            this.Field5 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)6146);
            this.WriteH((short)0);
            // 121 S2MO PACKET_CHAR_CREATE_CHARA_ACK wire, after the 6-byte client header
            // [len][opcode][this WriteH(0)]. Confirmed from vf3 @0xEB3209 + walker sub_EB3416:
            //   [result u32][uchar4][S2MO_CHAR_EQUIP_INFO 18x8B][cash u32][gold u32][uchar3][uchar2][uchar1][bool]
            // vf3 reads result FIRST and gates on result>=0; the field linked list then walks head->tail.
            // The old build omitted the leading result dword, so the client read cash from the gold field
            // and gold from the trailing [Field5][20][Slot][1] (= 0x01061401 = the 17,175,553 garbage).
            // The 18-entry block mirrors 2453: weapons 0-4, char at index 8, parts 9-17 (EquipmentDataChara).
            this.WriteD(this.Field0);
            if (this.Field0 == 0)
            {
                this.WriteC((byte)0);
                byte[][] block = new byte[18][];
                for (int i = 0; i < 18; i++)
                    block[i] = new byte[8];
                block[0] = this.Field3.EquipmentDataChara(this.Field4.WeaponPrimary);
                block[1] = this.Field3.EquipmentDataChara(this.Field4.WeaponSecondary);
                block[2] = this.Field3.EquipmentDataChara(this.Field4.WeaponMelee);
                block[3] = this.Field3.EquipmentDataChara(this.Field4.WeaponExplosive);
                block[4] = this.Field3.EquipmentDataChara(this.Field4.WeaponSpecial);
                block[5] = this.Field3.EquipmentDataChara(this.Field4.WeaponSpecial2);
                block[8] = this.Field3.EquipmentDataChara(this.Field2.Id);
                block[9] = this.Field3.EquipmentDataChara(this.Field4.PartHead);
                block[10] = this.Field3.EquipmentDataChara(this.Field4.PartFace);
                block[11] = this.Field3.EquipmentDataChara(this.Field4.PartJacket);
                block[12] = this.Field3.EquipmentDataChara(this.Field4.PartPocket);
                block[13] = this.Field3.EquipmentDataChara(this.Field4.PartGlove);
                block[14] = this.Field3.EquipmentDataChara(this.Field4.PartBelt);
                block[15] = this.Field3.EquipmentDataChara(this.Field4.PartHolster);
                block[16] = this.Field3.EquipmentDataChara(this.Field4.PartSkin);
                block[17] = this.Field3.EquipmentDataChara(this.Field4.BeretItem);
                for (int i = 0; i < 18; i++)
                    this.WriteB(block[i]);
                this.WriteD(this.Field1.Cash);
                this.WriteD(this.Field1.Gold);
                this.WriteC(this.Field5);
                this.WriteC((byte)20);
                this.WriteC((byte)this.Field2.Slot);
                this.WriteC((byte)1);
            }
        }
    }
}