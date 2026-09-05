namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEDAL_INFO_ACK : GameServerPacket
    {
        private static readonly int[,] MedalSteps =
        {
            { 500, 2, 1, 0x3E4CCCCD }, { 700, 3, 1, 0x3D4CCCCD }, { 1000, 5, 1, 0x3E4CCCCD }, { 1300, 8, 1, 0x3D4CCCCD },
            { 1700, 2, 2, 0x3F000000 }, { 2000, 3, 2, 0x3DCCCCCD }, { 2500, 5, 2, 0x3E99999A }, { 3000, 8, 2, 0x3DA3D70A },
            { 4000, 4, 1, 0x40000000 }, { 5000, 7, 1, 0x3E99999A }, { 6000, 2, 3, 0x3F400000 }, { 7000, 3, 3, 0x3E19999A },
            { 8000, 5, 3, 0x3F000000 }, { 10000, 8, 3, 0x3DF5C28F }, { 12000, 4, 2, 0x40400000 }, { 14000, 7, 2, 0x3F000000 },
            { 16000, 1, 1, 0x3E4CCCCD }, { 18000, 2, 4, 0x3F800000 }, { 20000, 3, 4, 0x3E4CCCCD }, { 23000, 5, 4, 0x3F333333 },
            { 25000, 8, 4, 0x3E19999A }, { 28000, 4, 3, 0x40800000 }, { 30000, 7, 3, 0x3F333333 }, { 32000, 1, 2, 0x3F000000 },
            { 35000, 4, 4, 0x40A00000 }, { 37000, 7, 4, 0x3F666666 }, { 40000, 1, 3, 0x3F400000 }, { 43000, 6, 1, 0x3D4CCCCD },
            { 45000, 1, 4, 0x3F800000 }, { 50000, 6, 2, 0x3DCCCCCD }
        };

        public override void Write()
        {
            this.WriteH((short)991);
            this.WriteD(0);
            this.WriteC((byte)0);
            this.WriteMedalBlock(1);
            this.WriteMedalBlock(2);
        }

        private void WriteMedalBlock(int slot)
        {
            int count = MedalSteps.GetLength(0);
            this.WriteD(slot);
            this.WriteD(count);
            for (int i = 0; i < count; i++)
            {
                this.WriteD(MedalSteps[i, 0]);
                this.WriteH((short)MedalSteps[i, 1]);
                this.WriteH((short)MedalSteps[i, 2]);
                this.WriteD(MedalSteps[i, 3]);
            }
        }
    }
}
