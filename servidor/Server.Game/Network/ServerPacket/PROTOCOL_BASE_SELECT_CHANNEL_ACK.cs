
namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_SELECT_CHANNEL_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly ushort Field1;
        private readonly uint Field2;

        public PROTOCOL_BASE_SELECT_CHANNEL_ACK(uint A_1, int A_2, int A_3)
        {
            this.Field2 = A_1;
            this.Field0 = A_2;
            this.Field1 = (ushort)A_3;
        }

        public override void Write()
        {
            this.WriteH((short)2335);
            // Reflective S2MO frames carry a 2-byte pad after the opcode, not 4: the ctor at
            // 0xEB5588 derives `H opcode, H pad, D status, ...`, so a WriteD here shifted every
            // following field by 2 bytes.
            this.WriteH(0);
            this.WriteD(this.Field2);
            if (this.Field2 != 0U)
                return;
            this.WriteD(this.Field0);
            this.WriteH(this.Field1);
        }
    }
}
