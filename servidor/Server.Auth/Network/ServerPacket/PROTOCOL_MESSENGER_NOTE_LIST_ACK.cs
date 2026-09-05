// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_MESSENGER_NOTE_LIST_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll


namespace Server.Auth.Network.ServerPacket
{
public class PROTOCOL_MESSENGER_NOTE_LIST_ACK : AuthServerPacket
{
  private readonly int Field0;
  private readonly int Field1;
  private readonly byte[] Field2;
  private readonly byte[] Field3;

  public PROTOCOL_MESSENGER_NOTE_LIST_ACK(int A_1, int A_2, byte[] A_3, byte[] A_4)
  {
    this.Field1 = A_1;
    this.Field0 = A_2;
    this.Field2 = A_3;
    this.Field3 = A_4;
  }

        public override void Write()
        {
            this.WriteH((short)1925);
            this.WriteC((byte)this.Field0);
            this.WriteC((byte)this.Field1);
            this.WriteB(this.Field2);
            this.WriteB(this.Field3);
        }
    }
}
