// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_GAMEGUARD_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_GAMEGUARD_ACK : AuthServerPacket
    {
        private readonly int Field0;
        private readonly byte[] Field1;

        public PROTOCOL_BASE_GAMEGUARD_ACK(int A_1, byte[] A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)2312);
            this.WriteB(this.Field1);
            this.WriteD(this.Field0);
        }
    }
}