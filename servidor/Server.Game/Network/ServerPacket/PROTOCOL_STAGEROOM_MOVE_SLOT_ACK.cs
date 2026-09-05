using System;
using System.Collections.Generic;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_STAGEROOM_MOVE_SLOT_ACK : GameServerPacket
    {
        public const int MaxSlots = 2;

        private readonly IReadOnlyList<MatchSlotInfo> Slots;
        private readonly byte MainSlotIdx;

        public PROTOCOL_STAGEROOM_MOVE_SLOT_ACK(IReadOnlyList<MatchSlotInfo> slots, byte mainSlotIdx)
        {
            this.Slots = slots;
            this.MainSlotIdx = mainSlotIdx;
        }

        public override void Write()
        {
            if (this.Slots == null)
                throw new ArgumentNullException(nameof(this.Slots));
            if (this.Slots.Count > MaxSlots)
                throw new ArgumentException("MOVE_SLOT_ACK accepts at most 2 slot records", nameof(this.Slots));

            this.WriteH((short)7950);
            this.WriteC((byte)this.Slots.Count);
            for (int i = 0; i < this.Slots.Count; i++)
                this.WriteMatchSlotInfo(this.Slots[i]);
            this.WriteC(this.MainSlotIdx);
        }
    }
}
