using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;
using Server.Auth.Data.Utils;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_LOGIN_ACK : AuthServerPacket
    {
        private readonly EventErrorEnum Field0;
        private readonly Account Field1;
        private readonly uint Field2;

        public PROTOCOL_BASE_LOGIN_ACK(EventErrorEnum A_1, Account A_2, uint A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)1283);
            this.WriteH((short)0);
            this.WriteD(this.Field2);
            this.WriteB(new byte[] { 0x01, 0x03, 0x00 });
            this.WriteB(new byte[9]);
            this.WriteD(AllUtils.GetFeatures());
            this.WriteH((short)1531);
            this.WriteB(new byte[16 /*0x10*/]);
            this.WriteB(this.Method0(this.Field0, this.Field1));
            this.WriteD((uint)this.Field0);
        }

        
        private byte[] Method0(EventErrorEnum A_1, Account A_2)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1.Equals((object)EventErrorEnum.SUCCESS))
                {
                    syncServerPacket.WriteC((byte)$"{A_2.PlayerId}".Length);
                    syncServerPacket.WriteS($"{A_2.PlayerId}", $"{A_2.PlayerId}".Length);
                    syncServerPacket.WriteC((byte)0);
                    syncServerPacket.WriteC((byte)A_2.Username.Length);
                    syncServerPacket.WriteS(A_2.Username, A_2.Username.Length);
                    syncServerPacket.WriteQ(A_2.PlayerId);
                }
                else
                    syncServerPacket.WriteB(Bitwise.HexStringToByteArray("00 00 00 00 00 00 00 00 00 00 00"));
                return syncServerPacket.ToArray();
            }
        }
    }
}