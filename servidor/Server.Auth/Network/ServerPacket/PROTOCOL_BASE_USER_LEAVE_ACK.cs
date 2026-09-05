// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_USER_LEAVE_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_USER_LEAVE_ACK : AuthServerPacket
    {
        private readonly int Field0;

        public PROTOCOL_BASE_USER_LEAVE_ACK(int A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)2329);
            this.WriteD(this.Field0);
            this.WriteH((short)0);
        }
    }
}