namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK : GameServerPacket
    {
        private readonly int Version;

        public PROTOCOL_AUTH_SHOP_MATCHINGLIST_ACK(int version)
        {
            this.Version = version;
        }

        public override void Write()
        {
            this.WriteH((short)1041);
            this.WriteD(this.Version);
        }
    }
}
