// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_AUTH_GET_POINT_CASH_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Server.Auth.Data.Models;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_AUTH_GET_POINT_CASH_ACK : AuthServerPacket
    {
        private readonly uint Field0;
        private readonly Account Field1;

        public PROTOCOL_AUTH_GET_POINT_CASH_ACK(uint A_1, Account A_2)
        {
            Field0 = A_1;
            Field1 = A_2;
        }

        public override void Write()
        {
            WriteH((short)1061);
            WriteD(Field0);
            WriteD(Field1.Cash);
            WriteD(Field1.Gold);
            WriteD(Field1.Tags);
        }
    }
}
