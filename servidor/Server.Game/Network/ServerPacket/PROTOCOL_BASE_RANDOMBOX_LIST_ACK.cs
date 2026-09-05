namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_RANDOMBOX_LIST_ACK : GameServerPacket
    {
        private readonly int Total;
        private readonly int Offset;
        private readonly byte[] Chunk;

        public PROTOCOL_BASE_RANDOMBOX_LIST_ACK()
            : this(0, 0, new byte[0])
        {
        }

        public PROTOCOL_BASE_RANDOMBOX_LIST_ACK(int total, int offset, byte[] chunk)
        {
            this.Total = total;
            this.Offset = offset;
            this.Chunk = chunk ?? new byte[0];
        }

        public override void Write()
        {
            base.WriteH(2501);
            base.WriteD(this.Total);
            base.WriteD(this.Offset);
            base.WriteD(this.Chunk.Length);
            base.WriteB(this.Chunk);
        }
    }
}
