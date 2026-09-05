// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK : AuthServerPacket
    {
        private readonly string Field0;

        public PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(string A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)3079);
            this.WriteH((short)0);
            this.WriteD(0);
            this.WriteH((ushort)this.Field0.Length);
            this.WriteN(this.Field0, this.Field0.Length, "UTF-16LE");
            this.WriteD(2);
        }
    }
}