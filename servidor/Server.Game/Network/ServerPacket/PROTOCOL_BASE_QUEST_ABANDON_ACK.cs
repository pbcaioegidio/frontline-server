namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_QUEST_ABANDON_ACK : GameServerPacket
    {
        public override void Write()
        {
            this.WriteH((short)8708);
            this.WriteD(0);
        }
    }
}
