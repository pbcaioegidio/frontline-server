namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_2478_UNKNOWN_PACKET_REQ : GameClientPacket
    {
        public override void Read()
        {
            ReadD();
        }

        public override void Run()
        {
        }
    }
}
