// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_RESPAWN_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_RESPAWN_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;
        private readonly SlotModel Field1;
        private readonly PlayerEquipment Field2;
        private readonly List<int> Field3;
        private readonly int Field4;

        public PROTOCOL_BATTLE_RESPAWN_ACK(RoomModel A_1, SlotModel A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            if (A_1 == null || A_2 == null)
                return;
            this.Field2 = A_2.Equipment;
            if (this.Field2 != null)
            {
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
            this.Field3 = AllUtils.GetDinossaurs(A_1, false, A_2.Id);
        }

        
        public override void Write()
        {
            this.WriteH((short)5138);
            this.WriteD(this.Field1.Id);
            this.WriteD(this.Field0.SpawnsCount++);
            this.WriteD(++this.Field1.SpawnsCount);
            this.WriteD(this.Field2.WeaponPrimary);
            this.WriteD(this.Field2.WeaponSecondary);
            this.WriteD(this.Field2.WeaponMelee);
            this.WriteD(this.Field2.WeaponExplosive);
            this.WriteD(this.Field2.WeaponSpecial);
            this.WriteD(this.Field2.WeaponSpecial2);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteB(Bitwise.HexStringToByteArray("64 64 64 64 64 00 00 00"));
            this.WriteD(this.Field4);
            this.WriteD(this.Field2.PartHead);
            this.WriteD(this.Field2.PartFace);
            this.WriteD(this.Field2.PartJacket);
            this.WriteD(this.Field2.PartPocket);
            this.WriteD(this.Field2.PartGlove);
            this.WriteD(this.Field2.PartBelt);
            this.WriteD(this.Field2.PartHolster);
            this.WriteD(this.Field2.PartSkin);
            this.WriteD(this.Field2.BeretItem);
            this.WriteD(this.Field2.AccessoryId);
        }

        
        private byte[] Method0(RoomModel A_1, List<int> A_2)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1.IsDinoMode())
                {
                    int num1 = A_2.Count == 1 || A_1.IsDinoMode("CC") ? (int)byte.MaxValue : A_1.TRex;
                    syncServerPacket.WriteC((byte)num1);
                    syncServerPacket.WriteC((byte)10);
                    for (int index = 0; index < A_2.Count; ++index)
                    {
                        int num2 = A_2[index];
                        if (num2 != A_1.TRex && A_1.IsDinoMode("DE") || A_1.IsDinoMode("CC"))
                            syncServerPacket.WriteC((byte)num2);
                    }
                    int num3 = 8 - A_2.Count - (num1 == (int)byte.MaxValue ? 1 : 0);
                    for (int index = 0; index < num3; ++index)
                        syncServerPacket.WriteC(byte.MaxValue);
                    syncServerPacket.WriteC(byte.MaxValue);
                }
                else
                    syncServerPacket.WriteB(new byte[10]);
                return syncServerPacket.ToArray();
            }
        }
    }
}