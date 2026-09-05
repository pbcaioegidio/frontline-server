// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_URL_LIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
public class PROTOCOL_BASE_URL_LIST_ACK : GameServerPacket
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
