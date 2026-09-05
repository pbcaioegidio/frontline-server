// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_LOBBY_GET_ROOMINFOADD_ACK
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_LOBBY_GET_ROOMINFOADD_ACK : GameServerPacket
    {
        private readonly RoomModel Field0;

        public PROTOCOL_LOBBY_GET_ROOMINFOADD_ACK(RoomModel A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)2568);
            this.WriteC((byte)0);
            this.WriteU(this.Field0.LeaderName, 66);
            this.WriteC((byte)this.Field0.KillTime);
            this.WriteC((byte)(this.Field0.Rounds - 1));
            this.WriteH((ushort)this.Field0.GetInBattleTime());
            this.WriteC(this.Field0.Limit);
            this.WriteC(this.Field0.WatchRuleFlag);
            this.WriteH((ushort)this.Field0.BalanceType);
            this.WriteB(this.Field0.RandomMaps);
            this.WriteC(this.Field0.CountdownIG == 0 ? (byte)5 : (byte)5);
            this.WriteB(this.Field0.LeaderAddr);
            this.WriteC(this.Field0.KillCam);
            this.WriteH((short)0);
            this.WriteB(new byte[68]);
        }
    }
}
