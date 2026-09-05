// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_ROOM_TEAM_BALANCE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_TEAM_BALANCE_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly int Field1;
        private readonly List<SlotModel> Field2;

        public PROTOCOL_ROOM_TEAM_BALANCE_ACK(List<SlotModel> A_1, int A_2, int A_3)
        {
            this.Field2 = A_1;
            this.Field1 = A_2;
            this.Field0 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)3622);
            this.WriteC((byte)this.Field0);
            this.WriteC((byte)this.Field1);
            this.WriteC((byte)this.Field2.Count);
            foreach (SlotModel slotModel in this.Field2)
            {
                this.WriteC((byte)slotModel.OldSlot.Id);
                this.WriteC((byte)slotModel.NewSlot.Id);
                this.WriteC(slotModel.OldSlot.GetClientState());
                this.WriteC(slotModel.NewSlot.GetClientState());
            }
        }
    }
}