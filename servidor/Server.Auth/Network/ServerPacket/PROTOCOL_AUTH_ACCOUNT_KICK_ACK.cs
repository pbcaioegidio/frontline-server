// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_AUTH_ACCOUNT_KICK_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_AUTH_ACCOUNT_KICK_ACK : AuthServerPacket
    {
        private readonly int Type;

        public PROTOCOL_AUTH_ACCOUNT_KICK_ACK(int Type)
        {
            this.Type = Type;
        }
        public override void Write()
        {
            WriteH((short)2009);
            WriteB(new byte[19]);
            WriteC((byte)0);
        }
    }
}
