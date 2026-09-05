namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_SHOP_ITEMGROUP_INFO_ACK : AuthServerPacket
    {
        public override void Write()
        {
            WriteH((short)1128);
            WriteC(1);
            WriteD(0);
            WriteD(0);
        }
    }
}
