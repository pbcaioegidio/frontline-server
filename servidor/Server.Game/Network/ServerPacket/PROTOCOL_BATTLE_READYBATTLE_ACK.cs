namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_READYBATTLE_ACK : GameServerPacket
    {
        private readonly uint Error;

        public PROTOCOL_BATTLE_READYBATTLE_ACK(uint Error) => this.Error = Error;

        public override void Write()
        {
            this.WriteH((short)5124);
            this.WriteH((short)0);
            this.WriteC((byte)0);
            this.WriteD(this.Error);
        }
    }
}