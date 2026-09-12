// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_START_GAME_TRANS_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_START_GAME_TRANS_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;
        private readonly SlotModel Field1;
        private readonly PlayerEquipment Field2;
        private readonly PlayerTitles Field3;
        private readonly int Field4;

        public PROTOCOL_BATTLE_START_GAME_TRANS_ACK(RoomModel A_1, SlotModel A_2, PlayerTitles A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field3 = A_3;
            if (A_1 == null || A_2 == null || A_3 == null)
                return;
            this.Field2 = A_2.Equipment;
            if (this.Field2 == null)
                return;
            switch (A_1.ValidateTeam(A_2.Team, A_2.CostumeTeam))
            {
                case TeamEnum.FR_TEAM:
                    this.Field4 = this.Field2.CharaRedId;
                    break;
                case TeamEnum.CT_TEAM:
                    this.Field4 = this.Field2.CharaBlueId;
                    break;
            }
        }

        
        public override void Write()
        {
            this.WriteH((short)5128);
            this.WriteH((short)0);
            this.WriteD((uint)this.Field1.PlayerId);
            this.WriteC((byte)2);
            this.WriteC((byte)this.Field1.Id);
            this.WriteD(this.Field4);
            this.WriteD(SafeWeapon(this.Field2.WeaponPrimary, 1));
            this.WriteD(SafeWeapon(this.Field2.WeaponSecondary, 2));
            this.WriteD(SafeWeapon(this.Field2.WeaponMelee, 3));
            this.WriteD(SafeWeapon(this.Field2.WeaponExplosive, 4));
            this.WriteD(SafeWeapon(this.Field2.WeaponSpecial, 5));
            this.WriteD(this.Field2.WeaponSpecial2 != 0 ? SafeWeapon(this.Field2.WeaponSpecial2, 5) : 0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteD(this.Field4);
            this.WriteD(SafePart(this.Field2.PartHead, 7));
            this.WriteD(SafePart(this.Field2.PartFace, 8));
            this.WriteD(SafePart(this.Field2.PartJacket, 9));
            this.WriteD(SafePart(this.Field2.PartPocket, 10));
            this.WriteD(SafePart(this.Field2.PartGlove, 11));
            this.WriteD(SafePart(this.Field2.PartBelt, 12));
            this.WriteD(SafePart(this.Field2.PartHolster, 13));
            this.WriteD(SafePart(this.Field2.PartSkin, 14));
            this.WriteD(0);
            this.WriteB(new byte[8] { 100, 100, 100, 100, 100, 100, 100, 100 });
            this.WriteB(new byte[39]);
        }

        private static readonly int[] WeaponFallback = new int[5] { 103004, 202022, 301012, 407056, 508002 };

        private static int SafeWeapon(int id, int category)
        {
            if (id != 0 && id / 100000 % 100 == category)
                return id;
            return WeaponFallback[category - 1];
        }

        private static int SafePart(int id, int category)
        {
            if (id != 0 && id / 100000 % 100 == category)
                return id;
            return 1000000000 + category * 100000;
        }
    }
}