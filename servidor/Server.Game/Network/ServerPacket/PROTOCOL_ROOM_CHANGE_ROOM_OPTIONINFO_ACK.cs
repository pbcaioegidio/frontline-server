using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)3637);
            this.WriteC((byte)0);
            this.WriteU(this.Field0.LeaderName, 66);
            this.WriteD(this.Field0.KillTime);
            this.WriteC(this.Field0.Limit);
            this.WriteC(this.Field0.WatchRuleFlag);
            this.WriteH((ushort)this.Field0.BalanceType);
            this.WriteB(this.Field0.RandomMaps);
            this.WriteC(this.Field0.CountdownIG == 0 ? (byte)5 : this.Field0.CountdownIG);
            this.WriteB(this.Field0.LeaderAddr);
            this.WriteC(this.Field0.KillCam);
            this.WriteH((short)0);
            this.WriteB(new byte[68]);
        }
    }
}
