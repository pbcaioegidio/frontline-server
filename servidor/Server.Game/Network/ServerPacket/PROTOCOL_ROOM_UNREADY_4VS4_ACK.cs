namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_UNREADY_4VS4_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)3624);
            this.WriteD(0);
        }
    }
}