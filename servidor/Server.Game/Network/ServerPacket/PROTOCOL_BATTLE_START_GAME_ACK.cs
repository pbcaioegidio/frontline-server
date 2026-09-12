// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_START_GAME_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_START_GAME_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_BATTLE_START_GAME_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5127);
            this.WriteH((short)0);
            this.WriteB(this.Method0(this.Field0));
            this.WriteB(this.Method1(this.Field0));
            this.WriteB(this.Method2(this.Field0));
            this.WriteC((byte)this.Field0.MapId);
            this.WriteC((byte)this.Field0.Rule);
            this.WriteC((byte)this.Field0.Stage);
            this.WriteC((byte)this.Field0.RoomType);
            this.WriteC((byte)0);
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

        private byte[] Method0(RoomModel A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.Slots.Length);
                foreach (SlotModel slot in A_1.Slots)
                    syncServerPacket.WriteD((uint)slot.AllKills);
                return syncServerPacket.ToArray();
            }
        }

        private byte[] Method1(RoomModel A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.Slots.Length);
                foreach (SlotModel slot in A_1.Slots)
                    syncServerPacket.WriteC((byte)slot.Team);
                return syncServerPacket.ToArray();
            }
        }

        
        private byte[] Method2(RoomModel A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.GetReadyPlayers());
                foreach (SlotModel slot in A_1.Slots)
                {
                    if (slot.State >= SlotState.READY && slot.Equipment != null)
                    {
                        Account playerBySlot = A_1.GetPlayerBySlot(slot);
                        if (playerBySlot != null && playerBySlot.SlotId == slot.Id)
                        {
                            syncServerPacket.WriteC((byte)slot.Id);
                            PlayerEquipment equipment = playerBySlot.Equipment;
                            PlayerTitles title = playerBySlot.Title;
                            int num = 0;
                            if (equipment != null && title != null)
                            {
                                switch (A_1.ValidateTeam(slot.Team, slot.CostumeTeam))
                                {
                                    case TeamEnum.FR_TEAM:
                                        num = equipment.CharaRedId;
                                        break;
                                    case TeamEnum.CT_TEAM:
                                        num = equipment.CharaBlueId;
                                        break;
                                }
                                syncServerPacket.WriteD(num);
                                syncServerPacket.WriteD(SafeWeapon(equipment.WeaponPrimary, 1));
                                syncServerPacket.WriteD(SafeWeapon(equipment.WeaponSecondary, 2));
                                syncServerPacket.WriteD(SafeWeapon(equipment.WeaponMelee, 3));
                                syncServerPacket.WriteD(SafeWeapon(equipment.WeaponExplosive, 4));
                                syncServerPacket.WriteD(SafeWeapon(equipment.WeaponSpecial, 5));
                                syncServerPacket.WriteD(equipment.WeaponSpecial2 != 0 ? SafeWeapon(equipment.WeaponSpecial2, 5) : 0);
                                syncServerPacket.WriteD(0);
                                syncServerPacket.WriteD(0);
                                syncServerPacket.WriteD(num);
                                syncServerPacket.WriteD(SafePart(equipment.PartHead, 7));
                                syncServerPacket.WriteD(SafePart(equipment.PartFace, 8));
                                syncServerPacket.WriteD(SafePart(equipment.PartJacket, 9));
                                syncServerPacket.WriteD(SafePart(equipment.PartPocket, 10));
                                syncServerPacket.WriteD(SafePart(equipment.PartGlove, 11));
                                syncServerPacket.WriteD(SafePart(equipment.PartBelt, 12));
                                syncServerPacket.WriteD(SafePart(equipment.PartHolster, 13));
                                syncServerPacket.WriteD(SafePart(equipment.PartSkin, 14));
                                syncServerPacket.WriteD(0);
                                syncServerPacket.WriteB(new byte[8] { 100, 100, 100, 100, 100, 100, 100, 100 });
                                syncServerPacket.WriteB(new byte[39]);
                            }
                        }
                    }
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}