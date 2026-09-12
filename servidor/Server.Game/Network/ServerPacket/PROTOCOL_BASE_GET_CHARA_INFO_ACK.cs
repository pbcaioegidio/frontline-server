using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_CHARA_INFO_ACK : GameServerPacket
    {
        private readonly PlayerInventory Field0;
        private readonly PlayerEquipment Field1;
        private readonly List<CharacterModel> Field2;

        public PROTOCOL_BASE_GET_CHARA_INFO_ACK(Account A_1)
        {
            this.Field0 = A_1.Inventory;
            this.Field1 = A_1.Equipment;
            this.Field2 = A_1.Character.Characters;
        }

        public override void Write()
        {
            CLogger.Print($"[2453 GET_CHARA_INFO] GAME chars={this.Field2.Count}, CharaRed={this.Field1.CharaRedId}, CharaBlue={this.Field1.CharaBlueId}", LoggerType.Info);
            foreach (CharacterModel cm in this.Field2)
                CLogger.Print($"[2453]   slot={cm.Slot} ObjId={cm.ObjectId} Id={cm.Id}", LoggerType.Info);

            this.WriteH((short)2453);
            this.WriteH((short)0);
            this.WriteH((short)this.Field2.Count);
            foreach (CharacterModel characterModel in this.Field2)
            {
                this.WriteC((byte)characterModel.Slot);
                this.WriteC((byte)20);
                this.WriteD((uint)characterModel.Id);
                this.WriteD((uint)characterModel.ObjectId);
            }
            byte[][] block = new byte[18][];
            for (int i = 0; i < 18; i++)
                block[i] = new byte[8];
            block[0] = this.Field0.EquipmentDataChara(this.Field1.WeaponPrimary);
            block[1] = this.Field0.EquipmentDataChara(this.Field1.WeaponSecondary);
            block[2] = this.Field0.EquipmentDataChara(this.Field1.WeaponMelee);
            block[3] = this.Field0.EquipmentDataChara(this.Field1.WeaponExplosive);
            block[4] = this.Field0.EquipmentDataChara(this.Field1.WeaponSpecial);
            block[5] = this.Field0.EquipmentDataChara(this.Field1.WeaponSpecial2);
            block[8] = this.Field0.EquipmentDataChara(this.Field1.CharaRedId);
            block[9] = this.Field0.EquipmentDataChara(this.Field1.PartHead);
            block[10] = this.Field0.EquipmentDataChara(this.Field1.PartFace);
            block[11] = this.Field0.EquipmentDataChara(this.Field1.PartJacket);
            block[12] = this.Field0.EquipmentDataChara(this.Field1.PartPocket);
            block[13] = this.Field0.EquipmentDataChara(this.Field1.PartGlove);
            block[14] = this.Field0.EquipmentDataChara(this.Field1.PartBelt);
            block[15] = this.Field0.EquipmentDataChara(this.Field1.PartHolster);
            block[16] = this.Field0.EquipmentDataChara(this.Field1.PartSkin);
            block[17] = this.Field0.EquipmentDataChara(this.Field1.BeretItem);
            for (int i = 0; i < 18; i++)
                this.WriteB(block[i]);
        }

    }
}