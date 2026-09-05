using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_GET_POINT_CASH_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;

        public PROTOCOL_AUTH_GET_POINT_CASH_ACK(uint A_1, Account A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1061);
            this.WriteD(this.Field0);
            this.WriteD(this.Field1.Cash);
            this.WriteD(this.Field1.Gold);
            this.WriteD(this.Field1.Tags);
        }
    }
}
