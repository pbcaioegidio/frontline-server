namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_COMMUNITY_USER_LIST_ACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_COMMUNITY_USER_LIST_ACK(uint A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short) 3855);
            this.WriteD(this.Field0);
        }
    }
}
