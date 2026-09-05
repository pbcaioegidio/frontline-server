// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_INVITED_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
public class PROTOCOL_SERVER_MESSAGE_INVITED_ACK : GameServerPacket
{
  private readonly Account Field0;
  private readonly RoomModel Field1;

  public PROTOCOL_SERVER_MESSAGE_INVITED_ACK(Account A_1, RoomModel A_2)
  {
    this.Field0 = A_1;
    this.Field1 = A_2;
  }

        public override void Write()
        {
            this.WriteH((short)3077);
            this.WriteU(this.Field0.Nickname, 66);
            this.WriteD(this.Field1.RoomId);
            this.WriteQ(this.Field0.PlayerId);
            this.WriteS(this.Field1.Password, 4);
        }
    }
}
