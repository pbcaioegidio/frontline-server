using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_SLOTONEINFO_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly RoomModel Field1;
        private readonly ClanModel Field2;

        public PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(Account A_1)
        {
            this.Field0 = A_1;
            if (A_1 == null)
                return;
            this.Field1 = A_1.Room;
            this.Field2 = ClanManager.GetClan(A_1.ClanId);
        }

        public PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(Account A_1, ClanModel A_2)
        {
            this.Field0 = A_1;
            if (A_1 != null)
                this.Field1 = A_1.Room;
            this.Field2 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)3588);
            this.WriteH((short)0);
            this.WriteC((byte)this.Field1.GetSlot(this.Field0.SlotId).Team);
            this.WriteC(this.Field1.GetSlot(this.Field0.SlotId).GetClientState());
            this.WriteC((byte)this.Field0.GetRank());
            this.WriteD(this.Field2.Id);
            this.WriteD(this.Field0.ClanAccess);
            this.WriteC((byte)this.Field2.Rank);
            this.WriteD(this.Field2.Logo);
            this.WriteC((byte)this.Field0.CafePC);
            this.WriteC((byte)this.Field0.TourneyLevel()); //CountryFlags);
            //this.WriteC((byte)this.Field0.CountryFlags); //CountryFlags);
            this.WriteQ((long)this.Field0.Effects);
            this.WriteC((byte)this.Field2.Effect);
            base.WriteC((byte)this.Field1.GetSlot(this.Field0.SlotId).ViewType);
            this.WriteC((byte)this.NATIONS);
            this.WriteC((byte)0);
            this.WriteD(this.Field0.Equipment.NameCardId);
            this.WriteC((byte)this.Field0.Bonus.NickBorderColor);
            this.WriteC((byte)0);
            this.WriteC((byte)0); // rec+35 pad (client 3595 handler proves clan name starts at +36)
            this.WriteU(this.Field2.Name, 34); // rec+36..69
            this.WriteC((byte)this.Field0.SlotId);
            this.WriteU(this.Field0.Nickname, 66);
            this.WriteC((byte)this.Field0.NickColor);
            this.WriteC((byte)this.Field0.Bonus.MuzzleColor);
            this.WriteC((byte)0);
            this.WriteC(byte.MaxValue);
            this.WriteC(byte.MaxValue);
        }
    }
}