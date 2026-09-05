namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly int Field1;

        public PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK(int A_1, int A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)3639);
            this.WriteD(this.Field0);
            this.WriteC((byte)this.Field1);
        }
    }
}