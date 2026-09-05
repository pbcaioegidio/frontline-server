// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_MATCH_SERVER_IDX_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_MATCH_SERVER_IDX_ACK : AuthServerPacket
    {
        private readonly short Field0;

        public PROTOCOL_MATCH_SERVER_IDX_ACK(short A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)7682);
            this.WriteH((short)0);
        }
    }
}