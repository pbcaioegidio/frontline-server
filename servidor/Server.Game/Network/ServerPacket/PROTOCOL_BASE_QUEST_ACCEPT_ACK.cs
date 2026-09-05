namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_QUEST_ACCEPT_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)8706);
            this.WriteD(0);
        }
    }
}
