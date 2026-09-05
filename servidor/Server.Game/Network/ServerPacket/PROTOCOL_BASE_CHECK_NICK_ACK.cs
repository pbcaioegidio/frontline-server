namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_CHECK_NICK_ACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_BASE_CHECK_NICK_ACK(uint A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)1065);
            this.WriteD(this.Field0);
        }
    }
}
