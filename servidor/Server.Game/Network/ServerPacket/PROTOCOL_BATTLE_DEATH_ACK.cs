// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BATTLE_DEATH_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_DEATH_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;
        private readonly FragInfos Field1;
        private readonly SlotModel Field2;

        public PROTOCOL_BATTLE_DEATH_ACK(RoomModel A_1, FragInfos A_2, SlotModel A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        
        public override void Write()
        {
            this.WriteH((short)5136);
            this.WriteC((byte)this.Field1.KillingType);
            this.WriteC(this.Field1.KillsCount);
            this.WriteC(this.Field1.KillerSlot);
            this.WriteD(this.Field1.WeaponId);
            this.WriteT(this.Field1.X);
            this.WriteT(this.Field1.Y);
            this.WriteT(this.Field1.Z);
            this.WriteC(this.Field1.Flag);
            this.WriteC(this.Field1.Unk);
            for (int index = 0; index < (int)this.Field1.KillsCount; ++index)
            {
                FragModel frag = this.Field1.Frags[index];
                this.WriteC(frag.VictimSlot);
                this.WriteC(frag.WeaponClass);
                this.WriteC(frag.HitspotInfo);
                this.WriteH((ushort)frag.KillFlag);
                this.WriteC(frag.Unk);
                this.WriteT(frag.X);
                this.WriteT(frag.Y);
                this.WriteT(frag.Z);
                this.WriteC(frag.AssistSlot);
                if (frag.Unks == null || frag.Unks.Length < 4)
                    throw new System.InvalidOperationException($"PROTOCOL_BATTLE_DEATH_ACK: frag {index} Unks needs 4 bytes, got {(frag.Unks == null ? "null" : frag.Unks.Length.ToString())}");
                this.WriteC(frag.Unks[0]);
                this.WriteC(frag.Unks[1]);
                this.WriteC(frag.Unks[2]);
                this.WriteC(frag.Unks[3]);
            }
            this.WriteH((ushort)this.Field0.FRKills);
            this.WriteH((ushort)this.Field0.FRDeaths);
            this.WriteH((ushort)this.Field0.FRAssists);
            this.WriteH((ushort)this.Field0.CTKills);
            this.WriteH((ushort)this.Field0.CTDeaths);
            this.WriteH((ushort)this.Field0.CTAssists);
            foreach (SlotModel slot in this.Field0.Slots)
            {
                this.WriteH((ushort)slot.AllKills);
                this.WriteH((ushort)slot.AllDeaths);
                this.WriteH((ushort)slot.AllAssists);
            }
            this.WriteC(this.Field2.KillsOnLife == 2 ? (byte)1 : (this.Field2.KillsOnLife == 3 ? (byte)2 : (this.Field2.KillsOnLife > 3 ? (byte)3 : (byte)0)));
            this.WriteH((ushort)this.Field1.Score);
        }
    }
}