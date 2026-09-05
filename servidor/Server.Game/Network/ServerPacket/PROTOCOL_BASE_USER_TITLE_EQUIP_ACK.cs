// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_USER_TITLE_EQUIP_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
public class PROTOCOL_BASE_USER_TITLE_EQUIP_ACK : GameServerPacket
{
  private readonly uint Field0;
  private readonly int Field1;
  private readonly int Field2;

  public PROTOCOL_BASE_USER_TITLE_EQUIP_ACK(uint A_1, int A_2, int A_3)
  {
    this.Field0 = A_1;
    this.Field1 = A_2;
    this.Field2 = A_3;
  }

        public override void Write()
        {
            this.WriteH((short)2379);
            this.WriteD(this.Field0);
            this.WriteC((byte)this.Field1);
            this.WriteC((byte)this.Field2);
        }
    }
}
