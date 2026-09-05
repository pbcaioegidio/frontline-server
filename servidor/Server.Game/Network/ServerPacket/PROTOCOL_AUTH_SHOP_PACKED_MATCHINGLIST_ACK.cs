namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_PACKED_MATCHINGLIST_ACK : GameServerPacket
    {
        private readonly int Total;
        private readonly int Offset;
        private readonly byte[] Chunk;

        public PROTOCOL_AUTH_SHOP_PACKED_MATCHINGLIST_ACK(int total, int offset, byte[] chunk)
        {
            this.Total = total;
            this.Offset = offset;
            this.Chunk = chunk ?? new byte[0];
        }

        public override void Write()
        {
            this.WriteH((short)1042);
            this.WriteD(this.Total);
            this.WriteD(this.Offset);
            this.WriteD(this.Chunk.Length);
            this.WriteB(this.Chunk);
        }
    }
}
