// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_NOTICE_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Models;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
public class PROTOCOL_BASE_NOTICE_ACK : AuthServerPacket
{
  private readonly ServerConfig Field0;
  private readonly string Field1;
  private readonly string Field2;

  public PROTOCOL_BASE_NOTICE_ACK(ServerConfig A_1)
  {
    this.Field0 = A_1;
    if (A_1 == null)
      return;
    this.Field1 = Translation.GetLabel(A_1.ChannelAnnounce);
    this.Field2 = Translation.GetLabel(A_1.ChatAnnounce);
  }

        
        public override void Write()
        {
            this.WriteH((short)2455);
            this.WriteH((short)0);
            this.WriteD(this.Field0.ChatAnnounceColor);
            this.WriteD(this.Field0.ChannelAnnounceColor);
            this.WriteH((short)0);
            this.WriteH((ushort)this.Field2.Length);
            this.WriteN(this.Field2, this.Field2.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Length);
            this.WriteN(this.Field1, this.Field1.Length, "UTF-16LE");
        }
    }
}
