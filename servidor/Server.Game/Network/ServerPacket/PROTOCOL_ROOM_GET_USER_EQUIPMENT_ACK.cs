// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_GET_USER_EQUIPMENT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_USER_EQUIPMENT_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly SlotModel Field1;
        private readonly PlayerEquipment Field2;
        private readonly int Field3;

        public PROTOCOL_ROOM_GET_USER_EQUIPMENT_ACK(uint A_1, RoomModel A_2, SlotModel A_3)
        {
            this.Field1 = A_3;
            uint num = A_1;
            if (A_2 != null && A_3 != null)
            {
                this.Field2 = A_3.Equipment;
                if (this.Field2 != null)
                {
                    switch (A_2.ValidateTeam(A_3.Team, A_3.CostumeTeam))
                    {
                        case TeamEnum.FR_TEAM:
                            this.Field3 = this.Field2.CharaRedId;
                            break;
                        case TeamEnum.CT_TEAM:
                            this.Field3 = this.Field2.CharaBlueId;
                            break;
                    }
                }
                else
                    num = 134217728U /*0x08000000*/;
            }
            else
                num = 134217728U /*0x08000000*/;
            this.Field0 = num;
        }

        public override void Write()
        {
            this.WriteH((short)3667);
            this.WriteH((short)0);
            this.WriteD(this.Field0);
            if (this.Field0 != 0U)
                return;
            this.WriteQ(0L);
            this.WriteD(0);
            this.WriteC((byte)10);
            this.WriteD(this.Field3);
            this.WriteD(this.Field2.PartHead);
            this.WriteD(this.Field2.PartFace);
            this.WriteD(this.Field2.PartJacket);
            this.WriteD(this.Field2.PartPocket);
            this.WriteD(this.Field2.PartGlove);
            this.WriteD(this.Field2.PartBelt);
            this.WriteD(this.Field2.PartHolster);
            this.WriteD(this.Field2.PartSkin);
            this.WriteD(this.Field2.BeretItem);
            this.WriteC((byte)8);
            this.WriteD(this.Field2.WeaponPrimary);
            this.WriteD(this.Field2.WeaponSecondary);
            this.WriteD(this.Field2.WeaponMelee);
            this.WriteD(this.Field2.WeaponExplosive);
            this.WriteD(this.Field2.WeaponSpecial);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteC((byte)2);
            this.WriteD(this.Field2.CharaRedId);
            this.WriteD(this.Field2.CharaBlueId);
            this.WriteC((byte)this.Field1.Id);
        }
    }
}