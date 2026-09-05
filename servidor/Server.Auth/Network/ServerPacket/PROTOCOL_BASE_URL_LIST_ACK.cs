// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_URL_LIST_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_URL_LIST_ACK : AuthServerPacket
    {
        private readonly ServerConfig Field0;

        public PROTOCOL_BASE_URL_LIST_ACK(ServerConfig A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)2466);
            this.WriteH((short)514);
            this.WriteH((ushort)this.Field0.OfficialBanner.Length);
            this.WriteD(0);
            this.WriteC((byte)2);
            this.WriteN(this.Field0.OfficialBanner, this.Field0.OfficialBanner.Length, "UTF-16LE");
            this.WriteQ(0L);
        }
    }
}